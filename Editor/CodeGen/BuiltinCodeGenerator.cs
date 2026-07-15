using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UniVue.UI;

namespace UniVue.Editor.CodeGen
{
    /// <summary>
    /// 内置生成器按照继承的风格生成UI代码。
    /// UIType 仅覆盖 GameObject、RectTransform 以及 GameObject/UI 菜单中的常用组件。
    /// </summary>
    public sealed class BuiltinCodeGenerator : UICodeGenerator
    {
        private enum UIType : byte
        {
            GameObject = 0,
            RectTransform = 1,
            Image = 2,
            RawImage = 3,
            Text = 4,
            Button = 5,
            Toggle = 6,
            Slider = 7,
            Scrollbar = 8,
            ScrollRect = 9,
            Dropdown = 10,
            InputField = 11,
            Canvas = 12,
            BaseUI = 13,
        }

        private const char Separator = '&';
        private const int IndentSize = 4;
        private const string BaseCommentPrefix = "// Base: ";

        private static readonly UTF8Encoding Utf8NoBom = new(false);
        private static readonly StringBuilder SharedBuilder = new(2048);

        private static readonly HashSet<string> CSharpKeywords = new(StringComparer.Ordinal)
        {
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked", "class",
            "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum", "event",
            "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto", "if",
            "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace", "new",
            "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
            "readonly", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static",
            "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong",
            "unchecked", "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
        };

        // 长后缀优先，避免 Input 抢在 InputField 之前匹配等
        private static readonly (string Suffix, UIType Type)[] SuffixRules =
        {
            ("RectTransform", UIType.RectTransform),
            ("GameObject", UIType.GameObject),
            ("ScrollView", UIType.ScrollRect),
            ("ScrollRect", UIType.ScrollRect),
            ("InputField", UIType.InputField),
            ("RawImage", UIType.RawImage),
            ("Scrollbar", UIType.Scrollbar),
            ("Dropdown", UIType.Dropdown),
            ("Button", UIType.Button),
            ("Toggle", UIType.Toggle),
            ("Slider", UIType.Slider),
            ("Canvas", UIType.Canvas),
            ("Image", UIType.Image),
            ("Panel", UIType.Image),
            ("Label", UIType.Text),
            ("Scroll", UIType.ScrollRect),
            ("RawImg", UIType.RawImage),
            ("Input", UIType.InputField),
            ("Drop", UIType.Dropdown),
            ("Text", UIType.Text),
            ("Icon", UIType.Image),
            ("Rect", UIType.RectTransform),
            ("Btn", UIType.Button),
            ("Tgl", UIType.Toggle),
            ("Sld", UIType.Slider),
            ("Scb", UIType.Scrollbar),
            ("Txt", UIType.Text),
            ("Lab", UIType.Text),
            ("Img", UIType.Image),
            ("Rtf", UIType.RectTransform),
            ("Obj", UIType.GameObject),
            ("Ipt", UIType.InputField),
            ("Go", UIType.GameObject),
            ("Sr", UIType.ScrollRect),
        };

        private static readonly string[] DeclaredTypeNames =
        {
            "UnityEngine.GameObject",          // GameObject
            "UnityEngine.RectTransform",       // RectTransform
            "UnityEngine.UI.Image",            // Image
            "UnityEngine.UI.RawImage",         // RawImage
            // Text / Dropdown / InputField：声明为 TMP 与 Legacy 的公共基类，避免回退赋值类型不兼容
            "UnityEngine.UI.MaskableGraphic",  // Text (TMP_Text / UI.Text)
            "UnityEngine.UI.Button",           // Button
            "UnityEngine.UI.Toggle",           // Toggle
            "UnityEngine.UI.Slider",           // Slider
            "UnityEngine.UI.Scrollbar",        // Scrollbar
            "UnityEngine.UI.ScrollRect",       // ScrollRect
            "UnityEngine.UI.Selectable",       // Dropdown (TMP_Dropdown / UI.Dropdown)
            "UnityEngine.UI.Selectable",       // InputField (TMP_InputField / UI.InputField)
            "UnityEngine.Canvas",              // Canvas
        };

        // 单类型组件的 GetComponent 目标（与 DeclaredTypeNames 对齐；多类型链单独处理）
        private static readonly string[] SingleComponentTypes =
        {
            null,                              // GameObject
            "UnityEngine.RectTransform",
            "UnityEngine.UI.Image",
            "UnityEngine.UI.RawImage",
            null,                              // Text → 链
            "UnityEngine.UI.Button",
            "UnityEngine.UI.Toggle",
            "UnityEngine.UI.Slider",
            "UnityEngine.UI.Scrollbar",
            "UnityEngine.UI.ScrollRect",
            null,                              // Dropdown → 链
            null,                              // InputField → 链
            "UnityEngine.Canvas",
        };

        private static readonly string[] TextComponentChain = { "TMPro.TMP_Text", "UnityEngine.UI.Text" };
        private static readonly string[] InputFieldComponentChain = { "TMPro.TMP_InputField", "UnityEngine.UI.InputField" };
        private static readonly string[] DropdownComponentChain = { "TMPro.TMP_Dropdown", "UnityEngine.UI.Dropdown" };

        private struct BindingInfo
        {
            public string PropertyName;
            public string UIPath;
            public UIType UIType;
            public string CustomTypeFullName;
        }

        private sealed class ExtraData
        {
            public readonly List<BindingInfo> Bindings = new(16);
            public readonly HashSet<string> PropertyNames = new(16, StringComparer.Ordinal);
        }

        private ExtraData Extra => (ExtraData)File.customExtraData;

        protected override bool IfSkipDescendant(GameObject ui, string uiPath)
        {
            return ui.TryGetComponent(out BaseUI uiScript) && uiScript != Script;
        }

        protected override string GetGeneratedFileNameWithoutExtension(string sourceFileName)
        {
            return string.Concat("UI", sourceFileName);
        }

        protected override void OnFileInitialized(UIGeneratedFile file)
        {
            file.customExtraData = new ExtraData();
        }

        protected override void OnVisitUIHierarchy(GameObject ui, string uiPath)
        {
            ExtraData extra = Extra;

            // 嵌套 BaseUI：生成对该脚本的引用属性，不再按命名后缀拆分
            if (ui.TryGetComponent(out BaseUI nestedUI) && nestedUI != Script)
            {
                TryAddBinding(extra, ui, uiPath, ToValidIdentifier(ui.name), UIType.BaseUI,
                    GetCodeTypeFullName(nestedUI.GetType()));
                return;
            }

            string goName = ui.name;
            int amp = goName.IndexOf(Separator);
            if (amp < 0)
            {
                TryProcessSegment(extra, ui, uiPath, goName);
                return;
            }

            // 手动分段，避免 Split 分配 string[]
            int start = 0;
            int length = goName.Length;
            for (int i = 0; i <= length; i++)
            {
                if (i != length && goName[i] != Separator)
                    continue;

                if (i > start)
                    TryProcessSegment(extra, ui, uiPath, goName, start, i - start);

                start = i + 1;
            }
        }

        private static void TryProcessSegment(ExtraData extra, GameObject ui, string uiPath, string name)
        {
            name = TrimInPlace(name);
            if (name.Length == 0)
                return;

            if (!TryMatchUIType(name, out UIType uiType))
                return;

            TryAddBinding(extra, ui, uiPath, ToValidIdentifier(name), uiType, ResolveConcreteType(ui, uiType));
        }

        private static void TryProcessSegment(ExtraData extra, GameObject ui, string uiPath, string source, int start,
                                             int count)
        {
            // 去首尾空白后得到子串（仅在需要时分配）
            int end = start + count;
            while (start < end && char.IsWhiteSpace(source[start]))
                start++;
            while (end > start && char.IsWhiteSpace(source[end - 1]))
                end--;

            if (end <= start)
                return;

            string name = source.Substring(start, end - start);
            if (!TryMatchUIType(name, out UIType uiType))
                return;

            TryAddBinding(extra, ui, uiPath, ToValidIdentifier(name), uiType, ResolveConcreteType(ui, uiType));
        }

        /// <summary>
        /// 对 Text / Dropdown / InputField 这类 TMP/Legacy 二选一的类型，
        /// 根据预制体上实际挂载的组件解析出具体类型全名（如 TMPro.TMP_Text）。
        /// 解析不到时返回 null，走公共基类 + 运行时回退链。
        /// </summary>
        private static string ResolveConcreteType(GameObject ui, UIType uiType)
        {
            if (!TryGetComponentChain(uiType, out string[] chain))
                return null;

            Component[] comps = ui.GetComponents<Component>();
            for (int c = 0; c < chain.Length; c++)
            {
                for (int i = 0; i < comps.Length; i++)
                {
                    Component comp = comps[i];
                    if (comp != null && TypeInheritsFullName(comp.GetType(), chain[c]))
                        return chain[c];
                }
            }

            return null;
        }

        private static bool TypeInheritsFullName(Type type, string fullName)
        {
            for (Type t = type; t != null; t = t.BaseType)
            {
                if (t.FullName == fullName)
                    return true;
            }

            return false;
        }

        /// <summary>无首尾空白时返回原引用，避免 Trim 分配。</summary>
        private static string TrimInPlace(string name)
        {
            if (name == null || name.Length == 0)
                return string.Empty;

            int start = 0;
            int end = name.Length;
            while (start < end && char.IsWhiteSpace(name[start]))
                start++;
            while (end > start && char.IsWhiteSpace(name[end - 1]))
                end--;

            if (start == 0 && end == name.Length)
                return name;
            if (end <= start)
                return string.Empty;
            return name.Substring(start, end - start);
        }

        private static void TryAddBinding(ExtraData extra, GameObject ui, string uiPath, string propertyName,
                                          UIType uiType, string customTypeFullName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return;

            if (!extra.PropertyNames.Add(propertyName))
            {
                Debug.LogWarning(
                    $"Duplicate property name '{propertyName}' found in UI '{uiPath}'. This property will be ignored.",
                    ui);
                return;
            }

            extra.Bindings.Add(new BindingInfo
            {
                PropertyName = propertyName,
                UIPath = uiPath,
                UIType = uiType,
                CustomTypeFullName = customTypeFullName,
            });
        }

        protected override void Generate(UIGeneratedFile file)
        {
            ExtraData extra = (ExtraData)file.customExtraData;
            List<BindingInfo> bindings = extra.Bindings;
            if (bindings.Count == 0)
            {
                Debug.Log(
                    $"No UI properties matched naming rules for '{file.SourceType}' (asset: {file.AssetPath}). Code file will not be generated.");
                return;
            }

            bindings.Sort(CompareBindingByPropertyName);

            Type sourceType = file.SourceType;
            if (sourceType == null)
            {
                Debug.LogError("Source type is null. Code generation skipped.");
                return;
            }

            string ns = sourceType.Namespace;
            bool hasNamespace = !string.IsNullOrEmpty(ns);
            string generatedFullName = hasNamespace ? string.Concat(ns, ".", file.FileName) : file.FileName;

            string baseTypeName = ResolveBaseTypeName(sourceType, generatedFullName, file.FilePath);
            if (string.IsNullOrEmpty(baseTypeName))
            {
                Debug.LogError($"Source type '{sourceType}' has no valid base type. Code generation skipped.");
                return;
            }

            StringBuilder sb = SharedBuilder;
            sb.Clear();
            // 粗估容量，减少扩容
            int estimated = 256 + bindings.Count * 180 + (file.FilePath?.Length ?? 0);
            if (sb.Capacity < estimated)
                sb.Capacity = estimated;

            int indent = 0;
            AppendLine(sb, indent, "// <auto-generated/>");
            sb.Append("// File: ").Append(file.FilePath).AppendLine();
            sb.Append("// Source: ").Append(sourceType).AppendLine();
            sb.Append("// Asset: ").Append(file.AssetPath).AppendLine();
            // 记录本次继承的基类，供下次源类型继承本生成类后解析沿用
            sb.Append(BaseCommentPrefix).Append(baseTypeName).AppendLine();
            sb.AppendLine();

            if (hasNamespace)
            {
                sb.Append("namespace ").Append(ns).AppendLine();
                AppendLine(sb, indent, "{");
                indent++;
            }

            AppendIndent(sb, indent);
            sb.Append("public abstract class ").Append(file.FileName).Append(" : ")
              .Append(baseTypeName).AppendLine();
            AppendLine(sb, indent, "{");
            indent++;

            for (int i = 0; i < bindings.Count; i++)
            {
                BindingInfo binding = bindings[i];
                string typeName = GetDeclaredTypeFullName(binding);
                AppendIndent(sb, indent);
                sb.Append("private ").Append(typeName).Append(" _g").Append(binding.PropertyName).AppendLine(";");
                AppendIndent(sb, indent);
                sb.Append("protected ").Append(typeName).Append(' ').Append(binding.PropertyName)
                  .Append(" => _g").Append(binding.PropertyName).AppendLine(";");
                sb.AppendLine();
            }

            AppendLine(sb, indent, "protected override void OnCreate()");
            AppendLine(sb, indent, "{");
            indent++;
            AppendLine(sb, indent, "base.OnCreate();");
            for (int i = 0; i < bindings.Count; i++)
                AppendInitializeBinding(sb, indent, bindings[i]);
            indent--;
            AppendLine(sb, indent, "}");

            indent--;
            AppendLine(sb, indent, "}");

            if (hasNamespace)
            {
                indent--;
                AppendLine(sb, indent, "}");
            }

            string dir = Path.GetDirectoryName(file.FilePath);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);

            System.IO.File.WriteAllText(file.FilePath, sb.ToString(), Utf8NoBom);
        }

        /// <summary>
        /// 解析本次生成应继承的基类：
        /// 1) 源类型尚未继承生成类时，直接取源类型的当前基类；
        /// 2) 源类型已继承上次生成的类时，沿用上次记录（读取 .g.cs 中的 "// Base:" 注释），
        ///    读取失败则回退为生成类的基类（等价于原始基类），避免出现自继承。
        /// </summary>
        private static string ResolveBaseTypeName(Type sourceType, string generatedFullName, string filePath)
        {
            Type currentBase = sourceType.BaseType;
            if (currentBase == null || currentBase == typeof(object))
                return null;

            // 源类型已经继承自上次生成的类
            if (GetCodeTypeFullName(currentBase) == generatedFullName)
            {
                string recorded = TryReadRecordedBaseType(filePath);
                if (!string.IsNullOrEmpty(recorded))
                    return recorded;

                Type originalBase = currentBase.BaseType;
                if (originalBase != null && originalBase != typeof(object))
                    return GetCodeTypeFullName(originalBase);

                return null;
            }

            return GetCodeTypeFullName(currentBase);
        }

        /// <summary>
        /// 从已存在的生成文件头部注释解析上一次记录的基类全名（"// Base: xxx"）。
        /// </summary>
        private static string TryReadRecordedBaseType(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
                return null;

            try
            {
                foreach (string line in System.IO.File.ReadLines(filePath))
                {
                    string trimmed = line.TrimStart();
                    if (trimmed.StartsWith(BaseCommentPrefix, StringComparison.Ordinal))
                        return trimmed.Substring(BaseCommentPrefix.Length).Trim();

                    // 已进入代码区（命名空间/类声明）仍未找到，则停止扫描
                    if (trimmed.StartsWith("namespace", StringComparison.Ordinal) ||
                        trimmed.StartsWith("public", StringComparison.Ordinal))
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"读取生成文件基类注释失败：{filePath}\n{ex.Message}");
            }

            return null;
        }

        private static int CompareBindingByPropertyName(BindingInfo a, BindingInfo b)
        {
            return string.CompareOrdinal(a.PropertyName, b.PropertyName);
        }

        private static void AppendInitializeBinding(StringBuilder sb, int indent, BindingInfo binding)
        {
            AppendIndent(sb, indent);

            if (binding.UIType == UIType.GameObject)
            {
                sb.Append("_g").Append(binding.PropertyName).Append(" = FindByPath(\"");
                AppendEscaped(sb, binding.UIPath);
                sb.AppendLine("\");");
                return;
            }

            // 未能在预制体上解析出具体类型时，声明公共基类并生成运行时回退链
            if (binding.CustomTypeFullName == null && TryGetComponentChain(binding.UIType, out string[] chain))
            {
                // 优先 TMP，再回退过时 UGUI
                string goVar = string.Concat("__go_", binding.PropertyName);
                sb.Append("UnityEngine.GameObject ").Append(goVar).Append(" = FindByPath(\"");
                AppendEscaped(sb, binding.UIPath);
                sb.AppendLine("\");");

                AppendIndent(sb, indent);
                sb.Append("_g").Append(binding.PropertyName).Append(" = ").Append(goVar)
                  .Append(" != null ? ").Append(goVar).Append(".GetComponent<").Append(chain[0])
                  .AppendLine(">() : null;");

                for (int i = 1; i < chain.Length; i++)
                {
                    AppendIndent(sb, indent);
                    sb.Append("if (_g").Append(binding.PropertyName).Append(" == null && ").Append(goVar)
                      .AppendLine(" != null)");
                    AppendLine(sb, indent, "{");
                    AppendIndent(sb, indent + 1);
                    sb.Append("_g").Append(binding.PropertyName).Append(" = ").Append(goVar)
                      .Append(".GetComponent<").Append(chain[i]).AppendLine(">();");
                    AppendLine(sb, indent, "}");
                }

                return;
            }

            // 已解析出具体类型（BaseUI 或 TMP/Legacy 实际组件）时，直接单次 GetComponent
            string componentType = binding.CustomTypeFullName ?? SingleComponentTypes[(int)binding.UIType];

            sb.Append("_g").Append(binding.PropertyName).Append(" = FindByPath(\"");
            AppendEscaped(sb, binding.UIPath);
            sb.Append("\").GetComponent<").Append(componentType).AppendLine(">();");
        }

        private static bool TryGetComponentChain(UIType uiType, out string[] chain)
        {
            switch (uiType)
            {
                case UIType.Text:
                    chain = TextComponentChain;
                    return true;
                case UIType.InputField:
                    chain = InputFieldComponentChain;
                    return true;
                case UIType.Dropdown:
                    chain = DropdownComponentChain;
                    return true;
                default:
                    chain = null;
                    return false;
            }
        }

        /// <summary>
        /// 忽略大小写匹配后缀；CloseBtn / close_btn / Close_Btn 均可（EndsWith 已覆盖下划线形式）。
        /// </summary>
        private static bool TryMatchUIType(string name, out UIType uiType)
        {
            int nameLen = name.Length;
            for (int i = 0; i < SuffixRules.Length; i++)
            {
                string suffix = SuffixRules[i].Suffix;
                int suffixLen = suffix.Length;
                if (nameLen < suffixLen)
                    continue;

                if (name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                {
                    uiType = SuffixRules[i].Type;
                    return true;
                }
            }

            uiType = default;
            return false;
        }

        private static string GetDeclaredTypeFullName(BindingInfo binding)
        {
            // BaseUI 或已从预制体解析出的具体 TMP/Legacy 类型
            if (binding.CustomTypeFullName != null)
                return binding.CustomTypeFullName;
            return DeclaredTypeNames[(int)binding.UIType];
        }

        private static string GetCodeTypeFullName(Type type)
        {
            if (type == null)
                return "object";

            if (!type.IsGenericType)
            {
                string fullName = type.FullName ?? type.Name;
                return fullName.IndexOf('+') >= 0 ? fullName.Replace('+', '.') : fullName;
            }

            string name = type.GetGenericTypeDefinition().FullName;
            int tick = name.IndexOf('`');
            if (tick >= 0)
                name = name.Substring(0, tick);
            if (name.IndexOf('+') >= 0)
                name = name.Replace('+', '.');

            Type[] args = type.GetGenericArguments();
            var sb = new StringBuilder(name.Length + 16 * args.Length);
            sb.Append(name).Append('<');
            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0)
                    sb.Append(", ");
                sb.Append(GetCodeTypeFullName(args[i]));
            }

            sb.Append('>');
            return sb.ToString();
        }

        /// <summary>
        /// 将 GameObject 名称转为合法的 C# 属性标识符；已合法时零分配返回原串。
        /// </summary>
        private static string ToValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "_";

            if (IsAlreadyValidIdentifier(name))
                return name;

            int len = name.Length;
            char[] buffer = new char[len + 1]; // +1 可能前缀 _
            int w = 0;
            bool lastUnderscore = false;

            for (int i = 0; i < len; i++)
            {
                char c = name[i];
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    if (c == '_')
                    {
                        if (lastUnderscore)
                            continue;
                        lastUnderscore = true;
                    }
                    else
                    {
                        lastUnderscore = false;
                    }

                    buffer[w++] = c;
                }
                else
                {
                    if (lastUnderscore)
                        continue;
                    buffer[w++] = '_';
                    lastUnderscore = true;
                }
            }

            // 去掉首尾多余下划线（保留非法首字符场景下的单个前导 _）
            int start = 0;
            int end = w;
            while (end > start && buffer[end - 1] == '_')
                end--;

            if (end <= start)
                return "_";

            bool needsPrefix = char.IsDigit(buffer[start]);
            if (needsPrefix)
            {
                // 已有空间在 buffer[0]，右移
                for (int i = end - 1; i >= start; i--)
                    buffer[i + 1] = buffer[i];
                buffer[start] = '_';
                end++;
            }

            string result = new string(buffer, start, end - start);
            if (CSharpKeywords.Contains(result))
                return string.Concat(result, "_");
            return result;
        }

        private static bool IsAlreadyValidIdentifier(string name)
        {
            char first = name[0];
            if (!(char.IsLetter(first) || first == '_'))
                return false;

            for (int i = 1; i < name.Length; i++)
            {
                char c = name[i];
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                    return false;
                if (c == '_' && name[i - 1] == '_')
                    return false;
            }

            return !CSharpKeywords.Contains(name);
        }

        private static void AppendEscaped(StringBuilder sb, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            // 路径几乎不含转义字符，快路径零分配扫描
            if (value.IndexOfAny(EscapeChars) < 0)
            {
                sb.Append(value);
                return;
            }

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '\\' || c == '"')
                    sb.Append('\\');
                sb.Append(c);
            }
        }

        private static readonly char[] EscapeChars = { '\\', '"' };

        private static void AppendIndent(StringBuilder sb, int indent)
        {
            if (indent > 0)
                sb.Append(' ', indent * IndentSize);
        }

        private static void AppendLine(StringBuilder sb, int indent, string line)
        {
            AppendIndent(sb, indent);
            sb.AppendLine(line);
        }
    }
}
