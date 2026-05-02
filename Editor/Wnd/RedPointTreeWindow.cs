using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UniVue.Common;

namespace UniVue.Editor
{
    public enum RedPointRule
    {
        Or,
        And
    }

    public sealed class RedPointTree
    {
        public Node root;

        public sealed class Node
        {
            public List<Node> children = new();
            public string comment;
            public bool enabled = true;
            public string key;
            public RedPointRule rule;
        }
    }

    public sealed class RedPointTreeWindow : EditorWindow
    {
        private const float LeftPanelWidth = 200f;
        private const float SplitWidth = 4f;
        private const float PmListRowHeight = 22f;
        private const float NodeRowSpacing = 0f;
        private const int MaxTreeDepth = 7;

        private static RedPointTreeWindow _window;
        private GUIStyle _boxStyle;
        private string _directoryPath = "";
        private bool _isDirty;
        private int _leftListHoverIndex = -1;
        private Vector2 _leftScroll;
        private GUIStyle _leftScrollStyle;
        private bool _modifiedSinceLastExport = true;
        private string _namespace = "UniVue";
        private bool _namespaceEditable;
        private GUIStyle _pmListLabelStyle;
        private bool _readOnly;
        private GUIStyle _readOnlyLabelStyle;
        private Vector2 _rightScroll;
        private string _searchFilter = "";
        private RedPointTree _selectedTree;
        private bool _showComment;

        private List<RedPointTree> _trees = new();
        // private static string PrefsKeyDirectory => Application.dataPath + "/RedPointTreeWindow.Directory";
        // private static string PrefsKeyNamespace => Application.dataPath + "/RedPointTreeWindow.Namespace";

        private static UniVueEditorSettings settings => UniVueEditorSettings.instance;

        private void OnEnable()
        {
            _trees ??= new List<RedPointTree>();
            _directoryPath = settings.redPointKeyExportDirectory;
            _namespace = settings.redPointKeyNamespace;
            _isDirty = false;
            _modifiedSinceLastExport = true;
            TryImportRedPointKeys();
        }

        private void OnDisable()
        {
            if (_isDirty && _modifiedSinceLastExport)
                ExportRedPointKeys(true);
            _window = null;
            AssetDatabase.SaveAssetIfDirty(settings);
        }

        private void OnGUI()
        {
            _trees ??= new List<RedPointTree>();

            if (_boxStyle == null)
                _boxStyle = new GUIStyle(GUI.skin.box) { padding = new RectOffset(8, 8, 8, 8) };
            if (_leftScrollStyle == null)
            {
                _leftScrollStyle = new GUIStyle(GUI.skin.box)
                    { padding = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0) };
            }

            if (_pmListLabelStyle == null)
            {
                _pmListLabelStyle = new GUIStyle(EditorStyles.label)
                {
                    padding = new RectOffset(12, 8, 2, 2),
                    alignment = TextAnchor.MiddleLeft,
                    clipping = TextClipping.Clip
                };
            }

            if (_readOnlyLabelStyle == null)
            {
                _readOnlyLabelStyle = new GUIStyle(EditorStyles.label)
                    { normal = { textColor = new Color(0.85f, 0.85f, 0.85f) } };
            }

            EditorGUILayout.Space(4);

            DrawToolbar();

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();

            DrawLeftPanel();
            GUILayout.Space(SplitWidth);
            DrawRightPanel();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2);
            string displayPath = !string.IsNullOrEmpty(_directoryPath)
                ? _directoryPath
                : Path.Combine(Application.dataPath, "Scripts");
            string ns = !string.IsNullOrEmpty(_namespace) ? _namespace : "UniVue";
            string bottomText = $"导出目录： {displayPath}    导出C#类（自动生成）：{ns}.RedPointKey";
            EditorGUILayout.LabelField(bottomText, EditorStyles.miniLabel);
        }

        [MenuItem("UniVue/Windows/RedPointTreeEditor (Editor)")]
        public static void ShowWindow()
        {
            if (EditorApplication.isPlaying)
            {
                LogUtil.Warn("游戏运行时不允许编辑红点树，如果要查看红点树结构，请打开红点树运行时查看窗口");
                return;
            }

            if (!_window)
                _window = GetWindow<RedPointTreeWindow>("红点树编辑器");
            _window.Show();
        }

        /// <summary>
        /// 获取所有红点树。窗口已打开时直接返回当前数据；未打开时尝试从 RedPointKey.g.cs 解析；失败返回 null。
        /// </summary>
        public static List<RedPointTree> GetAllTrees()
        {
            if (_window != null && _window._trees != null)
                return _window._trees;
            return TryParseTreesFromFile();
        }

        /// <summary>
        /// 获取所有 Key 及其 ulong 值的映射，用于 PropertyDrawer 等。解析 RedPointKey.g.cs，失败返回 null。
        /// </summary>
        public static Dictionary<string, ulong> GetAllKeysWithValues()
        {
            List<(string name, ulong value, int depth, string comment)> entries = TryParseKeyValueEntriesFromFile();
            if (entries == null) return null;
            Dictionary<string, ulong> dict = new(StringComparer.Ordinal);
            foreach ((string name, ulong value, int _, string _) in entries)
                dict[name] = value;
            return dict;
        }

        private static List<(string name, ulong value, int depth, string comment)> TryParseKeyValueEntriesFromFile()
        {
            string filePath = FindRedPointKeyFilePathStatic();
            if (string.IsNullOrEmpty(filePath)) return null;
            try
            {
                string content = File.ReadAllText(filePath);
                if (!content.Contains("enum RedPointKey") && !content.Contains("enum RedPointKey "))
                    return null;
                return ParseRedPointKeyEnumStatic(content);
            }
            catch
            {
                return null;
            }
        }

        private static List<RedPointTree> TryParseTreesFromFile()
        {
            string filePath = FindRedPointKeyFilePathStatic();
            if (string.IsNullOrEmpty(filePath))
                return null;
            try
            {
                string content = File.ReadAllText(filePath);
                if (!content.Contains("enum RedPointKey") && !content.Contains("enum RedPointKey "))
                    return null;
                List<(string name, ulong value, int depth, string comment)> entries =
                    ParseRedPointKeyEnumStatic(content);
                if (entries.Count == 0)
                    return null;
                return RebuildTreesFromEntriesStatic(entries);
            }
            catch
            {
                return null;
            }
        }

        private static string FindRedPointKeyFilePathStatic()
        {
            string dir = settings.redPointKeyExportDirectory;
            if (string.IsNullOrEmpty(dir))
                dir = Path.Combine(Application.dataPath, "Scripts");
            string path = Path.Combine(dir, "RedPointKey.g.cs");
            if (File.Exists(path))
                return path;
            string assetsDir = Application.dataPath;
            if (!Directory.Exists(assetsDir))
                return null;
            return Directory.GetFiles(assetsDir, "RedPointKey.g.cs", SearchOption.AllDirectories).FirstOrDefault();
        }

        private void MarkDirty()
        {
            _isDirty = true;
            _modifiedSinceLastExport = true;
        }

        private void SaveDirectory()
        {
            settings.redPointKeyExportDirectory = _directoryPath;
        }

        private void SaveNamespace()
        {
            settings.redPointKeyNamespace = _namespace;
        }

        /// <summary>
        /// 查找 RedPointKey.g.cs 文件：优先配置目录，否则在 Assets 下递归搜索
        /// </summary>
        private string FindRedPointKeyFilePath()
        {
            string dir = !string.IsNullOrEmpty(_directoryPath)
                ? _directoryPath
                : Path.Combine(Application.dataPath, "Scripts");
            string path = Path.Combine(dir, "RedPointKey.g.cs");
            if (File.Exists(path))
                return path;
            string assetsDir = Application.dataPath;
            if (!Directory.Exists(assetsDir))
                return null;
            return Directory.GetFiles(assetsDir, "RedPointKey.g.cs", SearchOption.AllDirectories).FirstOrDefault();
        }

        private void ExportRedPointKeys(bool silent = false)
        {
            string dir = !string.IsNullOrEmpty(_directoryPath)
                ? _directoryPath
                : Path.Combine(Application.dataPath, "Scripts");
            if (!Directory.Exists(dir))
            {
                if (!silent) EditorUtility.DisplayDialog("导出失败", $"目录不存在: {dir}", "确定");
                return;
            }

            if (_trees == null || _trees.Count == 0)
            {
                if (!silent) EditorUtility.DisplayDialog("导出失败", "没有可导出的红点树", "确定");
                return;
            }

            StringBuilder sb = new();
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("// 此文件由红点树编辑器自动生成，请勿手动修改。");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine();
            sb.AppendLine($"namespace {_namespace ?? "UniVue"}");
            sb.AppendLine("{");
            sb.AppendLine("    public enum RedPointKey : ulong");
            sb.AppendLine("    {");

            List<(string name, ulong value, string comment, int depth, int treeIndex)> entries = new();
            int treeIndex = 0;
            foreach (RedPointTree tree in _trees)
            {
                if (tree?.root == null) continue;
                CollectExportEntries(tree.root, 0, 0, treeIndex, 0, entries);
                treeIndex++;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                (string name, ulong value, string comment, int depth, int treeIndex) entry = entries[i];
                (string name, ulong value, string comment, int nodeDepth, int entryTreeIndex) = (
                    entry.name, entry.value, entry.comment, entry.depth, entry.treeIndex);
                if (i > 0 && nodeDepth == 0)
                    sb.AppendLine().AppendLine().AppendLine();
                if (!string.IsNullOrWhiteSpace(comment))
                {
                    sb.AppendLine("        /// <summary>");
                    foreach (string line in comment.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        sb.AppendLine("        /// " + EscapeXmlComment(line.Trim()));
                    sb.AppendLine("        /// </summary>");
                }

                string valueStr = FormatBinaryValue(value, nodeDepth);
                sb.AppendLine($"        {SanitizeEnumName(name)} = {valueStr},");
                if (i < entries.Count - 1 && !(nodeDepth == 0 && i + 1 < entries.Count && entries[i + 1].depth == 0))
                    sb.AppendLine();
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            string filePath = Path.Combine(dir, "RedPointKey.g.cs");
            try
            {
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                AssetDatabase.Refresh();
                _modifiedSinceLastExport = false;
                if (!silent) EditorUtility.DisplayDialog("导出成功", $"已生成: {filePath}", "确定");
            }
            catch (Exception ex)
            {
                if (!silent) EditorUtility.DisplayDialog("导出失败", ex.Message, "确定");
            }
        }

        private void TryImportRedPointKeys()
        {
            if (!TryImportRedPointKeysInternal(out List<RedPointTree> trees, out string ns))
                return;
            _trees = trees;
            _selectedTree = _trees.Count > 0 ? _trees[0] : null;
            if (!string.IsNullOrEmpty(ns))
            {
                _namespace = ns;
                SaveNamespace();
            }
        }

        private bool TryImportRedPointKeysInternal(out List<RedPointTree> trees, out string namespaceExtracted)
        {
            trees = new List<RedPointTree>();
            namespaceExtracted = null;
            string filePath = FindRedPointKeyFilePath();
            if (string.IsNullOrEmpty(filePath))
                return false;
            if (string.IsNullOrEmpty(_directoryPath))
            {
                _directoryPath = Path.GetDirectoryName(filePath);
                SaveDirectory();
            }

            try
            {
                string content = File.ReadAllText(filePath);
                if (!content.Contains("enum RedPointKey") && !content.Contains("enum RedPointKey "))
                    return false;
                namespaceExtracted = ExtractNamespaceFromContent(content);
                List<(string name, ulong value, int depth, string comment)> entries =
                    ParseRedPointKeyEnumStatic(content);
                if (entries.Count == 0)
                    return false;
                trees = RebuildTreesFromEntriesStatic(entries);
                return trees.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        private static string ExtractNamespaceFromContent(string content)
        {
            Match m = Regex.Match(content, @"namespace\s+([\w.]+)\s*\{");
            return m.Success ? m.Groups[1].Value.Trim() : null;
        }

        private static List<(string name, ulong value, int depth, string comment)> ParseRedPointKeyEnumStatic(
            string content)
        {
            List<(string name, ulong value, int depth, string comment)> entries = new();
            string comment = "";
            string[] lines = content.Split('\r', '\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (line.TrimStart().StartsWith("///"))
                {
                    string c = line.TrimStart().Substring(3).Trim();
                    if (!string.IsNullOrEmpty(c) && !c.StartsWith("<"))
                        comment += (comment.Length > 0 ? " " : "") + c;
                    continue;
                }

                Match m = Regex.Match(line, @"^\s*(\w+)\s*=\s*0b([01_]+)U?L?\s*,?\s*$");
                if (m.Success)
                {
                    string name = m.Groups[1].Value;
                    string binWithUnderscores = m.Groups[2].Value;
                    string[] groups = binWithUnderscores.Split('_');
                    if (groups.Length == 0 || groups[0].Length == 0)
                        continue;
                    string rootBits = groups[0].PadLeft(16, '0');
                    if (rootBits.Length > 16) rootBits = rootBits.Substring(rootBits.Length - 16);
                    ulong value = 0;
                    value |= (ulong)Convert.ToUInt16(rootBits, 2) << 48;
                    int depth = 0;
                    for (int g = 1; g < groups.Length && g <= 6; g++)
                    {
                        string childBits = groups[g].PadLeft(8, '0');
                        if (childBits.Length > 8) childBits = childBits.Substring(childBits.Length - 8);
                        byte b = Convert.ToByte(childBits, 2);
                        value |= (ulong)b << (8 * (6 - g));
                        if (b != 0) depth = g;
                    }

                    string commentText = UnescapeXmlComment(comment.Trim());
                    entries.Add((name, value, depth, commentText));
                    comment = "";
                }
                else if (!string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("///"))
                {
                    comment = "";
                }
            }

            return entries;
        }

        private static List<RedPointTree> RebuildTreesFromEntriesStatic(
            List<(string name, ulong value, int depth, string comment)> entries)
        {
            if (entries.Count == 0) return new List<RedPointTree>();
            Dictionary<(ulong value, int depth), RedPointTree.Node> keyToNode = new();
            List<RedPointTree> roots = new();
            foreach ((string name, ulong value, int depth, string comment) in entries.OrderBy(e => e.depth)
                        .ThenBy(e => e.value))
            {
                int ruleBit = depth == 0 ? (int)((value >> 48) & 1) : (int)((value >> (8 * (6 - depth))) & 1);
                RedPointTree.Node node = new()
                {
                    key = name,
                    comment = string.IsNullOrWhiteSpace(comment) ? null : comment,
                    rule = ruleBit == 1 ? RedPointRule.And : RedPointRule.Or
                };
                (ulong value, int depth) key = (value, depth);
                keyToNode[key] = node;
                if (depth == 0)
                {
                    roots.Add(new RedPointTree { root = node });
                }
                else
                {
                    ulong parentValue = GetParentValueFromDepth(value, depth);
                    (ulong parentValue, int) parentKey = (parentValue, depth - 1);
                    if (keyToNode.TryGetValue(parentKey, out RedPointTree.Node parent))
                        parent.children.Add(node);
                }
            }

            return roots;
        }

        private static ulong GetParentValueFromDepth(ulong value, int depth)
        {
            if (depth <= 0) return 0;
            int shift = 8 * (6 - depth);
            return value & ~(0xFFUL << shift);
        }

        /// <summary>
        /// 获取节点深度（子层级数）。根=0，深度1=1个子字节。
        /// </summary>
        private static int GetDepth(ulong value)
        {
            if ((value & 0x0000FFFFFFFFFFFFUL) == 0) return 0;
            int shift = 40;
            while (shift >= 0 && ((value >> shift) & 0xFF) == 0)
                shift -= 8;
            return shift < 0 ? 0 : (48 - shift) / 8;
        }

        /// <summary>
        /// 获取父节点的 value。根节点无父节点。
        /// </summary>
        private static ulong GetParentValue(ulong value)
        {
            int depth = GetDepth(value);
            if (depth <= 0) return 0;
            int shift = 8 * (6 - depth);
            return value & ~(0xFFUL << shift);
        }

        /// <summary>
        /// 收集导出项。根: 2B 末位规则; 子: 1B 末位规则. 子值包含父值.
        /// </summary>
        private void CollectExportEntries(RedPointTree.Node node, int depth, ulong parentValue, int treeIndex,
                                          int siblingIndex,
                                          List<(string name, ulong value, string comment, int depth, int treeIndex)>
                                              entries)
        {
            if (node == null) return;
            string key = node.key ?? "Unnamed";
            if (string.IsNullOrWhiteSpace(key)) key = "Unnamed";

            ulong myValue;
            if (depth == 0)
            {
                int ruleBit = node.rule == RedPointRule.And ? 1 : 0;
                ushort rootVal = (ushort)(((treeIndex + 1) << 1) | ruleBit);
                myValue = (ulong)rootVal << 48;
            }
            else
            {
                int ruleBit = node.rule == RedPointRule.And ? 1 : 0;
                byte childVal = (byte)(((siblingIndex + 1) << 1) | ruleBit);
                int shift = 8 * (6 - depth);
                myValue = parentValue | ((ulong)childVal << shift);
            }

            entries.Add((key, myValue, node.comment ?? "", depth, treeIndex));

            int idx = 0;
            foreach (RedPointTree.Node child in node.children)
            {
                CollectExportEntries(child, depth + 1, myValue, treeIndex, idx, entries);
                idx++;
            }
        }

        /// <summary>
        /// 格式化为二进制字面量，每层级用下划线分隔。根16位，子8位。固定输出64位使 C# 解析得到正确的高位对齐值。
        /// </summary>
        private string FormatBinaryValue(ulong value, int depth)
        {
            List<string> parts = new();
            ushort rootPart = (ushort)(value >> 48);
            parts.Add(Convert.ToString(rootPart, 2).PadLeft(16, '0'));
            for (int i = 0; i < 6; i++)
            {
                int shift = 40 - 8 * i;
                byte b = (byte)(value >> shift);
                parts.Add(Convert.ToString(b, 2).PadLeft(8, '0'));
            }

            return "0b" + string.Join("_", parts) + "UL";
        }

        private static string SanitizeEnumName(string key)
        {
            if (string.IsNullOrEmpty(key)) return "Unnamed";
            StringBuilder sb = new();
            foreach (char c in key)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                    sb.Append(c);
                else if (c == ' ' || c == '-' || c == '.')
                    sb.Append('_');
            }

            string s = sb.ToString().Trim('_');
            if (string.IsNullOrEmpty(s)) return "Unnamed";
            if (char.IsDigit(s[0])) return "_" + s;
            return s;
        }

        private static string EscapeXmlComment(string text)
        {
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;");
        }

        private static string UnescapeXmlComment(string text)
        {
            return text.Replace("&quot;", "\"").Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&");
        }

        private Dictionary<string, int> CollectAllKeyCounts()
        {
            Dictionary<string, int> counts = new(StringComparer.Ordinal);
            if (_trees == null) return counts;
            foreach (RedPointTree tree in _trees)
            {
                if (tree?.root == null) continue;
                CollectKeyCountsRecursive(tree.root, counts);
            }

            return counts;
        }

        private void CollectKeyCountsRecursive(RedPointTree.Node node, Dictionary<string, int> counts)
        {
            if (node == null) return;
            string k = node.key ?? "";
            counts.TryGetValue(k, out int c);
            counts[k] = c + 1;
            foreach (RedPointTree.Node child in node.children)
                CollectKeyCountsRecursive(child, counts);
        }

        private IEnumerable<RedPointTree> GetFilteredTrees()
        {
            if (_trees == null) yield break;
            string filter = _searchFilter?.Trim().ToLowerInvariant() ?? "";
            foreach (RedPointTree tree in _trees)
            {
                if (tree?.root == null) continue;
                string key = tree.root.key?.ToLowerInvariant() ?? "";
                if (string.IsNullOrEmpty(filter) || key.Contains(filter))
                    yield return tree;
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUILayout.LabelField("搜索", GUILayout.Width(28));
            _searchFilter =
                EditorGUILayout.TextField(_searchFilter, EditorStyles.toolbarSearchField, GUILayout.Width(120));

            if (GUILayout.Button("+创建", EditorStyles.toolbarButton, GUILayout.Width(60))) CreateNewTree();

            bool hasSelection = _selectedTree != null && _trees.Contains(_selectedTree);
            EditorGUI.BeginDisabledGroup(!hasSelection);
            if (GUILayout.Button("-删除", EditorStyles.toolbarButton, GUILayout.Width(60))) DeleteSelectedTree();
            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("导出", EditorStyles.toolbarButton, GUILayout.Width(40))) ExportRedPointKeys();

            if (GUILayout.Button("导入", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                if (TryImportRedPointKeysInternal(out List<RedPointTree> trees, out string ns))
                {
                    _trees = trees;
                    _selectedTree = _trees.Count > 0 ? _trees[0] : null;
                    if (!string.IsNullOrEmpty(ns))
                    {
                        _namespace = ns;
                        SaveNamespace();
                    }

                    _isDirty = false;
                    _modifiedSinceLastExport = true;
                    Repaint();
                }
                else
                {
                    EditorUtility.DisplayDialog("导入失败", "文件不存在或格式不符合导出规则", "确定");
                }
            }

            EditorGUILayout.LabelField("命名空间", GUILayout.Width(50));
            EditorGUI.BeginDisabledGroup(!_namespaceEditable);
            EditorGUI.BeginChangeCheck();
            _namespace = EditorGUILayout.TextField(_namespace ?? "", GUILayout.Width(100));
            if (EditorGUI.EndChangeCheck())
                SaveNamespace();
            EditorGUI.EndDisabledGroup();
            _namespaceEditable =
                GUILayout.Toggle(_namespaceEditable, "编辑", EditorStyles.toolbarButton, GUILayout.Width(36));

            _showComment = GUILayout.Toggle(_showComment, "注释", EditorStyles.toolbarButton, GUILayout.Width(40));
            _readOnly = GUILayout.Toggle(_readOnly, "只读", EditorStyles.toolbarButton, GUILayout.Width(40));

            if (GUILayout.Button("选择目录", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                string path =
                    EditorUtility.OpenFolderPanel("选择目录",
                                                  string.IsNullOrEmpty(_directoryPath)
                                                      ? Application.dataPath
                                                      : _directoryPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    _directoryPath = path;
                    SaveDirectory();
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void CreateNewTree()
        {
            RedPointTree tree = new()
            {
                root = new RedPointTree.Node
                {
                    key = "NewRoot",
                    rule = RedPointRule.Or
                }
            };
            _trees.Add(tree);
            _selectedTree = tree;
            MarkDirty();
            Repaint();
        }

        private void DeleteSelectedTree()
        {
            if (_selectedTree == null) return;
            if (!EditorUtility.DisplayDialog("确认删除", $"确定要删除红点树 \"{_selectedTree.root?.key ?? "未命名"}\" 吗？", "删除", "取消"))
                return;
            _trees.Remove(_selectedTree);
            _selectedTree = _trees.Count > 0 ? _trees[0] : null;
            MarkDirty();
            Repaint();
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(LeftPanelWidth));

            EditorGUILayout.LabelField("红点树列表", EditorStyles.boldLabel);

            _leftScroll = EditorGUILayout.BeginScrollView(_leftScroll, _leftScrollStyle, GUILayout.ExpandHeight(true));

            List<RedPointTree> filtered = GetFilteredTrees().Where(t => t?.root != null).ToList();

            int newHover = -1;
            for (int i = 0; i < filtered.Count; i++)
            {
                RedPointTree tree = filtered[i];
                string key = string.IsNullOrEmpty(tree.root.key) ? "(空)" : tree.root.key;
                bool selected = _selectedTree == tree;

                Rect rowRect =
                    GUILayoutUtility.GetRect(0f, PmListRowHeight, GUILayout.ExpandWidth(true));

                if (UnityEngine.Event.current.type == EventType.MouseMove &&
                    rowRect.Contains(UnityEngine.Event.current.mousePosition))
                    newHover = i;

                switch (UnityEngine.Event.current.type)
                {
                    case EventType.MouseDown when UnityEngine.Event.current.button == 0 &&
                                                  rowRect.Contains(UnityEngine.Event.current.mousePosition):
                        _selectedTree = tree;
                        GUI.changed = true;
                        UnityEngine.Event.current.Use();
                        break;
                    case EventType.Repaint:
                        DrawPackageManagerListRow(rowRect, selected, _leftListHoverIndex == i);
                        GUI.Label(rowRect, new GUIContent(key), _pmListLabelStyle);
                        break;
                }
            }

            if (UnityEngine.Event.current.type == EventType.MouseMove)
            {
                if (newHover != _leftListHoverIndex)
                {
                    _leftListHoverIndex = newHover;
                    Repaint();
                }
            }

            if (filtered.Count == 0) EditorGUILayout.HelpBox("暂无红点树\n点击「+创建」添加", MessageType.Info);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 与 Package Manager 侧栏接近的行背景：选中高亮、悬停、底部分隔线（无左侧图标）。
        /// </summary>
        private static void DrawPackageManagerListRow(Rect rowRect, bool selected, bool hover)
        {
            bool pro = EditorGUIUtility.isProSkin;
            // 与 Package Manager 类似：默认行不铺底，仅选中/悬停高亮；底部分隔线保留
            if (selected)
            {
                Color bg = pro
                    ? new Color(44f / 255f, 93f / 255f, 135f / 255f, 1f)
                    : new Color(62f / 255f, 125f / 255f, 231f / 255f, 1f);
                EditorGUI.DrawRect(rowRect, bg);
            }
            else if (hover)
            {
                Color bg = pro ? new Color(0.22f, 0.22f, 0.22f, 1f) : new Color(0.78f, 0.78f, 0.78f, 1f);
                EditorGUI.DrawRect(rowRect, bg);
            }

            Color line = pro ? new Color(0.12f, 0.12f, 0.12f, 1f) : new Color(0.65f, 0.65f, 0.65f, 1f);
            EditorGUI.DrawRect(new Rect(rowRect.x, rowRect.yMax - 1f, rowRect.width, 1f), line);
        }

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            if (_selectedTree != null)
            {
                EditorGUILayout.LabelField($"红点树结构: {_selectedTree.root?.key ?? "未命名"}", EditorStyles.boldLabel);

                _rightScroll = EditorGUILayout.BeginScrollView(_rightScroll, _boxStyle, GUILayout.ExpandHeight(true));

                Dictionary<string, int> keyCounts = CollectAllKeyCounts();
                DrawNode(_selectedTree.root, "", true, true, 1, keyCounts, _showComment, _readOnly);

                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.BeginVertical(_boxStyle, GUILayout.ExpandHeight(true));
                EditorGUILayout.HelpBox("请在左侧选择一棵红点树，或点击「+创建」新建", MessageType.None);
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawNode(RedPointTree.Node node, string prefix, bool isLast, bool isRoot, int depth,
                              Dictionary<string, int> keyCounts, bool showComment, bool readOnly)
        {
            if (node == null) return;

            EditorGUILayout.Space(NodeRowSpacing);
            EditorGUILayout.BeginHorizontal();

            // 根节点无连接符；子节点使用 ├─ 或 └─
            string connector = isRoot ? "" : isLast ? "└─ " : "├─ ";
            string prefixAndConnector = prefix + connector;
            if (!string.IsNullOrEmpty(prefixAndConnector))
            {
                float w = EditorStyles.label.CalcSize(new GUIContent(prefixAndConnector)).x;
                EditorGUILayout.LabelField(prefixAndConnector, GUILayout.Width(w));
            }

            EditorGUI.BeginDisabledGroup(readOnly);
            EditorGUI.BeginChangeCheck();
            // Key 重复时输入框背景变红（森林中所有节点 Key 必须唯一）
            string nodeKey = node.key ?? "";
            bool keyDuplicated = keyCounts != null && keyCounts.TryGetValue(nodeKey, out int cnt) && cnt > 1;
            if (readOnly)
            {
                EditorGUILayout.LabelField(node.key ?? "", _readOnlyLabelStyle, GUILayout.MinWidth(100));
            }
            else if (keyDuplicated)
            {
                Color origBg = GUI.backgroundColor;
                GUI.backgroundColor = Color.red;
                node.key = EditorGUILayout.TextField(node.key, GUILayout.MinWidth(100));
                GUI.backgroundColor = origBg;
            }
            else
            {
                node.key = EditorGUILayout.TextField(node.key, GUILayout.MinWidth(100));
            }

            if (EditorGUI.EndChangeCheck()) MarkDirty();
            EditorGUI.EndDisabledGroup();

            if (!readOnly)
            {
                bool atMaxDepth = depth >= MaxTreeDepth;
                EditorGUI.BeginDisabledGroup(atMaxDepth);
                if (GUILayout.Button("+", GUILayout.Width(24)))
                {
                    node.children.Add(new RedPointTree.Node
                    {
                        key = "NewNode",
                        rule = RedPointRule.Or
                    });
                    MarkDirty();
                }

                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("-", GUILayout.Width(24)))
                {
                    if (EditorUtility.DisplayDialog("确认删除", $"确定要删除节点 \"{node.key}\" 及其所有子节点吗？", "删除", "取消"))
                    {
                        if (node == _selectedTree.root)
                        {
                            _trees.Remove(_selectedTree);
                            _selectedTree = _trees.Count > 0 ? _trees[0] : null;
                        }
                        else
                        {
                            RemoveNode(_selectedTree.root, node);
                        }

                        MarkDirty();
                        EditorGUILayout.EndHorizontal();
                        Repaint();
                        return;
                    }
                }
            }

            if (node.children.Count > 0)
            {
                if (readOnly)
                {
                    EditorGUILayout.LabelField(node.rule.ToString(), _readOnlyLabelStyle, GUILayout.Width(60));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    node.rule = (RedPointRule)EditorGUILayout.EnumPopup(node.rule, GUILayout.Width(60));
                    if (EditorGUI.EndChangeCheck()) MarkDirty();
                }
            }

            if (showComment)
            {
                EditorGUILayout.LabelField("//", GUILayout.Width(18));
                if (readOnly)
                {
                    EditorGUILayout.LabelField(node.comment ?? "", _readOnlyLabelStyle, GUILayout.MinWidth(120),
                                               GUILayout.ExpandWidth(true));
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    node.comment =
                        EditorGUILayout.TextField(node.comment ?? "", GUILayout.MinWidth(120),
                                                  GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck()) MarkDirty();
                }
            }

            EditorGUILayout.EndHorizontal();

            // 子节点前缀：父为根时仅空，否则加 "   " 或 "│  "
            string childPrefix = isRoot ? "" : prefix + (isLast ? "   " : "│  ");
            List<RedPointTree.Node> children = node.children.ToList();
            int childDepth = depth + 1;
            for (int i = 0; i < children.Count; i++)
            {
                DrawNode(children[i], childPrefix, i == children.Count - 1, false, childDepth, keyCounts, showComment,
                         readOnly);
            }
        }

        private bool RemoveNode(RedPointTree.Node parent, RedPointTree.Node toRemove)
        {
            if (parent == null) return false;
            if (parent == toRemove)
                // 不能删除根节点，根节点由删除红点树处理
                return false;
            if (parent.children.Remove(toRemove))
                return true;
            foreach (RedPointTree.Node child in parent.children)
            {
                if (RemoveNode(child, toRemove))
                    return true;
            }

            return false;
        }
    }
}