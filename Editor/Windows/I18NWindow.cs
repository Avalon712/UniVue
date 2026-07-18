#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UniVue.I18n;

#endregion

namespace UniVue.Editor
{
    /// <summary>
    /// 多语言（I18N）编辑窗口：以表格形式管理 <see cref="LanguageTable" /> 中的所有文本与语种，
    /// 同时可编辑 <see cref="I18NFontConfig" /> 的相关配置。
    /// </summary>
    public sealed class I18NWindow : EditorWindow
    {
        private const float IdColumnWidth = 150f;
        private const float LangColumnWidth = 220f;
        private const float OpColumnWidth = 168f;
        private const float RowHeight = 26f;
        private const float HeaderHeight = 28f;

        // 搜索过滤（正则 + 多选匹配项；bit0=Text Id，bit1..=语种）
        private const int SearchIdBit = 1;
        private GUIStyle _cellFieldStyle;
        private UnityEditor.Editor _configEditor;

        // I18NConfig 内嵌编辑器
        private bool _configFoldout = true;
        private Color _defaultColBg;
        private int _deleteLangIndex;
        private int _exporterIndex;
        private string[] _exporterNames;

        // 颜色
        private Color _headerBg;

        // 样式（懒加载）
        private GUIStyle _headerCellStyle;
        private GUIStyle _idCellStyle;
        private Color _lineColor;
        private Color _lineColorSoft;
        private string _newLanguage = string.Empty;
        private string _newText = string.Empty;

        // 需要在本次 OnGUI 布局结束后再执行的结构性变更，避免 GUILayout 布局错位
        private Action _pending;
        private int _renameLangIndex;
        private string _renameLanguage = string.Empty;
        private Color _rowOddBg;
        private Vector2 _scroll;
        private int _searchLanguageMask;
        private bool _searchMaskInitialized;
        private string _searchPattern = string.Empty;
        private string _searchRegexError;
        private bool _stylesReady;

        private LanguageTable _table;
        private int _translatorIndex;
        private string[] _translatorNames;

        // 表格列显示（仅控制语种列；Text Id / 操作始终显示；默认全部显示）
        private int _visibleColumnMask;
        private bool _visibleColumnMaskInitialized;
        private bool _wasWaitingTranslation;

        private void OnEnable()
        {
            _table = LanguageTable.GetOrCreate();
            RefreshTranslatorNames();
            RefreshExporterNames();
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;

            // 窗口关闭时自动保存一次
            LanguageTable.Save();

            if (_configEditor != null)
            {
                DestroyImmediate(_configEditor);
                _configEditor = null;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            if (_table == null) _table = LanguageTable.GetOrCreate();

            DrawConfigSection();
            DrawSettingsBar();

            EditorGUILayout.Space(4);

            if (_table.languageTypes == null || _table.languageTypes.Count == 0)
                EditorGUILayout.HelpBox("尚未定义任何语种，请先在上方添加一个语种。", MessageType.Info);
            else
                DrawTable();

            ExecutePending();
        }

        [MenuItem("UniVue/I18N Editor Window")]
        public static void Open()
        {
            I18NWindow window = GetWindow<I18NWindow>();
            window.titleContent = new GUIContent("I18N Editor Window");
            window.minSize = new Vector2(560, 360);
            window.Show();
        }

        private void RefreshTranslatorNames()
        {
            _translatorNames = LanguageTable.Translators
                .Select(t => t.GetType().Name)
                .ToArray();
            if (_translatorNames.Length == 0)
                _translatorIndex = 0;
            else
                _translatorIndex = Mathf.Clamp(_translatorIndex, 0, _translatorNames.Length - 1);
        }

        private void RefreshExporterNames()
        {
            _exporterNames = LanguageTable.Exporters
                .Select(e => e.GetType().Name)
                .ToArray();
            if (_exporterNames.Length == 0)
                _exporterIndex = 0;
            else
                _exporterIndex = Mathf.Clamp(_exporterIndex, 0, _exporterNames.Length - 1);
        }

        private void OnEditorUpdate()
        {
            if (_table == null) return;
            // 翻译进行中或刚结束时刷新，以便解除禁用并显示最新译文
            bool waiting = _table.HasAnyTextIsWaitTranslationResult();
            if (waiting || _wasWaitingTranslation) Repaint();
            _wasWaitingTranslation = waiting;
        }

        private void EnsureStyles()
        {
            if (_stylesReady) return;

            _headerCellStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(6, 6, 0, 0)
            };

            _idCellStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 6, 0, 0)
            };

            _cellFieldStyle = new GUIStyle(EditorStyles.textField)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(6, 6, 0, 0),
                wordWrap = false
            };

            bool pro = EditorGUIUtility.isProSkin;
            _headerBg = pro ? new Color(0.24f, 0.24f, 0.24f) : new Color(0.78f, 0.78f, 0.78f);
            _rowOddBg = pro ? new Color(1f, 1f, 1f, 0.035f) : new Color(0f, 0f, 0f, 0.035f);
            _defaultColBg = pro ? new Color(0.35f, 0.55f, 0.85f, 0.12f) : new Color(0.35f, 0.55f, 0.85f, 0.15f);
            _lineColor = pro ? new Color(0f, 0f, 0f, 0.55f) : new Color(0f, 0f, 0f, 0.25f);
            _lineColorSoft = pro ? new Color(0f, 0f, 0f, 0.28f) : new Color(0f, 0f, 0f, 0.12f);

            _stylesReady = true;
        }

        private void DrawSettingsBar()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string[] langs = _table.languageTypes != null
                    ? _table.languageTypes.ToArray()
                    : Array.Empty<string>();
                bool hasLangs = langs.Length > 0;

                // 默认语种（任意文本正在翻译时不允许切换）
                bool anyWaiting = _table.HasAnyTextIsWaitTranslationResult();
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("默认语种", GUILayout.Width(60));
                    if (!hasLangs)
                    {
                        EditorGUILayout.LabelField("（暂无语种）", EditorStyles.miniLabel, GUILayout.Width(180));
                    }
                    else
                    {
                        int current = Mathf.Max(0, Array.IndexOf(langs, _table.defaultLanguage));
                        using (new EditorGUI.DisabledScope(anyWaiting))
                        {
                            EditorGUI.BeginChangeCheck();
                            int next = EditorGUILayout.Popup(current, langs, GUILayout.Width(180));
                            if (EditorGUI.EndChangeCheck() && next >= 0 && next < langs.Length && next != current)
                            {
                                string target = langs[next];
                                ChangeDefaultLanguage(target);
                            }
                        }
                    }

                    if (anyWaiting) EditorGUILayout.LabelField("（有文本正在翻译，暂不可切换）", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.LabelField(
                    "提示：一旦设置应尽量保持不变，因为游戏中的文本都是以默认语种文本进行显示的。",
                    EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.Space(2);

                // 删除语种（默认语种不可删除）
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("删除语种", GUILayout.Width(60));
                    string[] removable = hasLangs
                        ? langs.Where(l => l != _table.defaultLanguage).ToArray()
                        : Array.Empty<string>();
                    if (removable.Length == 0)
                    {
                        EditorGUILayout.LabelField(
                            hasLangs ? "（默认语种不可删除）" : "（暂无语种）",
                            EditorStyles.miniLabel,
                            GUILayout.Width(180));
                    }
                    else
                    {
                        _deleteLangIndex = Mathf.Clamp(_deleteLangIndex, 0, removable.Length - 1);
                        _deleteLangIndex = EditorGUILayout.Popup(_deleteLangIndex, removable, GUILayout.Width(180));
                        if (GUILayout.Button("删除语种", GUILayout.Width(90)))
                        {
                            string lang = removable[_deleteLangIndex];
                            if (EditorUtility.DisplayDialog("删除语种", $"确定删除语种 \"{lang}\" 及其所有文本内容？", "删除", "取消"))
                                _pending = () =>
                                {
                                    _table.RemoveLanguage(lang);
                                    _deleteLangIndex = 0;
                                    GUI.FocusControl(null);
                                };
                        }
                    }

                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space(2);

                // 添加语种
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("新增语种", GUILayout.Width(60));
                    _newLanguage = EditorGUILayout.TextField(_newLanguage, GUILayout.Width(180));
                    using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(_newLanguage)))
                    {
                        if (GUILayout.Button("添加语种", GUILayout.Width(90))) AddLanguage();
                    }

                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space(2);

                // 重命名语种（翻译中不允许）
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("重命名", GUILayout.Width(60));
                    if (!hasLangs)
                    {
                        EditorGUILayout.LabelField("（暂无语种）", EditorStyles.miniLabel, GUILayout.Width(100));
                    }
                    else
                    {
                        using (new EditorGUI.DisabledScope(anyWaiting))
                        {
                            _renameLangIndex = Mathf.Clamp(_renameLangIndex, 0, langs.Length - 1);
                            _renameLangIndex = EditorGUILayout.Popup(_renameLangIndex, langs, GUILayout.Width(100));
                            EditorGUILayout.LabelField("→", GUILayout.Width(16));
                            _renameLanguage = EditorGUILayout.TextField(_renameLanguage, GUILayout.Width(100));
                            bool canRename = !string.IsNullOrWhiteSpace(_renameLanguage)
                                             && _renameLanguage.Trim() != langs[_renameLangIndex];
                            using (new EditorGUI.DisabledScope(!canRename))
                            {
                                if (GUILayout.Button("重命名语种", GUILayout.Width(90)))
                                {
                                    string oldName = langs[_renameLangIndex];
                                    string newName = _renameLanguage.Trim();
                                    _pending = () => RenameLanguage(oldName, newName);
                                }
                            }
                        }
                    }

                    if (anyWaiting) EditorGUILayout.LabelField("（翻译中不可重命名）", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space(2);

                // 添加文本
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("新增文本", GUILayout.Width(60));
                    _newText = EditorGUILayout.TextField(_newText, GUILayout.Width(180));
                    using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_newText) ||
                                                       string.IsNullOrEmpty(_table.defaultLanguage)))
                    {
                        if (GUILayout.Button("添加文本", GUILayout.Width(90))) AddText();
                    }

                    EditorGUILayout.LabelField("（以默认语种文本作为内容）", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space(2);

                // 翻译器
                if (_translatorNames == null) RefreshTranslatorNames();
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("翻译器", GUILayout.Width(60));
                    if (_translatorNames.Length == 0)
                        EditorGUILayout.LabelField("（未找到 ITextTranslator 实现）", EditorStyles.miniLabel);
                    else
                        _translatorIndex =
                            EditorGUILayout.Popup(_translatorIndex, _translatorNames, GUILayout.Width(180));
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space(2);

                // 语言导出器
                if (_exporterNames == null) RefreshExporterNames();
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("导出器", GUILayout.Width(60));
                    using (new EditorGUI.DisabledScope(anyWaiting))
                    {
                        if (_exporterNames.Length == 0)
                        {
                            EditorGUILayout.LabelField("（未找到 II10NFileExporter 实现）", EditorStyles.miniLabel);
                        }
                        else
                        {
                            _exporterIndex =
                                EditorGUILayout.Popup(_exporterIndex, _exporterNames, GUILayout.Width(180));
                            using (new EditorGUI.DisabledScope(langs.Length == 0))
                            {
                                if (GUILayout.Button("导出语言配置", GUILayout.Width(100))) _pending = ExportLanguages;
                            }
                        }
                    }

                    if (anyWaiting) EditorGUILayout.LabelField("（翻译中不可导出）", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space(2);

                // 搜索过滤
                EnsureSearchLanguageMask(langs);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("搜索", GUILayout.Width(60));
                    _searchPattern = EditorGUILayout.TextField(_searchPattern, GUILayout.Width(180));
                    EditorGUILayout.LabelField("匹配", GUILayout.Width(36));
                    DrawSearchTargetDropdown(langs);
                    if (GUILayout.Button("清空", GUILayout.Width(50)))
                    {
                        _searchPattern = string.Empty;
                        GUI.FocusControl(null);
                    }

                    EditorGUILayout.LabelField("（支持正则；选 Text Id 时仅按 Id 匹配）", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                }

                if (!string.IsNullOrEmpty(_searchRegexError))
                    EditorGUILayout.HelpBox($"正则无效：{_searchRegexError}", MessageType.Warning);

                EditorGUILayout.Space(2);

                // 显示列（语种列多选，默认全部）
                // 注意：MaskField / Popup 不能传入空数组，否则 Unity 内部会 IndexOutOfRangeException
                EnsureVisibleColumnMask(langs);
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("显示列", GUILayout.Width(60));
                    if (!hasLangs)
                    {
                        EditorGUILayout.LabelField("（暂无语种列）", EditorStyles.miniLabel, GUILayout.Width(180));
                    }
                    else
                    {
                        _visibleColumnMask = EditorGUILayout.MaskField(
                            _visibleColumnMask, langs, GUILayout.Width(180));
                        if (GUILayout.Button("全部", GUILayout.Width(50)))
                            _visibleColumnMask = langs.Length >= 31 ? -1 : (1 << langs.Length) - 1;
                    }

                    EditorGUILayout.LabelField("（Text Id / 操作始终显示）", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void EnsureVisibleColumnMask(string[] langs)
        {
            if (langs == null || langs.Length == 0)
            {
                _visibleColumnMask = 0;
                _visibleColumnMaskInitialized = false;
                return;
            }

            int maxMask = langs.Length >= 31 ? -1 : (1 << langs.Length) - 1;
            _visibleColumnMask &= maxMask;

            if (!_visibleColumnMaskInitialized || _visibleColumnMask == 0)
            {
                _visibleColumnMask = maxMask;
                _visibleColumnMaskInitialized = true;
            }
        }

        private bool IsLanguageColumnVisible(int languageIndex)
        {
            return (_visibleColumnMask & (1 << languageIndex)) != 0;
        }

        private int CountVisibleLanguageColumns(int languageCount)
        {
            int count = 0;
            for (int i = 0; i < languageCount; i++)
                if (IsLanguageColumnVisible(i))
                    count++;
            return count;
        }

        /// <summary>
        /// 保证搜索 Mask 与当前语种列表对齐；首次或列表变化时默认选中默认语种。
        /// bit0 = Text Id，bit(i+1) = langs[i]
        /// </summary>
        private void EnsureSearchLanguageMask(string[] langs)
        {
            if (langs == null || langs.Length == 0)
            {
                // 仅保留 Id 位的可能
                if ((_searchLanguageMask & SearchIdBit) != 0)
                {
                    _searchLanguageMask = SearchIdBit;
                }
                else
                {
                    _searchLanguageMask = 0;
                    _searchMaskInitialized = false;
                }

                return;
            }

            // 有效位：Id + 各语种
            int bitCount = langs.Length + 1;
            int maxMask = bitCount >= 31 ? -1 : (1 << bitCount) - 1;
            _searchLanguageMask &= maxMask;

            // 选中 Text Id 时互斥，清除其它语种勾选
            if ((_searchLanguageMask & SearchIdBit) != 0)
            {
                _searchLanguageMask = SearchIdBit;
                _searchMaskInitialized = true;
                return;
            }

            if (!_searchMaskInitialized || _searchLanguageMask == 0)
            {
                int defaultIndex = Array.IndexOf(langs, _table.defaultLanguage);
                if (defaultIndex < 0) defaultIndex = 0;
                _searchLanguageMask = 1 << (defaultIndex + 1);
                _searchMaskInitialized = true;
            }
        }

        private void DrawSearchTargetDropdown(string[] langs)
        {
            string summary = GetSearchTargetSummary(langs);
            if (!EditorGUILayout.DropdownButton(new GUIContent(summary), FocusType.Keyboard, GUILayout.Width(150)))
                return;

            bool idSelected = (_searchLanguageMask & SearchIdBit) != 0;
            GenericMenu menu = new();

            menu.AddItem(new GUIContent("Text Id"), idSelected, () =>
            {
                if (idSelected)
                {
                    // 取消 Id → 回退到默认语种
                    int defaultIndex = langs != null ? Array.IndexOf(langs, _table.defaultLanguage) : -1;
                    if (defaultIndex < 0) defaultIndex = 0;
                    _searchLanguageMask = langs != null && langs.Length > 0
                        ? 1 << (defaultIndex + 1)
                        : 0;
                }
                else
                {
                    _searchLanguageMask = SearchIdBit;
                }
            });

            if (langs != null && langs.Length > 0)
            {
                menu.AddSeparator(string.Empty);
                for (int i = 0; i < langs.Length; i++)
                {
                    int langBit = 1 << (i + 1);
                    bool on = (_searchLanguageMask & langBit) != 0;
                    if (idSelected)
                    {
                        // 选中 Text Id 后其它语种禁用勾选
                        menu.AddDisabledItem(new GUIContent(langs[i]));
                    }
                    else
                    {
                        int captured = i;
                        menu.AddItem(new GUIContent(langs[i]), on, () =>
                        {
                            _searchLanguageMask ^= 1 << (captured + 1);
                            if (_searchLanguageMask == 0 && langs.Length > 0)
                            {
                                int defaultIndex = Array.IndexOf(langs, _table.defaultLanguage);
                                if (defaultIndex < 0) defaultIndex = 0;
                                _searchLanguageMask = 1 << (defaultIndex + 1);
                            }
                        });
                    }
                }
            }

            menu.ShowAsContext();
        }

        private string GetSearchTargetSummary(string[] langs)
        {
            if ((_searchLanguageMask & SearchIdBit) != 0) return "Text Id";

            if (langs == null || langs.Length == 0) return "(未选择)";

            List<string> selected = new();
            for (int i = 0; i < langs.Length; i++)
                if ((_searchLanguageMask & (1 << (i + 1))) != 0)
                    selected.Add(langs[i]);

            if (selected.Count == 0) return "(未选择)";
            if (selected.Count == 1) return selected[0];
            if (selected.Count == langs.Length) return "全部语种";
            return string.Join(", ", selected);
        }

        private void DrawConfigSection()
        {
            UniVueEditorSettings settings = UniVueEditorSettings.instance;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _configFoldout = EditorGUILayout.Foldout(_configFoldout, "多语言配置 (I18NConfig)", true);
                if (!_configFoldout) return;

                I18NFontConfig config = settings.languageConfig;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    config = (I18NFontConfig)EditorGUILayout.ObjectField(
                        "配置资源", settings.languageConfig, typeof(I18NFontConfig), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        settings.languageConfig = config;
                        settings.SaveSettings();
                        RecreateConfigEditor(config);
                    }

                    if (GUILayout.Button("创建", GUILayout.Width(60))) _pending = CreateConfigAsset;
                }

                if (config == null)
                {
                    EditorGUILayout.HelpBox("未指定 I18NConfig 资源，可点击右侧“创建”新建一个。", MessageType.Info);
                    return;
                }

                if (_configEditor == null || _configEditor.target != config) RecreateConfigEditor(config);

                EditorGUILayout.Space(2);
                _configEditor.OnInspectorGUI();
            }
        }

        private void RecreateConfigEditor(I18NFontConfig config)
        {
            if (_configEditor != null)
            {
                DestroyImmediate(_configEditor);
                _configEditor = null;
            }

            if (config != null) _configEditor = UnityEditor.Editor.CreateEditor(config);
        }

        private void CreateConfigAsset()
        {
            // 打开保存文件对话框（限定在工程内），默认目录为 Assets
            string path = EditorUtility.SaveFilePanelInProject(
                "创建 I18NConfig",
                "I18NConfig",
                "asset",
                "选择 I18NConfig 资源的保存位置",
                "Assets");

            if (string.IsNullOrEmpty(path)) return;

            I18NFontConfig config = CreateInstance<I18NFontConfig>();
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            UniVueEditorSettings.instance.languageConfig = config;
            UniVueEditorSettings.instance.SaveSettings();
            RecreateConfigEditor(config);
            EditorGUIUtility.PingObject(config);
        }

        private void DrawTable()
        {
            List<string> languageTypes = _table.languageTypes;
            string defaultLanguage = _table.defaultLanguage;
            List<Languages> languages = _table.languages;

            int visibleLangCount = CountVisibleLanguageColumns(languageTypes.Count);
            float totalWidth = IdColumnWidth + LangColumnWidth * visibleLangCount + OpColumnWidth;

            Regex regex = TryCompileSearchRegex();
            List<Languages> filtered = FilterLanguages(languages, languageTypes, regex);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            // 表头
            Rect headerRect = GUILayoutUtility.GetRect(totalWidth, HeaderHeight, GUILayout.Width(totalWidth));
            DrawHeaderRow(headerRect, languageTypes, defaultLanguage);

            // 数据行
            if (languages == null || languages.Count == 0)
            {
                Rect emptyRect = GUILayoutUtility.GetRect(totalWidth, RowHeight * 2, GUILayout.Width(totalWidth));
                EditorGUI.LabelField(emptyRect, "暂无文本数据，请在上方“新增文本”。", EditorStyles.centeredGreyMiniLabel);
            }
            else if (filtered.Count == 0)
            {
                Rect emptyRect = GUILayoutUtility.GetRect(totalWidth, RowHeight * 2, GUILayout.Width(totalWidth));
                EditorGUI.LabelField(emptyRect, "无匹配结果。", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                for (int r = 0; r < filtered.Count; r++)
                {
                    Rect rowRect = GUILayoutUtility.GetRect(totalWidth, RowHeight, GUILayout.Width(totalWidth));
                    DrawDataRow(rowRect, filtered[r], r, languageTypes, defaultLanguage);
                }

                // 底部收尾边框
                Rect bottom = GUILayoutUtility.GetRect(totalWidth, 1, GUILayout.Width(totalWidth));
                EditorGUI.DrawRect(new Rect(bottom.x, bottom.y, totalWidth, 1), _lineColor);
            }

            EditorGUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(_searchPattern) && regex != null && languages != null)
                EditorGUILayout.LabelField(
                    $"显示 {filtered.Count} / {languages.Count} 条",
                    EditorStyles.miniLabel);
        }

        private Regex TryCompileSearchRegex()
        {
            _searchRegexError = null;
            if (string.IsNullOrEmpty(_searchPattern)) return null;

            try
            {
                return new Regex(_searchPattern, RegexOptions.CultureInvariant);
            }
            catch (ArgumentException ex)
            {
                _searchRegexError = ex.Message;
                return null;
            }
        }

        private List<Languages> FilterLanguages(List<Languages> languages, List<string> languageTypes, Regex regex)
        {
            if (languages == null || languages.Count == 0) return new List<Languages>();

            if (string.IsNullOrEmpty(_searchPattern)) return languages;

            if (regex == null)
                // 正则无效时不隐藏数据，避免误操作
                return languages;

            if (_searchLanguageMask == 0) return new List<Languages>();

            // 非 Id 模式时需要有语种可选
            if ((_searchLanguageMask & SearchIdBit) == 0
                && (languageTypes == null || languageTypes.Count == 0))
                return new List<Languages>();

            List<Languages> result = new();
            for (int i = 0; i < languages.Count; i++)
            {
                Languages entry = languages[i];
                if (MatchesSearch(entry, languageTypes, regex)) result.Add(entry);
            }

            return result;
        }

        private bool MatchesSearch(Languages entry, List<string> languageTypes, Regex regex)
        {
            if (entry == null) return false;

            // 选中 Text Id：仅按 Id 匹配，忽略语种
            if ((_searchLanguageMask & SearchIdBit) != 0) return regex.IsMatch(entry.id.ToString());

            if (entry.languages == null || languageTypes == null) return false;

            for (int c = 0; c < languageTypes.Count; c++)
            {
                // bit0 留给 Text Id，语种从 bit1 开始
                if ((_searchLanguageMask & (1 << (c + 1))) == 0) continue;
                string value = c < entry.languages.Count ? entry.languages[c] : string.Empty;
                if (value != null && regex.IsMatch(value)) return true;
            }

            return false;
        }

        private void DrawHeaderRow(Rect rect, List<string> languageTypes, string defaultLanguage)
        {
            EditorGUI.DrawRect(rect, _headerBg);

            float x = rect.x;
            EditorGUI.LabelField(new Rect(x, rect.y, IdColumnWidth, rect.height), "Text Id", _headerCellStyle);
            x += IdColumnWidth;

            for (int i = 0; i < languageTypes.Count; i++)
            {
                if (!IsLanguageColumnVisible(i)) continue;

                string lang = languageTypes[i];
                bool isDefault = lang == defaultLanguage;
                Rect cell = new(x, rect.y, LangColumnWidth, rect.height);
                if (isDefault) EditorGUI.DrawRect(cell, _defaultColBg);
                EditorGUI.LabelField(cell, isDefault ? $"★ {lang}" : lang, _headerCellStyle);
                x += LangColumnWidth;
            }

            EditorGUI.LabelField(new Rect(x, rect.y, OpColumnWidth, rect.height), "操作", _headerCellStyle);

            // 表头底部实线 + 竖向分隔线
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), _lineColor);
            DrawColumnLines(rect, CountVisibleLanguageColumns(languageTypes.Count), _lineColor);
        }

        private void DrawDataRow(Rect rect, Languages entry, int index, List<string> languageTypes,
            string defaultLanguage)
        {
            if (index % 2 == 1) EditorGUI.DrawRect(rect, _rowOddBg);

            bool waiting = _table.IsWaitTranslationResult(entry.id);
            float x = rect.x;

            // Id：SelectableLabel 支持鼠标拖选复制
            Rect idRect = new(x, rect.y, IdColumnWidth, rect.height);
            if (waiting)
            {
                EditorGUI.SelectableLabel(
                    new Rect(idRect.x, idRect.y, idRect.width - 48, idRect.height),
                    entry.id.ToString(),
                    _idCellStyle);
                EditorGUI.LabelField(
                    new Rect(idRect.xMax - 46, idRect.y, 44, idRect.height),
                    "翻译中",
                    EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                EditorGUI.SelectableLabel(idRect, entry.id.ToString(), _idCellStyle);
            }

            x += IdColumnWidth;

            // 各语种文本（按“显示列”过滤；默认语种列只读，需点“编辑”）
            for (int c = 0; c < languageTypes.Count; c++)
            {
                if (!IsLanguageColumnVisible(c)) continue;

                string language = languageTypes[c];
                Rect cell = new(x, rect.y, LangColumnWidth, rect.height);
                if (language == defaultLanguage) EditorGUI.DrawRect(cell, _defaultColBg);

                string value = c < entry.languages.Count ? entry.languages[c] : string.Empty;
                Rect field = new(cell.x + 3, cell.y + 3, cell.width - 6, cell.height - 6);

                if (language == defaultLanguage)
                {
                    EditorGUI.SelectableLabel(field, value, _cellFieldStyle);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    string edited = EditorGUI.DelayedTextField(field, value, _cellFieldStyle);
                    if (EditorGUI.EndChangeCheck() && edited != value)
                    {
                        long id = entry.id;
                        _pending = () => _table.SetText(id, edited, language);
                    }
                }

                x += LangColumnWidth;
            }

            // 操作：编辑 + 翻译 + 删除
            Rect opCell = new(x, rect.y, OpColumnWidth, rect.height);
            float btnY = opCell.y + 4;
            float btnH = rect.height - 8;
            float gap = 3f;
            float btnW = (opCell.width - 8 - gap * 2) / 3f;
            Rect editBtn = new(opCell.x + 4, btnY, btnW, btnH);
            Rect translateBtn = new(editBtn.xMax + gap, btnY, btnW, btnH);
            Rect deleteBtn = new(translateBtn.xMax + gap, btnY, btnW, btnH);

            using (new EditorGUI.DisabledScope(waiting))
            {
                if (GUI.Button(editBtn, "编辑", EditorStyles.miniButton))
                {
                    long id = entry.id;
                    LanguageTextEditPopup.Open(_table, id, () =>
                    {
                        LanguageTable.Save();
                        if (this != null) Repaint();
                    });
                }
            }

            bool canTranslate = !waiting
                                && _translatorNames != null
                                && _translatorNames.Length > 0
                                && _translatorIndex >= 0
                                && _translatorIndex < LanguageTable.Translators.Count;

            using (new EditorGUI.DisabledScope(!canTranslate))
            {
                if (GUI.Button(translateBtn, waiting ? "…" : "翻译", EditorStyles.miniButton))
                {
                    long id = entry.id;
                    int translatorIndex = _translatorIndex;
                    _pending = () => TranslateRow(id, translatorIndex);
                }
            }

            using (new EditorGUI.DisabledScope(waiting))
            {
                if (GUI.Button(deleteBtn, "删除", EditorStyles.miniButton))
                {
                    long id = entry.id;
                    int defaultIndex = languageTypes.IndexOf(defaultLanguage);
                    string preview = defaultIndex >= 0 && entry.languages != null &&
                                     defaultIndex < entry.languages.Count
                        ? entry.languages[defaultIndex]
                        : string.Empty;
                    if (preview.Length > 40) preview = preview.Substring(0, 40) + "…";

                    if (EditorUtility.DisplayDialog(
                            "删除文本",
                            $"确定删除该文本？\nId: {id}\n内容: {preview}",
                            "删除",
                            "取消"))
                        _pending = () => _table.RemoveText(id);
                }
            }

            // 行分隔线 + 竖向分隔线
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), _lineColorSoft);
            DrawColumnLines(rect, CountVisibleLanguageColumns(languageTypes.Count), _lineColorSoft);
        }

        private void TranslateRow(long id, int translatorIndex)
        {
            if (translatorIndex < 0 || translatorIndex >= LanguageTable.Translators.Count)
            {
                ShowNotification(new GUIContent("请先选择翻译器"));
                return;
            }

            if (_table.IsWaitTranslationResult(id)) return;

            ITextTranslator translator = LanguageTable.Translators[translatorIndex];
            _table.Translate(id, translator, _ =>
            {
                LanguageTable.Save();
                if (this != null) Repaint();
            });
            Repaint();
        }

        private void DrawColumnLines(Rect rect, int langCount, Color color)
        {
            float x = rect.x;
            DrawVLine(x, rect, color);
            x += IdColumnWidth;
            DrawVLine(x, rect, color);
            for (int i = 0; i < langCount; i++)
            {
                x += LangColumnWidth;
                DrawVLine(x, rect, color);
            }

            x += OpColumnWidth;
            DrawVLine(x, rect, color);
        }

        private static void DrawVLine(float x, Rect rect, Color color)
        {
            EditorGUI.DrawRect(new Rect(x, rect.y, 1, rect.height), color);
        }

        private void ChangeDefaultLanguage(string target)
        {
            bool ok = EditorUtility.DisplayDialog(
                "切换默认语种",
                $"文本 id 由默认语种文本经 FNV-1a 生成。\n切换默认语种为 \"{target}\" 后，" +
                "所有文本的 id 都将按新默认语种下的文本重新计算，" +
                "已经引用旧 id 的组件（LocalizationText.TextId）可能失效。\n\n是否继续？",
                "继续", "取消");
            if (!ok) return;

            _pending = () =>
            {
                _table.SetDefaultLanguage(target);
                GUI.FocusControl(null);
            };
        }

        private void AddLanguage()
        {
            string lang = _newLanguage.Trim();
            if (string.IsNullOrEmpty(lang)) return;

            if (_table.languageTypes.Contains(lang))
            {
                ShowNotification(new GUIContent($"语种 \"{lang}\" 已存在"));
                return;
            }

            _pending = () =>
            {
                _table.AddNewLanguage(lang);
                _newLanguage = string.Empty;
                GUI.FocusControl(null);
            };
        }

        private void RenameLanguage(string oldName, string newName)
        {
            if (_table.HasAnyTextIsWaitTranslationResult())
            {
                ShowNotification(new GUIContent("有文本正在翻译，无法重命名"));
                return;
            }

            if (!_table.TryRenameLanguage(oldName, newName))
            {
                ShowNotification(new GUIContent($"重命名失败：名称无效或 \"{newName}\" 已存在"));
                return;
            }

            SyncConfigLanguageName(oldName, newName);
            _renameLanguage = string.Empty;
            GUI.FocusControl(null);
            ShowNotification(new GUIContent($"已重命名：{oldName} → {newName}"));
        }

        /// <summary>
        /// 同步 I18NConfig 中的默认语言与字体配置语种名
        /// </summary>
        private void SyncConfigLanguageName(string oldName, string newName)
        {
            I18NFontConfig config = UniVueEditorSettings.instance != null
                ? UniVueEditorSettings.instance.languageConfig
                : null;
            if (config == null) return;

            Undo.RecordObject(config, "Rename I18N Language");
            if (config.defaultLanguage == oldName) config.defaultLanguage = newName;

            if (config.fonts != null)
                foreach (I18NFontConfig.CfgLanguageFont font in config.fonts)
                    if (font != null && font.language == oldName)
                        font.language = newName;

            EditorUtility.SetDirty(config);
        }

        private void ExportLanguages()
        {
            if (_table.HasAnyTextIsWaitTranslationResult())
            {
                ShowNotification(new GUIContent("有文本正在翻译，无法导出"));
                return;
            }

            if (_exporterIndex < 0 || _exporterIndex >= LanguageTable.Exporters.Count)
            {
                ShowNotification(new GUIContent("请先选择导出器"));
                return;
            }

            II10NFileExporter exporter = LanguageTable.Exporters[_exporterIndex];
            try
            {
                exporter.Export(_table);
                ShowNotification(new GUIContent($"导出完成（{exporter.GetType().Name}）"));
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                ShowNotification(new GUIContent("导出失败，详见 Console"));
            }
        }

        private void AddText()
        {
            if (string.IsNullOrEmpty(_newText) || string.IsNullOrEmpty(_table.defaultLanguage))
                return;

            string text = _newText;
            _pending = () =>
            {
                Dictionary<string, string> texts = new()
                {
                    [_table.defaultLanguage] = text
                };
                long id = _table.AddText(texts);
                if (id < 0)
                {
                    ShowNotification(new GUIContent("该文本已存在"));
                    return;
                }

                _newText = string.Empty;
                GUI.FocusControl(null);
            };
        }

        private void ExecutePending()
        {
            if (_pending == null) return;

            Action action = _pending;
            _pending = null;
            action();
            LanguageTable.Save();
            Repaint();
        }
    }

    /// <summary>
    /// 单条本地化文本编辑弹窗：默认语种 + 可选其它语种，保存时按是否改动默认文本走不同路径。
    /// </summary>
    internal sealed class LanguageTextEditPopup : EditorWindow
    {
        // 约可在不滚动情况下显示约 100 个字符（按换行估算）
        private const float TextAreaMinHeight = 88f;
        private readonly Dictionary<string, string> _originalOthers = new();
        private readonly Dictionary<string, string> _otherEdits = new();
        private string _defaultLanguage;
        private string _defaultText;
        private long _id;
        private Action _onSaved;
        private string _originalDefaultText;
        private int _otherLangIndex;
        private string[] _otherLanguages;
        private string _otherText = string.Empty;

        private LanguageTable _table;

        private string CurrentOtherLanguage =>
            _otherLanguages != null && _otherLangIndex >= 0 && _otherLangIndex < _otherLanguages.Length
                ? _otherLanguages[_otherLangIndex]
                : string.Empty;

        private void OnGUI()
        {
            if (_table == null)
            {
                Close();
                return;
            }

            EditorGUILayout.Space(6);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.LongField("Text Id", _id);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField($"默认语种（{_defaultLanguage}）", EditorStyles.boldLabel);
            _defaultText = EditorGUILayout.TextArea(_defaultText, GUILayout.MinHeight(TextAreaMinHeight));

            EditorGUILayout.Space(8);

            if (_otherLanguages == null || _otherLanguages.Length == 0)
            {
                EditorGUILayout.HelpBox("当前没有其它语种可编辑。", MessageType.Info);
            }
            else
            {
                int previousIndex = _otherLangIndex;
                EditorGUI.BeginChangeCheck();
                int nextIndex = EditorGUILayout.Popup("其它语种", _otherLangIndex, _otherLanguages);
                if (EditorGUI.EndChangeCheck() && nextIndex != previousIndex)
                {
                    CacheCurrentOtherText();
                    _otherLangIndex = nextIndex;
                    ReloadCurrentOtherText();
                    GUI.FocusControl(null);
                }

                EditorGUILayout.LabelField($"其它语种文本（{CurrentOtherLanguage}）", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();
                string editedOther = EditorGUILayout.TextArea(_otherText, GUILayout.MinHeight(TextAreaMinHeight));
                if (EditorGUI.EndChangeCheck())
                {
                    _otherText = editedOther;
                    CacheCurrentOtherText();
                }
            }

            GUILayout.FlexibleSpace();

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("取消", GUILayout.Width(80), GUILayout.Height(24))) Close();

                if (GUILayout.Button("保存", GUILayout.Width(80), GUILayout.Height(24))) Save();
            }

            EditorGUILayout.Space(8);
        }

        public static void Open(LanguageTable table, long id, Action onSaved)
        {
            if (table == null) return;
            if (table.IsWaitTranslationResult(id))
            {
                EditorUtility.DisplayDialog("编辑文本", "该文本正在翻译中，暂不可编辑。", "确定");
                return;
            }

            LanguageTextEditPopup window = CreateInstance<LanguageTextEditPopup>();
            window.titleContent = new GUIContent($"编辑文本 (Id={id})");
            window.minSize = new Vector2(520, 360);
            window._table = table;
            window._id = id;
            window._onSaved = onSaved;
            window.Initialize();
            window.ShowUtility();
            window.Focus();
        }

        private void Initialize()
        {
            _defaultLanguage = _table.defaultLanguage;
            _originalDefaultText = _table.GetText(_id, _defaultLanguage) ?? string.Empty;
            _defaultText = _originalDefaultText;

            _otherLanguages = _table.languageTypes
                .Where(l => l != _defaultLanguage)
                .ToArray();

            _otherEdits.Clear();
            _originalOthers.Clear();
            foreach (string lang in _otherLanguages)
            {
                string text = _table.GetText(_id, lang) ?? string.Empty;
                _originalOthers[lang] = text;
                _otherEdits[lang] = text;
            }

            _otherLangIndex = 0;
            ReloadCurrentOtherText();
        }

        private void ReloadCurrentOtherText()
        {
            string lang = CurrentOtherLanguage;
            _otherText = string.IsNullOrEmpty(lang)
                ? string.Empty
                : _otherEdits.GetValueOrDefault(lang, string.Empty);
        }

        private void CacheCurrentOtherText()
        {
            string lang = CurrentOtherLanguage;
            if (!string.IsNullOrEmpty(lang)) _otherEdits[lang] = _otherText ?? string.Empty;
        }

        private void Save()
        {
            CacheCurrentOtherText();

            if (_table.IsWaitTranslationResult(_id))
            {
                EditorUtility.DisplayDialog("保存失败", "该文本正在翻译中，无法保存。", "确定");
                return;
            }

            bool defaultChanged = (_defaultText ?? string.Empty) != (_originalDefaultText ?? string.Empty);
            bool anyOtherChanged = false;
            foreach (KeyValuePair<string, string> kv in _otherEdits)
            {
                string original = _originalOthers.GetValueOrDefault(kv.Key, string.Empty);
                if (kv.Value != original)
                {
                    anyOtherChanged = true;
                    break;
                }
            }

            if (!defaultChanged && !anyOtherChanged)
            {
                Close();
                return;
            }

            if (defaultChanged)
            {
                int choice = EditorUtility.DisplayDialogComplex(
                    "默认文本已修改",
                    "默认语种文本已更改。请选择如何保存：\n\n" +
                    "• 更新：覆盖当前条目（Id 可能变化）\n" +
                    "• 新加：将修改后的默认文本作为新条目添加",
                    "更新",
                    "取消",
                    "新加");

                if (choice == 1) return;

                if (choice == 0)
                {
                    long newId = _table.UpdateText(_id, _defaultText);
                    if (newId < 0)
                    {
                        EditorUtility.DisplayDialog("保存失败", "更新默认文本失败（可能正在翻译中）。", "确定");
                        return;
                    }

                    ApplyOtherEdits(newId);
                    LanguageTable.Save();
                    _onSaved?.Invoke();
                    Close();
                    return;
                }

                Dictionary<string, string> texts = new()
                {
                    [_defaultLanguage] = _defaultText
                };
                foreach (KeyValuePair<string, string> kv in _otherEdits) texts[kv.Key] = kv.Value;

                long addedId = _table.AddText(texts);
                if (addedId < 0)
                {
                    EditorUtility.DisplayDialog("保存失败", "新加失败：语言表中可能已存在相同默认文本。", "确定");
                    return;
                }

                LanguageTable.Save();
                _onSaved?.Invoke();
                Close();
                return;
            }

            ApplyOtherEdits(_id);
            LanguageTable.Save();
            _onSaved?.Invoke();
            Close();
        }

        private void ApplyOtherEdits(long id)
        {
            foreach (KeyValuePair<string, string> kv in _otherEdits)
            {
                string original = _originalOthers.GetValueOrDefault(kv.Key, string.Empty);
                if (kv.Value != original) _table.SetText(id, kv.Value, kv.Key);
            }
        }
    }
}