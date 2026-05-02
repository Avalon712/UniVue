using System.IO;
using UnityEditor;
using UnityEngine;

namespace UniVue.Editor.Misc
{
    /// <summary>
    /// 在工程中创建 UI 脚本骨架（BaseView / BaseComponent / BaseUI）。
    /// </summary>
    public static class UIScriptCreator
    {
        private const string DefaultBaseName = "NewUIScript";

        [MenuItem("UniVue/Create C# UI Script/BaseView", false, 10)]
        [MenuItem("Assets/UniVue/Create C# UI Script/BaseView", false, 10)]
        public static void CreateBaseViewScript()
        {
            CreateScriptFile(UIScriptKind.BaseView);
        }

        [MenuItem("UniVue/Create C# UI Script/BaseComponent", false, 11)]
        [MenuItem("Assets/UniVue/Create C# UI Script/BaseComponent", false, 11)]
        public static void CreateBaseComponentScript()
        {
            CreateScriptFile(UIScriptKind.BaseComponent);
        }

        [MenuItem("UniVue/Create C# UI Script/BaseUI", false, 12)]
        [MenuItem("Assets/UniVue/Create C# UI Script/BaseUI", false, 12)]
        public static void CreateBaseUIScript()
        {
            CreateScriptFile(UIScriptKind.BaseUI);
        }

        private static void CreateScriptFile(UIScriptKind kind)
        {
            string folder = GetSelectedFolderAssetPath();
            if (!TryGetUniqueScriptAssetPath(folder, DefaultBaseName, out string assetPath, out string className))
            {
                EditorUtility.DisplayDialog("Create C# UI Script", "无法生成唯一脚本路径。", "确定");
                return;
            }

            string body = BuildScriptContent(kind, className);
            File.WriteAllText(ToAbsoluteAssetPath(assetPath), body);
            AssetDatabase.Refresh();
            Object created = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
            if (created)
            {
                Selection.activeObject = created;
                EditorGUIUtility.PingObject(created);
            }
        }

        private static string GetSelectedFolderAssetPath()
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (AssetDatabase.IsValidFolder(path))
                return path;
            if (!string.IsNullOrEmpty(path))
            {
                string parent = Path.GetDirectoryName(path);
                return string.IsNullOrEmpty(parent) ? "Assets" : parent.Replace('\\', '/');
            }

            return "Assets";
        }

        private static bool TryGetUniqueScriptAssetPath(
            string folderAssetPath, string baseName, out string assetPath, out string className)
        {
            for (int i = 0; i < 10000; i++)
            {
                className = i == 0 ? baseName : $"{baseName}{i}";
                assetPath = $"{folderAssetPath}/{className}.cs".Replace("//", "/");
                if (!File.Exists(ToAbsoluteAssetPath(assetPath)))
                    return true;
            }

            assetPath = null;
            className = null;
            return false;
        }

        private static string ToAbsoluteAssetPath(string assetPath)
        {
            string relative = assetPath.StartsWith("Assets/")
                ? assetPath["Assets/".Length..]
                : assetPath;
            return Path.GetFullPath(Path.Combine(Application.dataPath, relative));
        }

        private static string BuildScriptContent(UIScriptKind kind, string className)
        {
            string rootNs = EditorSettings.projectGenerationRootNamespace;
            bool useNs = !string.IsNullOrEmpty(rootNs);

            string core = kind switch
            {
                UIScriptKind.BaseView => BuildBaseViewClass(className),
                UIScriptKind.BaseComponent => BuildBaseComponentClass(className),
                _ => BuildBaseUIClass(className)
            };

            if (!useNs)
                return "using UniVue.UI;\n\n" + core;

            return $"using UniVue.UI;\n\nnamespace {rootNs}\n{{\n{Indent(core)}\n}}\n";
        }

        private static string BuildBaseViewClass(string className)
        {
            return
                $@"public partial class {className} : BaseView
{{
    public override int Layer => 0; // 界面层级，数值越大越靠近屏幕前层

    protected override void OnInit()
    {{
        enableUpdate = false; // false：不回调OnUpdate；true：每帧调用OnUpdate
        enableUpdatePerSecond = false; // false：不回调OnUpdatePerSecond；true：每秒调用OnUpdatePerSecond
    }}

    protected override void OnOpen()
    {{
        base.OnOpen();
    }}

    protected override void OnClose()
    {{
        base.OnClose();
    }}

    protected override void OnRelease()
    {{
    }}
}}";
        }

        private static string BuildBaseComponentClass(string className)
        {
            return
                $@"public partial class {className} : BaseComponent
{{
    protected override void OnCreate()
    {{
        enableUpdate = false; // false：不回调OnUpdate；true：每帧调用OnUpdate
        enableUpdatePerSecond = false; // false：不回调OnUpdatePerSecond；true：每秒调用OnUpdatePerSecond
        base.OnCreate();
    }}

    protected override void OnShow()
    {{
        base.OnShow();
    }}

    protected override void OnHide()
    {{
        base.OnHide();
    }}

    protected override void OnDispose()
    {{
        base.OnDispose();
    }}
}}";
        }

        private static string BuildBaseUIClass(string className)
        {
            return
                $@"public class {className} : BaseUI
{{
    protected override void OnCreate()
    {{
        enableUpdate = false; // false：不回调OnUpdate；true：每帧调用OnUpdate
        enableUpdatePerSecond = false; // false：不回调OnUpdatePerSecond；true：每秒调用OnUpdatePerSecond
        base.OnCreate();
    }}

    protected override void OnDispose()
    {{
        base.OnDispose();
    }}
}}";
        }

        private static string Indent(string text)
        {
            string pad = "    ";
            string[] lines = text.Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length > 0)
                    lines[i] = pad + lines[i];
            }

            return string.Join("\n", lines);
        }

        private enum UIScriptKind
        {
            BaseView,
            BaseComponent,
            BaseUI
        }
    }
}