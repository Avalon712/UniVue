using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UniVue.Editor.CodeGen;
using UniVue.UI;

namespace UniVue.Editor
{
    public sealed class UICodeExportWindow : EditorWindow
    {
        private const string PrefsKeyModules = "UniVue.UICodeExport.Modules";
        private const string PrefsKeyGeneratorPrefix = "UniVue.UICodeExport.Generator.";
        private const char ModulePathSeparator = '|';

        private Type[] _generatorTypes = Array.Empty<Type>();
        private string[] _generatorDisplayNames = Array.Empty<string>();
        private readonly List<ModuleNode> _modules = new();
        private Vector2 _scroll;
        private string _statusMessage = "";
        private bool _awaitingModuleDrop;

        private bool _stylesReady;
        private GUIStyle _cardStyle;
        private GUIStyle _headerLabelStyle;
        private GUIStyle _badgeStyle;
        private GUIStyle _prefabLabelStyle;
        private GUIStyle _prefabDisabledLabelStyle;
        private GUIStyle _typeLabelStyle;
        private GUIStyle _emptyHintStyle;

        private sealed class PrefabEntry
        {
            public string AssetPath;
            public string DisplayName;
            public Type ScriptType;
            public string SourceFileAbsolutePath;
            public bool CodeGenDisabled;
        }

        private sealed class ModuleNode
        {
            public string FolderPath;
            public string Name;
            public bool Foldout = true;
            public bool IsTopLevel;
            public Color AccentColor = Color.gray;
            public string SelectedGeneratorFullName;
            public readonly List<ModuleNode> Children = new();
            public readonly List<PrefabEntry> Prefabs = new();
        }

        [MenuItem("UniVue/Windows/UI Code Export")]
        public static void ShowWindow()
        {
            var window = GetWindow<UICodeExportWindow>("UI Code Export");
            window.minSize = new Vector2(480, 320);
            window.Show();
        }

        private void OnEnable()
        {
            DiscoverGenerators();
            RestoreModules();
        }

        private void DiscoverGenerators()
        {
            _generatorTypes = TypeCache.GetTypesDerivedFrom<UICodeGenerator>()
                .Where(t => !t.IsAbstract && t.GetConstructor(Type.EmptyTypes) != null)
                .OrderBy(t => t.FullName, StringComparer.Ordinal)
                .ToArray();
            _generatorDisplayNames = _generatorTypes.Select(t => t.FullName).ToArray();
        }

        private void RestoreModules()
        {
            _modules.Clear();
            string saved = EditorPrefs.GetString(PrefsKeyModules, "");
            if (string.IsNullOrEmpty(saved))
            {
                UpdateStatusMessage();
                return;
            }

            bool changed = false;
            foreach (string raw in saved.Split(ModulePathSeparator))
            {
                if (string.IsNullOrEmpty(raw) || !AssetDatabase.IsValidFolder(raw))
                {
                    changed = true;
                    continue;
                }

                string path = raw.Replace('\\', '/');

                // 过滤相互包含的历史数据：若与已加入模块存在祖先/后代关系则丢弃
                if (_modules.Any(m => IsPathInside(path, m.FolderPath) || IsPathInside(m.FolderPath, path)))
                {
                    changed = true;
                    continue;
                }

                _modules.Add(BuildModuleNode(path, isTopLevel: true));
            }

            _modules.Sort((a, b) => string.Compare(a.FolderPath, b.FolderPath, StringComparison.OrdinalIgnoreCase));
            if (changed)
                SaveModuleList();
            UpdateStatusMessage();
        }

        private void SaveModuleList()
        {
            string value = string.Join(ModulePathSeparator.ToString(),
                _modules.Where(m => m.IsTopLevel).Select(m => m.FolderPath));
            EditorPrefs.SetString(PrefsKeyModules, value);
        }

        private void OnGUI()
        {
            EnsureStyles();

            DrawToolbar();

            if (_generatorTypes.Length == 0)
            {
                EditorGUILayout.Space(4);
                EditorGUILayout.HelpBox(
                    "未发现可用的 UICodeGenerator 实现（需非抽象且具有无参构造函数）。导出已禁用。",
                    MessageType.Warning);
            }

            if (_awaitingModuleDrop)
                DrawAddModuleDropArea();

            EditorGUILayout.Space(6);

            if (_modules.Count == 0 && !_awaitingModuleDrop)
                DrawEmptyState();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.Space(2);
            for (int i = 0; i < _modules.Count; i++)
                DrawModuleNode(_modules[i], 0, _modules[i].AccentColor);
            EditorGUILayout.EndScrollView();

            DrawStatusBar();
        }

        private void EnsureStyles()
        {
            if (_stylesReady)
                return;

            _cardStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(0, 6, 4, 6),
                margin = new RectOffset(4, 4, 2, 6),
            };

            _headerLabelStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleLeft,
            };

            _badgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
            };

            _prefabLabelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
            };

            _prefabDisabledLabelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Italic,
            };
            _prefabDisabledLabelStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 1f);

            _typeLabelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
            };
            _typeLabelStyle.normal.textColor = new Color(0.55f, 0.55f, 0.55f, 1f);

            _emptyHintStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontSize = 11,
            };

            _stylesReady = true;
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUI.enabled = _modules.Count > 0 && _generatorTypes.Length > 0;
                if (GUILayout.Button(new GUIContent(" 导出全部", EditorGUIUtility.IconContent("SaveActive").image),
                        EditorStyles.toolbarButton, GUILayout.Width(90)))
                    ExportAllModules();
                GUI.enabled = true;

                if (GUILayout.Button(new GUIContent(" 刷新", EditorGUIUtility.IconContent("Refresh").image),
                        EditorStyles.toolbarButton, GUILayout.Width(64)))
                    RefreshAllModules();

                Color prev = GUI.backgroundColor;
                if (_awaitingModuleDrop)
                    GUI.backgroundColor = new Color(1f, 0.75f, 0.4f);
                if (GUILayout.Button(
                        new GUIContent(_awaitingModuleDrop ? " 取消添加" : " 添加模块",
                            EditorGUIUtility.IconContent(_awaitingModuleDrop ? "d_winbtn_win_close" : "Toolbar Plus").image),
                        EditorStyles.toolbarButton, GUILayout.Width(84)))
                    _awaitingModuleDrop = !_awaitingModuleDrop;
                GUI.backgroundColor = prev;

                GUILayout.FlexibleSpace();

                GUILayout.Label($"{_modules.Count} 个模块", EditorStyles.miniLabel);
            }
        }

        private void DrawEmptyState()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.Space(18);
                GUILayout.Label("尚未添加任何模块", _emptyHintStyle);
                GUILayout.Label("点击工具栏「添加模块」，然后把 Assets 下的目录拖进来", _emptyHintStyle);
                EditorGUILayout.Space(18);
            }
        }

        private void DrawAddModuleDropArea()
        {
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUI.BeginChangeCheck();
                DefaultAsset picked = (DefaultAsset)EditorGUILayout.ObjectField(
                    new GUIContent("选择目录", "也可在此直接选择 Assets 下的目录"),
                    null,
                    typeof(DefaultAsset),
                    false);
                if (EditorGUI.EndChangeCheck() && picked != null)
                {
                    TryAddModule(picked);
                    return;
                }

                Rect dropArea = GUILayoutUtility.GetRect(0, 52, GUILayout.ExpandWidth(true));
                bool hover = dropArea.Contains(UnityEngine.Event.current.mousePosition);
                Color fill = hover
                    ? new Color(0.3f, 0.55f, 0.95f, 0.22f)
                    : new Color(0.5f, 0.5f, 0.5f, 0.10f);
                EditorGUI.DrawRect(dropArea, fill);
                DrawBorder(dropArea, hover ? new Color(0.35f, 0.6f, 1f, 0.9f) : new Color(1f, 1f, 1f, 0.15f));

                var content = new GUIContent("  将目录拖放到此处以添加为模块",
                    EditorGUIUtility.IconContent("Folder Icon").image);
                var centered = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
                GUI.Label(dropArea, content, centered);
                HandleAddModuleDragAndDrop(dropArea);
            }
        }

        private void HandleAddModuleDragAndDrop(Rect dropArea)
        {
            UnityEngine.Event evt = UnityEngine.Event.current;
            if (!dropArea.Contains(evt.mousePosition))
                return;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!TryGetDraggedFolder(out _))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                        break;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        if (TryGetDraggedFolder(out DefaultAsset folder))
                            TryAddModule(folder);
                    }

                    evt.Use();
                    break;
            }
        }

        private static bool TryGetDraggedFolder(out DefaultAsset folder)
        {
            folder = null;
            if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0)
                return false;

            UnityEngine.Object obj = DragAndDrop.objectReferences[0];
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path))
                return false;

            folder = AssetDatabase.LoadAssetAtPath<DefaultAsset>(path);
            return folder != null;
        }

        private void TryAddModule(DefaultAsset folder)
        {
            if (folder == null)
                return;

            string path = AssetDatabase.GetAssetPath(folder).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(path))
            {
                Debug.LogWarning("请选择 Assets 下的有效目录。");
                return;
            }

            foreach (ModuleNode existing in _modules)
            {
                if (string.Equals(existing.FolderPath, path, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogWarning($"模块已存在：{path}");
                    _awaitingModuleDrop = false;
                    return;
                }

                // 不允许模块之间相互包含：新目录不能位于已有模块内部
                if (IsPathInside(path, existing.FolderPath))
                {
                    Debug.LogWarning($"该目录已包含在模块 '{existing.FolderPath}' 中，作为其子模块存在，无需重复添加。");
                    _awaitingModuleDrop = false;
                    return;
                }

                // 也不允许新目录反过来包含已有模块
                if (IsPathInside(existing.FolderPath, path))
                {
                    Debug.LogWarning($"该目录包含已存在的模块 '{existing.FolderPath}'，请先移除该子模块再作为父模块添加。");
                    _awaitingModuleDrop = false;
                    return;
                }
            }

            _modules.Add(BuildModuleNode(path, isTopLevel: true));
            _modules.Sort((a, b) => string.Compare(a.FolderPath, b.FolderPath, StringComparison.OrdinalIgnoreCase));
            _awaitingModuleDrop = false;
            SaveModuleList();
            UpdateStatusMessage();
        }

        /// <summary>
        /// 判断 <paramref name="child"/> 是否等于 <paramref name="parent"/> 或位于其内部（按目录层级，不做前缀误判）。
        /// </summary>
        private static bool IsPathInside(string child, string parent)
        {
            if (string.IsNullOrEmpty(child) || string.IsNullOrEmpty(parent))
                return false;

            if (string.Equals(child, parent, StringComparison.OrdinalIgnoreCase))
                return true;

            return child.Length > parent.Length
                   && child[parent.Length] == '/'
                   && child.StartsWith(parent, StringComparison.OrdinalIgnoreCase);
        }

        private void RemoveModule(ModuleNode module)
        {
            if (!_modules.Remove(module))
                return;

            SaveModuleList();
            UpdateStatusMessage();
        }

        private void RefreshAllModules()
        {
            DiscoverGenerators();
            List<string> paths = _modules.Select(m => m.FolderPath).ToList();
            _modules.Clear();
            foreach (string path in paths)
            {
                if (!AssetDatabase.IsValidFolder(path))
                    continue;
                _modules.Add(BuildModuleNode(path, isTopLevel: true));
            }

            SaveModuleList();
            UpdateStatusMessage();
        }

        private ModuleNode BuildModuleNode(string folderPath, bool isTopLevel)
        {
            folderPath = folderPath.Replace('\\', '/');
            var node = new ModuleNode
            {
                FolderPath = folderPath,
                Name = Path.GetFileName(folderPath),
                IsTopLevel = isTopLevel,
                AccentColor = GetStableColor(folderPath),
                SelectedGeneratorFullName = LoadSavedGenerator(folderPath)
            };

            if (string.IsNullOrEmpty(node.SelectedGeneratorFullName) && _generatorDisplayNames.Length > 0)
                node.SelectedGeneratorFullName = _generatorDisplayNames[0];

            CollectDirectPrefabs(node);
            CollectChildModules(node);
            return node;
        }

        private void CollectDirectPrefabs(ModuleNode node)
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { node.FolderPath });
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid).Replace('\\', '/');
                string parentDir = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
                if (!string.Equals(parentDir, node.FolderPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (prefab == null)
                    continue;

                BaseUI baseUI = prefab.GetComponent<BaseUI>();
                if (baseUI == null)
                    continue;

                Type scriptType = baseUI.GetType();

                MonoScript monoScript = MonoScript.FromMonoBehaviour(baseUI);
                if (monoScript == null)
                    continue;

                string scriptAssetPath = AssetDatabase.GetAssetPath(monoScript);
                if (string.IsNullOrEmpty(scriptAssetPath))
                    continue;

                node.Prefabs.Add(new PrefabEntry
                {
                    AssetPath = assetPath,
                    DisplayName = Path.GetFileNameWithoutExtension(assetPath),
                    ScriptType = scriptType,
                    SourceFileAbsolutePath = ToAbsolutePath(scriptAssetPath),
                    // 带 DisableUICodeGeneratedAttribute 的类仍显示，但禁用导出
                    CodeGenDisabled = IsUICodeGenerationDisabled(scriptType)
                });
            }

            node.Prefabs.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsUICodeGenerationDisabled(Type scriptType)
        {
            return scriptType.GetCustomAttribute<DisableUICodeGeneratedAttribute>(inherit: true) != null;
        }

        private void CollectChildModules(ModuleNode node)
        {
            string[] subFolders = AssetDatabase.GetSubFolders(node.FolderPath);
            Array.Sort(subFolders, StringComparer.OrdinalIgnoreCase);
            foreach (string subFolder in subFolders)
                node.Children.Add(BuildModuleNode(subFolder.Replace('\\', '/'), isTopLevel: false));
        }

        private static string ToAbsolutePath(string assetPath)
        {
            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            return Path.GetFullPath(Path.Combine(projectRoot ?? string.Empty, assetPath));
        }

        private string LoadSavedGenerator(string folderPath)
        {
            string saved = EditorPrefs.GetString(PrefsKeyGeneratorPrefix + folderPath, "");
            if (string.IsNullOrEmpty(saved))
                return null;
            if (_generatorDisplayNames.Contains(saved))
                return saved;
            return null;
        }

        private static void SaveGeneratorSelection(string folderPath, string generatorFullName)
        {
            if (string.IsNullOrEmpty(generatorFullName))
                EditorPrefs.DeleteKey(PrefsKeyGeneratorPrefix + folderPath);
            else
                EditorPrefs.SetString(PrefsKeyGeneratorPrefix + folderPath, generatorFullName);
        }

        private void DrawModuleNode(ModuleNode node, int depth, Color accent)
        {
            if (depth == 0)
            {
                Color prevBg = GUI.backgroundColor;
                GUI.backgroundColor = Color.Lerp(Color.white, accent, 0.10f);
                EditorGUILayout.BeginVertical(_cardStyle);
                GUI.backgroundColor = prevBg;

                DrawModuleHeader(node, depth, accent);
                DrawModuleBody(node, depth, accent);

                EditorGUILayout.EndVertical();
                return;
            }

            DrawModuleHeader(node, depth, accent);
            DrawModuleBody(node, depth, accent);
        }

        private void DrawModuleBody(ModuleNode node, int depth, Color accent)
        {
            if (!node.Foldout)
                return;

            if (node.Prefabs.Count == 0 && node.Children.Count == 0)
            {
                var r = EditorGUILayout.GetControlRect(false, 18);
                r.xMin += (depth + 1) * 14f + 22f;
                GUI.Label(r, "（无可导出的预制体）", _emptyHintStyle);
                return;
            }

            for (int i = 0; i < node.Prefabs.Count; i++)
                DrawPrefabRow(node, node.Prefabs[i], depth + 1, accent, i);

            foreach (ModuleNode child in node.Children)
                DrawModuleNode(child, depth + 1, accent);
        }

        private void DrawModuleHeader(ModuleNode node, int depth, Color accent)
        {
            const float rowH = 24f;
            Rect row = EditorGUILayout.GetControlRect(false, rowH);
            row.xMin += 2f;

            // 背景与左侧强调色条
            Color headerBg = depth == 0
                ? Color.Lerp(EditorBg, accent, 0.16f)
                : new Color(1f, 1f, 1f, 0.03f);
            EditorGUI.DrawRect(row, headerBg);
            EditorGUI.DrawRect(new Rect(row.x, row.y, 3f, row.height), accent);

            float indent = depth * 14f;
            float x = row.x + 8f + indent;

            var foldRect = new Rect(x, row.y + 4f, 14f, 16f);
            node.Foldout = EditorGUI.Foldout(foldRect, node.Foldout, GUIContent.none, true);
            x += 16f;

            var iconRect = new Rect(x, row.y + 4f, 16f, 16f);
            Texture folderIcon = EditorGUIUtility.IconContent(node.Foldout ? "FolderOpened Icon" : "Folder Icon").image;
            if (folderIcon != null)
                GUI.DrawTexture(iconRect, folderIcon, ScaleMode.ScaleToFit);
            x += 20f;

            // 右侧按钮区（从右往左排布）
            float rx = row.xMax - 6f;

            if (node.IsTopLevel)
            {
                var delRect = new Rect(rx - 22f, row.y + 3f, 22f, 18f);
                if (GUI.Button(delRect, new GUIContent("✕", "移除此模块"), EditorStyles.miniButton))
                {
                    RemoveModule(node);
                    GUIUtility.ExitGUI();
                    return;
                }

                rx -= 26f;
            }

            using (new EditorGUI.DisabledScope(_generatorTypes.Length == 0 || !HasExportablePrefabRecursive(node)))
            {
                var exportRect = new Rect(rx - 84f, row.y + 3f, 84f, 18f);
                if (GUI.Button(exportRect, new GUIContent(" 导出模块",
                            EditorGUIUtility.IconContent("SaveActive").image,
                            "导出该模块及其所有子模块的可生成预制体"),
                        EditorStyles.miniButton))
                    ExportModule(node);
                rx -= 88f;
            }

            float popupW = Mathf.Min(240f, Mathf.Max(140f, rx - x - 120f));
            var popupRect = new Rect(rx - popupW, row.y + 3f, popupW, 18f);
            DrawGeneratorPopup(node, popupRect);
            rx -= popupW + 6f;

            // 名称 + 数量徽章
            int prefabCount = CountPrefabs(node);
            var nameRect = new Rect(x, row.y, rx - x - 6f, row.height);
            GUI.Label(nameRect, node.Name, _headerLabelStyle);

            if (prefabCount > 0)
            {
                float nameW = Mathf.Min(_headerLabelStyle.CalcSize(new GUIContent(node.Name)).x + 6f, nameRect.width);
                var badgeRect = new Rect(x + nameW, row.y + 5f, 34f, 14f);
                if (badgeRect.xMax < rx)
                {
                    EditorGUI.DrawRect(badgeRect, Color.Lerp(EditorBg, accent, 0.55f));
                    GUI.Label(badgeRect, prefabCount.ToString(), _badgeStyle);
                }
            }
        }

        private void DrawPrefabRow(ModuleNode module, PrefabEntry prefab, int depth, Color accent, int index)
        {
            const float rowH = 20f;
            Rect row = EditorGUILayout.GetControlRect(false, rowH);
            row.xMin += 2f;

            UnityEngine.Event evt = UnityEngine.Event.current;
            bool hover = row.Contains(evt.mousePosition);

            if (hover)
                EditorGUI.DrawRect(row, new Color(0.35f, 0.6f, 1f, 0.12f));
            else if ((index & 1) == 1)
                EditorGUI.DrawRect(row, new Color(1f, 1f, 1f, 0.02f));

            EditorGUI.DrawRect(new Rect(row.x, row.y, 2f, row.height), Color.Lerp(accent, EditorBg, 0.35f));

            float indent = depth * 14f;
            float x = row.x + 10f + indent;

            var iconRect = new Rect(x, row.y + 2f, 16f, 16f);
            Texture icon = AssetDatabase.GetCachedIcon(prefab.AssetPath);
            if (icon != null)
                GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit);
            x += 20f;

            // 导出按钮（右）；带 DisableUICodeGeneratedAttribute 的预制体禁用导出
            float rx = row.xMax - 6f;
            using (new EditorGUI.DisabledScope(_generatorTypes.Length == 0 || prefab.CodeGenDisabled))
            {
                var exportRect = new Rect(rx - 52f, row.y + 1f, 52f, 18f);
                var exportContent = prefab.CodeGenDisabled
                    ? new GUIContent("导出", "该类标注了 DisableUICodeGeneratedAttribute，已禁止生成代码")
                    : new GUIContent("导出");
                if (GUI.Button(exportRect, exportContent, EditorStyles.miniButton))
                    ExportSinglePrefab(module, prefab);
                rx -= 56f;
            }

            // 禁用标记（右侧灰字，位于导出按钮左侧）
            if (prefab.CodeGenDisabled)
            {
                var tagRect = new Rect(rx - 44f, row.y, 44f, row.height);
                GUI.Label(tagRect, new GUIContent("已禁用", "该类标注了 DisableUICodeGeneratedAttribute"), _typeLabelStyle);
                rx -= 48f;
            }

            // 脚本类型（右侧灰字）
            float typeW = Mathf.Min(150f, _typeLabelStyle.CalcSize(new GUIContent(prefab.ScriptType.Name)).x + 4f);
            var typeRect = new Rect(rx - typeW, row.y, typeW, row.height);
            GUI.Label(typeRect, new GUIContent(prefab.ScriptType.Name, prefab.ScriptType.FullName), _typeLabelStyle);
            rx -= typeW + 6f;

            // 名称（可点击定位）
            var nameRect = new Rect(x, row.y, rx - x, row.height);
            EditorGUIUtility.AddCursorRect(nameRect, MouseCursor.Link);
            var nameStyle = prefab.CodeGenDisabled ? _prefabDisabledLabelStyle : _prefabLabelStyle;
            GUI.Label(nameRect, new GUIContent(prefab.DisplayName, "点击在 Project 中定位"), nameStyle);

            if (evt.type == EventType.MouseDown && evt.button == 0 && nameRect.Contains(evt.mousePosition))
            {
                PingPrefab(prefab);
                evt.Use();
            }
        }

        private static void PingPrefab(PrefabEntry prefab)
        {
            var obj = AssetDatabase.LoadAssetAtPath<GameObject>(prefab.AssetPath);
            if (obj == null)
                return;
            EditorGUIUtility.PingObject(obj);
            Selection.activeObject = obj;
        }

        private void DrawGeneratorPopup(ModuleNode node, Rect rect)
        {
            if (_generatorDisplayNames.Length == 0)
            {
                GUI.Label(rect, "(无生成器)", EditorStyles.miniLabel);
                return;
            }

            int index = Array.IndexOf(_generatorDisplayNames, node.SelectedGeneratorFullName);
            if (index < 0)
                index = 0;

            var content = new GUIContent(_generatorDisplayNames[index], _generatorDisplayNames[index]);
            if (EditorGUI.DropdownButton(rect, content, FocusType.Keyboard, EditorStyles.miniPullDown))
                ShowGeneratorMenu(node, index);
        }

        private void ShowGeneratorMenu(ModuleNode node, int currentIndex)
        {
            var menu = new GenericMenu();
            for (int i = 0; i < _generatorDisplayNames.Length; i++)
            {
                int captured = i;
                menu.AddItem(new GUIContent(_generatorDisplayNames[i]), i == currentIndex, () =>
                {
                    node.SelectedGeneratorFullName = _generatorDisplayNames[captured];
                    SaveGeneratorSelection(node.FolderPath, node.SelectedGeneratorFullName);
                    Repaint();
                });
            }

            menu.ShowAsContext();
        }

        private void DrawStatusBar()
        {
            if (string.IsNullOrEmpty(_statusMessage))
                return;

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.Label(EditorGUIUtility.IconContent("console.infoicon.sml"), GUILayout.Width(18));
                GUILayout.Label(_statusMessage, EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
            }
        }

        private static Color EditorBg =>
            EditorGUIUtility.isProSkin ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.78f, 0.78f, 0.78f);

        private static void DrawBorder(Rect rect, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1f, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), color);
        }

        private static Color GetStableColor(string s)
        {
            unchecked
            {
                int h = 17;
                if (s != null)
                {
                    for (int i = 0; i < s.Length; i++)
                        h = h * 31 + s[i];
                }

                float hue = (Mathf.Abs(h) % 360) / 360f;
                float sat = EditorGUIUtility.isProSkin ? 0.55f : 0.6f;
                float val = EditorGUIUtility.isProSkin ? 0.95f : 0.8f;
                return Color.HSVToRGB(hue, sat, val);
            }
        }

        private void ExportSinglePrefab(ModuleNode module, PrefabEntry entry)
        {
            int count = TryExportPrefab(module, entry) ? 1 : 0;
            FinishExport(count);
        }

        private void ExportModule(ModuleNode module)
        {
            FinishExport(ExportModuleRecursive(module, null));
        }

        private static bool HasExportablePrefabRecursive(ModuleNode node)
        {
            for (int i = 0; i < node.Prefabs.Count; i++)
            {
                if (!node.Prefabs[i].CodeGenDisabled)
                    return true;
            }

            for (int i = 0; i < node.Children.Count; i++)
            {
                if (HasExportablePrefabRecursive(node.Children[i]))
                    return true;
            }

            return false;
        }

        private void ExportAllModules()
        {
            int count = 0;
            foreach (ModuleNode module in _modules)
                count += ExportModuleRecursive(module, null);
            FinishExport(count);
        }

        private int ExportModuleRecursive(ModuleNode module, string inheritedGeneratorFullName)
        {
            // 子模块未单独指定生成器时，继承父模块的选择
            string effectiveGeneratorFullName = string.IsNullOrEmpty(module.SelectedGeneratorFullName)
                ? inheritedGeneratorFullName
                : module.SelectedGeneratorFullName;

            int count = 0;
            foreach (PrefabEntry entry in module.Prefabs)
            {
                if (entry.CodeGenDisabled)
                    continue;
                if (TryExportPrefab(module, entry, effectiveGeneratorFullName))
                    count++;
            }

            foreach (ModuleNode child in module.Children)
                count += ExportModuleRecursive(child, effectiveGeneratorFullName);
            return count;
        }

        private bool TryExportPrefab(ModuleNode module, PrefabEntry entry, string generatorFullName = null)
        {
            if (IsUICodeGenerationDisabled(entry.ScriptType))
            {
                Debug.LogWarning($"类型 {entry.ScriptType.FullName} 标注了 DisableUICodeGeneratedAttribute，已跳过：{entry.AssetPath}");
                return false;
            }

            Type generatorType = ResolveGeneratorType(generatorFullName ?? module.SelectedGeneratorFullName);
            if (generatorType == null)
            {
                Debug.LogWarning($"模块 '{module.Name}' 未选择有效的代码生成器，已跳过预制体：{entry.AssetPath}");
                return false;
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(entry.AssetPath);
            if (prefab == null)
            {
                Debug.LogWarning($"无法加载预制体：{entry.AssetPath}");
                return false;
            }

            try
            {
                var generator = (UICodeGenerator)Activator.CreateInstance(generatorType);
                generator.PrefabPath = entry.AssetPath;
                generator.ExportInternal(prefab, entry.SourceFileAbsolutePath, entry.ScriptType, entry.AssetPath);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        private Type ResolveGeneratorType(string generatorFullName)
        {
            if (_generatorTypes.Length == 0)
                return null;

            if (string.IsNullOrEmpty(generatorFullName))
                return _generatorTypes[0];

            for (int i = 0; i < _generatorTypes.Length; i++)
            {
                if (_generatorTypes[i].FullName == generatorFullName)
                    return _generatorTypes[i];
            }

            return null;
        }

        private void FinishExport(int successCount)
        {
            AssetDatabase.Refresh();
            _statusMessage = $"导出完成：成功 {successCount} 个预制体。";
            Debug.Log(_statusMessage);
            Repaint();
        }

        private void UpdateStatusMessage()
        {
            int moduleCount = 0;
            int prefabCount = 0;
            foreach (ModuleNode module in _modules)
            {
                moduleCount += CountModules(module);
                prefabCount += CountPrefabs(module);
            }

            _statusMessage = _modules.Count == 0
                ? ""
                : $"已添加模块：{_modules.Count} 个（含层级共 {moduleCount} 个），预制体：{prefabCount} 个。";
        }

        private static int CountModules(ModuleNode node)
        {
            if (node == null)
                return 0;
            int count = 1;
            foreach (ModuleNode child in node.Children)
                count += CountModules(child);
            return count;
        }

        private static int CountPrefabs(ModuleNode node)
        {
            if (node == null)
                return 0;
            int count = node.Prefabs.Count;
            foreach (ModuleNode child in node.Children)
                count += CountPrefabs(child);
            return count;
        }
    }
}
