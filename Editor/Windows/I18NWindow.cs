using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UniVue.I18n;

namespace UniVue.Editor
{
    /// <summary>
    /// 多语言（I18N）编辑窗口：以表格形式管理 <see cref="LanguageTable"/> 中的所有文本与语种，
    /// 同时可编辑 <see cref="I18NConfig"/> 的相关配置。
    /// </summary>
    public sealed class I18NWindow : EditorWindow
    {
        private const float IdColumnWidth = 150f;
        private const float LangColumnWidth = 220f;
        private const float OpColumnWidth = 64f;
        private const float RowHeight = 26f;
        private const float HeaderHeight = 28f;

        private LanguageTable _table;
        private Vector2 _scroll;
        private string _newLanguage = string.Empty;
        private string _newText = string.Empty;
        private int _deleteLangIndex;

        // 需要在本次 OnGUI 布局结束后再执行的结构性变更，避免 GUILayout 布局错位
        private Action _pending;
        private bool _wasWaitingTranslation;

        // I18NConfig 内嵌编辑器
        private bool _configFoldout = true;
        private UnityEditor.Editor _configEditor;

        // 样式（懒加载）
        private GUIStyle _headerCellStyle;
        private GUIStyle _idCellStyle;
        private GUIStyle _cellFieldStyle;
        private bool _stylesReady;

        // 颜色
        private Color _headerBg;
        private Color _rowOddBg;
        private Color _defaultColBg;
        private Color _lineColor;
        private Color _lineColorSoft;

        [MenuItem("UniVue/Windows/I18N Editor Window")]
        public static void Open()
        {
            I18NWindow window = GetWindow<I18NWindow>();
            window.titleContent = new GUIContent("I18N Editor Window");
            window.minSize = new Vector2(560, 360);
            window.Show();
        }

        private void OnEnable()
        {
            _table = LanguageTable.GetOrCreate();
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

        private void OnEditorUpdate()
        {
            if (_table == null) return;
            // 翻译进行中或刚结束时刷新，以便解除禁用并显示最新译文
            bool waiting = _table.HasAnyTextIsWaitTranslationResult();
            if (waiting || _wasWaitingTranslation)
            {
                Repaint();
            }
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

        private void OnGUI()
        {
            EnsureStyles();

            if (_table == null)
            {
                _table = LanguageTable.GetOrCreate();
            }

            DrawConfigSection();
            DrawSettingsBar();

            EditorGUILayout.Space(4);

            if (_table.languageTypes == null || _table.languageTypes.Count == 0)
            {
                EditorGUILayout.HelpBox("尚未定义任何语种，请先在上方添加一个语种。", MessageType.Info);
            }
            else
            {
                DrawTable();
            }

            ExecutePending();
        }

        private void DrawSettingsBar()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string[] langs = _table.languageTypes.ToArray();

                // 默认语种（任意文本正在翻译时不允许切换）
                bool anyWaiting = _table.HasAnyTextIsWaitTranslationResult();
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("默认语种", GUILayout.Width(60));
                    int current = Mathf.Max(0, Array.IndexOf(langs, _table.defaultLanguage));
                    using (new EditorGUI.DisabledScope(langs.Length == 0 || anyWaiting))
                    {
                        EditorGUI.BeginChangeCheck();
                        int next = EditorGUILayout.Popup(current, langs, GUILayout.Width(180));
                        if (EditorGUI.EndChangeCheck() && next >= 0 && next < langs.Length && next != current)
                        {
                            string target = langs[next];
                            ChangeDefaultLanguage(target);
                        }
                    }
                    if (anyWaiting)
                    {
                        EditorGUILayout.LabelField("（有文本正在翻译，暂不可切换）", EditorStyles.miniLabel);
                    }
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
                    string[] removable = langs.Where(l => l != _table.defaultLanguage).ToArray();
                    using (new EditorGUI.DisabledScope(removable.Length == 0))
                    {
                        _deleteLangIndex = Mathf.Clamp(_deleteLangIndex, 0, Mathf.Max(0, removable.Length - 1));
                        _deleteLangIndex = EditorGUILayout.Popup(_deleteLangIndex, removable, GUILayout.Width(180));
                        if (GUILayout.Button("删除语种", GUILayout.Width(90)) && removable.Length > 0)
                        {
                            string lang = removable[_deleteLangIndex];
                            if (EditorUtility.DisplayDialog("删除语种", $"确定删除语种 \"{lang}\" 及其所有文本内容？", "删除", "取消"))
                            {
                                _pending = () =>
                                {
                                    _table.RemoveLanguage(lang);
                                    _deleteLangIndex = 0;
                                    GUI.FocusControl(null);
                                };
                            }
                        }
                    }
                    if (removable.Length == 0)
                    {
                        EditorGUILayout.LabelField("（默认语种不可删除）", EditorStyles.miniLabel);
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
                        if (GUILayout.Button("添加语种", GUILayout.Width(90)))
                        {
                            AddLanguage();
                        }
                    }
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space(2);

                // 添加文本
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("新增文本", GUILayout.Width(60));
                    _newText = EditorGUILayout.TextField(_newText, GUILayout.Width(180));
                    using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_newText) || string.IsNullOrEmpty(_table.defaultLanguage)))
                    {
                        if (GUILayout.Button("添加文本", GUILayout.Width(90)))
                        {
                            AddText();
                        }
                    }
                    EditorGUILayout.LabelField("（以默认语种文本作为内容）", EditorStyles.miniLabel);
                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void DrawConfigSection()
        {
            UniVueEditorSettings settings = UniVueEditorSettings.instance;

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                _configFoldout = EditorGUILayout.Foldout(_configFoldout, "多语言配置 (I18NConfig)", true);
                if (!_configFoldout) return;

                I18NConfig config = settings.languageConfig;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUI.BeginChangeCheck();
                    config = (I18NConfig)EditorGUILayout.ObjectField(
                        "配置资源", settings.languageConfig, typeof(I18NConfig), false);
                    if (EditorGUI.EndChangeCheck())
                    {
                        settings.languageConfig = config;
                        settings.SaveSettings();
                        RecreateConfigEditor(config);
                    }

                    if (GUILayout.Button("创建", GUILayout.Width(60)))
                    {
                        _pending = CreateConfigAsset;
                    }
                }

                if (config == null)
                {
                    EditorGUILayout.HelpBox("未指定 I18NConfig 资源，可点击右侧“创建”新建一个。", MessageType.Info);
                    return;
                }

                if (_configEditor == null || _configEditor.target != config)
                {
                    RecreateConfigEditor(config);
                }

                EditorGUILayout.Space(2);
                _configEditor.OnInspectorGUI();
            }
        }

        private void RecreateConfigEditor(I18NConfig config)
        {
            if (_configEditor != null)
            {
                DestroyImmediate(_configEditor);
                _configEditor = null;
            }
            if (config != null)
            {
                _configEditor = UnityEditor.Editor.CreateEditor(config);
            }
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

            I18NConfig config = ScriptableObject.CreateInstance<I18NConfig>();
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

            float totalWidth = IdColumnWidth + LangColumnWidth * languageTypes.Count + OpColumnWidth;

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
            else
            {
                for (int r = 0; r < languages.Count; r++)
                {
                    Rect rowRect = GUILayoutUtility.GetRect(totalWidth, RowHeight, GUILayout.Width(totalWidth));
                    DrawDataRow(rowRect, languages[r], r, languageTypes, defaultLanguage);
                }

                // 底部收尾边框
                Rect bottom = GUILayoutUtility.GetRect(totalWidth, 1, GUILayout.Width(totalWidth));
                EditorGUI.DrawRect(new Rect(bottom.x, bottom.y, totalWidth, 1), _lineColor);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeaderRow(Rect rect, List<string> languageTypes, string defaultLanguage)
        {
            EditorGUI.DrawRect(rect, _headerBg);

            float x = rect.x;
            EditorGUI.LabelField(new Rect(x, rect.y, IdColumnWidth, rect.height), "Text Id", _headerCellStyle);
            x += IdColumnWidth;

            foreach (string lang in languageTypes)
            {
                bool isDefault = lang == defaultLanguage;
                Rect cell = new Rect(x, rect.y, LangColumnWidth, rect.height);
                if (isDefault) EditorGUI.DrawRect(cell, _defaultColBg);
                EditorGUI.LabelField(cell, isDefault ? $"★ {lang}" : lang, _headerCellStyle);
                x += LangColumnWidth;
            }

            EditorGUI.LabelField(new Rect(x, rect.y, OpColumnWidth, rect.height), "操作", _headerCellStyle);

            // 表头底部实线 + 竖向分隔线
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), _lineColor);
            DrawColumnLines(rect, languageTypes.Count, _lineColor);
        }

        private void DrawDataRow(Rect rect, Languages entry, int index, List<string> languageTypes, string defaultLanguage)
        {
            if (index % 2 == 1)
            {
                EditorGUI.DrawRect(rect, _rowOddBg);
            }

            bool waiting = _table.IsWaitTranslationResult(entry.id);
            float x = rect.x;

            // Id（只读）；翻译中在 Id 旁提示
            string idLabel = waiting ? $"{entry.id} (翻译中)" : entry.id.ToString();
            EditorGUI.LabelField(new Rect(x, rect.y, IdColumnWidth, rect.height), idLabel, _idCellStyle);
            x += IdColumnWidth;

            // 各语种文本
            for (int c = 0; c < languageTypes.Count; c++)
            {
                string language = languageTypes[c];
                Rect cell = new Rect(x, rect.y, LangColumnWidth, rect.height);
                if (language == defaultLanguage) EditorGUI.DrawRect(cell, _defaultColBg);

                string value = c < entry.languages.Count ? entry.languages[c] : string.Empty;
                Rect field = new Rect(cell.x + 3, cell.y + 3, cell.width - 6, cell.height - 6);

                // 翻译中：默认语种列禁止修改（与 LanguageTable 注释一致）
                bool lockDefault = waiting && language == defaultLanguage;
                using (new EditorGUI.DisabledScope(lockDefault))
                {
                    EditorGUI.BeginChangeCheck();
                    string edited = EditorGUI.DelayedTextField(field, value, _cellFieldStyle);
                    if (EditorGUI.EndChangeCheck() && edited != value && !lockDefault)
                    {
                        long id = entry.id;
                        if (language == defaultLanguage)
                            _pending = () => _table.UpdateText(id, edited);
                        else
                            _pending = () => _table.SetText(id, edited, language);
                    }
                }

                x += LangColumnWidth;
            }

            // 删除（翻译中禁止）
            Rect opCell = new Rect(x, rect.y, OpColumnWidth, rect.height);
            Rect btn = new Rect(opCell.x + 6, opCell.y + 4, opCell.width - 12, rect.height - 8);
            using (new EditorGUI.DisabledScope(waiting))
            {
                if (GUI.Button(btn, "删除", EditorStyles.miniButton))
                {
                    long id = entry.id;
                    _pending = () => _table.RemoveText(id);
                }
            }

            // 行分隔线 + 竖向分隔线
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), _lineColorSoft);
            DrawColumnLines(rect, languageTypes.Count, _lineColorSoft);
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

        private void AddText()
        {
            if (string.IsNullOrEmpty(_newText) || string.IsNullOrEmpty(_table.defaultLanguage))
                return;

            string text = _newText;
            _pending = () =>
            {
                Dictionary<string, string> texts = new Dictionary<string, string>
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
}
