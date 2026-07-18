#region

using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniVue.I18n;
using UniVue.UI.Widgets;
using UniVue.Utils;

#endregion

namespace UniVue.Editor
{
    [CustomEditor(typeof(LocalizationText))]
    public sealed class LocalizationTextEditor : UnityEditor.Editor
    {
        private TMP_FontAsset _cachedFont;

        // 预览前缓存，关闭 Inspector 时还原，避免预览写入场景/预制体
        private string _cachedText;
        private I18NFontConfig _config;

        private string _defaultLanguage;
        private string _defaultText;
        private long _entryId;
        private bool _existsInTable;
        private bool _hasCache;
        private int _languageIndex;
        private string[] _languageTypes;
        private string _otherText;
        private LanguageTable _table;

        private long _textId;
        private TMP_Text _tmpText;
        private int _translatorIndex;
        private string[] _translatorNames;
        private bool _wasWaitingTranslation;

        private string CurrentLanguage =>
            _languageTypes != null && _languageIndex >= 0 && _languageIndex < _languageTypes.Length
                ? _languageTypes[_languageIndex]
                : string.Empty;

        private void OnEnable()
        {
            LocalizationText component = (LocalizationText)target;
            _tmpText = component.GetComponent<TMP_Text>();
            _table = LanguageTable.GetOrCreate();
            _config = UniVueEditorSettings.instance != null
                ? UniVueEditorSettings.instance.languageConfig
                : null;

            _defaultLanguage = !string.IsNullOrEmpty(_table.defaultLanguage)
                ? _table.defaultLanguage
                : _config != null
                    ? configDefaultLanguage(_config)
                    : string.Empty;

            _languageTypes = _table.languageTypes != null && _table.languageTypes.Count > 0
                ? _table.languageTypes.ToArray()
                : Array.Empty<string>();

            _translatorNames = new string[LanguageTable.Translators.Count];
            for (int i = 0; i < LanguageTable.Translators.Count; i++)
                _translatorNames[i] = LanguageTable.Translators[i].GetType().Name;

            _languageIndex = Mathf.Max(0, Array.IndexOf(_languageTypes, _defaultLanguage));

            CachePreviewState();
            ReloadFromTmpText();

            EditorSceneManager.sceneSaving += OnSceneSaving;
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            EditorSceneManager.sceneSaving -= OnSceneSaving;
            RestorePreviewState();
        }

        private static string configDefaultLanguage(I18NFontConfig config)
        {
            return config.defaultLanguage ?? string.Empty;
        }

        private void OnEditorUpdate()
        {
            if (_table == null || !_existsInTable) return;
            bool waiting = _table.IsWaitTranslationResult(_entryId);
            if (waiting || _wasWaitingTranslation) Repaint();
            _wasWaitingTranslation = waiting;
        }

        private void OnSceneSaving(Scene scene, string path)
        {
            RestorePreviewState();
            CachePreviewState();
        }

        private void CachePreviewState()
        {
            if (_tmpText == null) return;
            _cachedText = _tmpText.text;
            _cachedFont = _tmpText.font;
            _hasCache = true;
        }

        private void RestorePreviewState()
        {
            if (!_hasCache || _tmpText == null) return;
            _tmpText.text = _cachedText;
            _tmpText.font = _cachedFont;
            _hasCache = false;
        }

        /// <summary>
        /// 以 TMP_Text 当前文本作为默认语种内容，生成 id 并查表。
        /// </summary>
        private void ReloadFromTmpText()
        {
            _defaultText = _tmpText != null ? _tmpText.text ?? string.Empty : string.Empty;
            // 若正在预览非默认语种，用缓存的默认文本作为权威内容
            if (_hasCache && CurrentLanguage != _defaultLanguage) _defaultText = _cachedText ?? string.Empty;

            LookupByDefaultText();
            _otherText = _existsInTable ? _table.GetText(_entryId, CurrentLanguage) : string.Empty;
            if (CurrentLanguage == _defaultLanguage) _otherText = _defaultText;
        }

        private void LookupByDefaultText()
        {
            _textId = string.IsNullOrEmpty(_defaultText) ? 0 : CoreUtils.GenerateUniqueId(_defaultText);
            _entryId = _textId;
            _existsInTable = _entryId != 0 && _table.languages != null
                                           && _table.languages.Exists(x => x.id == _entryId);

            if (_existsInTable)
            {
                SerializedProperty prop = serializedObject.FindProperty("<TextId>k__BackingField");
                if (prop != null && prop.longValue != _entryId) ApplyTextId(_entryId);
            }
        }

        private void RefreshTextIdFromDefault()
        {
            _textId = string.IsNullOrEmpty(_defaultText) ? 0 : CoreUtils.GenerateUniqueId(_defaultText);
        }

        public override void OnInspectorGUI()
        {
            if (_tmpText == null)
            {
                EditorGUILayout.HelpBox("缺少 TMP_Text 组件。", MessageType.Error);
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.LongField("Text Id", _textId);
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("Language", I18N.Language);
                return;
            }

            if (!_existsInTable && !string.IsNullOrEmpty(_defaultText))
                EditorGUILayout.HelpBox("语言表中未找到该默认文本，可通过“新加”写入语言表。", MessageType.Info);

            EditorGUILayout.Space(4);

            if (_languageTypes.Length == 0)
            {
                EditorGUILayout.HelpBox("LanguageTable 尚未定义任何语种，请先在 I18N Editor 中添加。", MessageType.Warning);
                return;
            }

            // 预览语种
            EditorGUI.BeginChangeCheck();
            _languageIndex = EditorGUILayout.Popup("预览语种", _languageIndex, _languageTypes);
            if (EditorGUI.EndChangeCheck()) OnPreviewLanguageChanged();

            EditorGUILayout.Space(6);

            bool waiting = _existsInTable && _table.IsWaitTranslationResult(_entryId);
            if (waiting) EditorGUILayout.HelpBox("该文本正在翻译中，默认文本暂不可修改。", MessageType.Warning);

            // 默认语种文本（翻译中禁止修改）
            EditorGUILayout.LabelField($"默认语种（{_defaultLanguage}）", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(waiting))
            {
                EditorGUI.BeginChangeCheck();
                _defaultText = EditorGUILayout.TextArea(_defaultText, GUILayout.MinHeight(48));
                if (EditorGUI.EndChangeCheck())
                {
                    // 默认文本作为资源真实内容写回 TMP；语言表需点“更新/新加”才写入
                    if (_tmpText != null)
                    {
                        _tmpText.text = _defaultText;
                        _cachedText = _defaultText;
                        _hasCache = true;
                        EditorUtility.SetDirty(_tmpText);
                    }

                    RefreshTextIdFromDefault();
                    if (CurrentLanguage == _defaultLanguage) _otherText = _defaultText;
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(!_existsInTable || waiting))
                {
                    if (GUILayout.Button("更新")) UpdateDefaultText();
                }

                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_defaultText) || waiting))
                {
                    if (GUILayout.Button("新加")) AddAsNewText();
                }
            }

            EditorGUILayout.Space(6);

            // 当前预览语种文本
            EditorGUILayout.LabelField($"预览语种（{CurrentLanguage}）", EditorStyles.boldLabel);
            bool isDefault = CurrentLanguage == _defaultLanguage;
            using (new EditorGUI.DisabledScope(isDefault))
            {
                EditorGUI.BeginChangeCheck();
                string edited = EditorGUILayout.TextArea(_otherText, GUILayout.MinHeight(48));
                if (EditorGUI.EndChangeCheck())
                {
                    _otherText = edited;
                    if (!isDefault)
                    {
                        SyncNonDefaultText();
                        ApplyPreview(_otherText, CurrentLanguage);
                    }
                }
            }

            if (isDefault)
                EditorGUILayout.HelpBox("预览语种为默认语种时，请在上方编辑默认文本，并通过“更新/新加”写入语言表。", MessageType.None);
            else
                DrawTranslatorBar();
        }

        private void DrawTranslatorBar()
        {
            EditorGUILayout.Space(4);

            if (_translatorNames == null || _translatorNames.Length == 0)
            {
                EditorGUILayout.HelpBox("未找到实现 ITextTranslator 的翻译器。", MessageType.Info);
                return;
            }

            bool waiting = _existsInTable && _table.IsWaitTranslationResult(_entryId);
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(waiting))
                {
                    _translatorIndex = EditorGUILayout.Popup("翻译器", _translatorIndex, _translatorNames);
                }

                using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_defaultText) || !_existsInTable || waiting))
                {
                    if (GUILayout.Button(waiting ? "翻译中…" : "翻译", GUILayout.Width(80))) TranslateCurrent();
                }
            }
        }

        private void TranslateCurrent()
        {
            ITextTranslator translator = GetSelectedTranslator();
            if (translator == null || !_existsInTable) return;
            if (_table.IsWaitTranslationResult(_entryId)) return;

            long id = _entryId;
            _table.Translate(id, translator, _ =>
            {
                LanguageTable.Save();
                if (this == null) return;

                // 仍指向同一条目时刷新预览语种文本
                if (_entryId == id && CurrentLanguage != _defaultLanguage)
                {
                    _otherText = _table.GetText(_entryId, CurrentLanguage);
                    ApplyPreview(_otherText, CurrentLanguage);
                }

                Repaint();
            });
            Repaint();
            GUI.FocusControl(null);
        }

        private ITextTranslator GetSelectedTranslator()
        {
            if (_translatorIndex >= 0 && _translatorIndex < LanguageTable.Translators.Count)
                return LanguageTable.Translators[_translatorIndex];
            return null;
        }

        private void OnPreviewLanguageChanged()
        {
            if (CurrentLanguage == _defaultLanguage)
                _otherText = _defaultText;
            else if (_existsInTable)
                _otherText = _table.GetText(_entryId, CurrentLanguage);
            else
                _otherText = string.Empty;

            ApplyPreview(
                CurrentLanguage == _defaultLanguage ? _defaultText : _otherText,
                CurrentLanguage);
            GUI.FocusControl(null);
        }

        /// <summary>
        /// 仅预览：改 TMP 文本与字体，不写入场景/预制体（关闭 Inspector 时会还原）。
        /// </summary>
        private void ApplyPreview(string text, string language)
        {
            if (_tmpText == null) return;

            _tmpText.text = text ?? string.Empty;

            TMP_FontAsset font = _config != null ? _config.GetFontAsync(language, CancellationToken.None).Result : null;
            if (font != null)
                _tmpText.font = font;
            else if (_hasCache) _tmpText.font = _cachedFont;

            // 预览不写入资源：清除 dirty，避免误保存
            EditorUtility.ClearDirty(_tmpText);
            EditorUtility.ClearDirty(_tmpText.gameObject);
            SceneView.RepaintAll();
        }

        private void SyncNonDefaultText()
        {
            if (!_existsInTable)
            {
                Debug.LogWarning("语言表中尚无该文本条目，请先“新加”默认文本后再编辑其他语种。");
                return;
            }

            _table.SetText(_entryId, _otherText, CurrentLanguage);
            LanguageTable.Save();
        }

        private void UpdateDefaultText()
        {
            if (!_existsInTable || _entryId == 0)
            {
                Debug.LogWarning("语言表中不存在该文本，无法更新，请使用“新加”。");
                return;
            }

            long newId = _table.UpdateText(_entryId, _defaultText);
            LanguageTable.Save();

            _entryId = newId;
            _textId = newId;
            _existsInTable = true;
            CommitAuthoringText(_defaultText);

            if (CurrentLanguage == _defaultLanguage)
                _otherText = _defaultText;
            else
                _otherText = _table.GetText(_entryId, CurrentLanguage);

            _languageIndex = Mathf.Max(0, Array.IndexOf(_languageTypes, _defaultLanguage));
            GUI.FocusControl(null);
        }

        private void AddAsNewText()
        {
            if (string.IsNullOrEmpty(_defaultText))
            {
                Debug.LogWarning("默认文本为空，无法新加。");
                return;
            }

            if (string.IsNullOrEmpty(_table.defaultLanguage)) _table.defaultLanguage = _defaultLanguage;

            Dictionary<string, string> texts = new()
            {
                [_table.defaultLanguage] = _defaultText
            };

            string current = CurrentLanguage;
            if (!string.IsNullOrEmpty(current) && current != _table.defaultLanguage &&
                !string.IsNullOrEmpty(_otherText)) texts[current] = _otherText;

            long newId = _table.AddText(texts);
            if (newId < 0)
                return;

            LanguageTable.Save();

            _entryId = newId;
            _textId = newId;
            _existsInTable = true;
            CommitAuthoringText(_defaultText);
            _languageIndex = Mathf.Max(0, Array.IndexOf(_languageTypes, _defaultLanguage));
            GUI.FocusControl(null);
        }

        /// <summary>
        /// 将默认语种文本写回 TMP（作为资源真实内容，允许保存），并重置预览缓存。
        /// </summary>
        private void CommitAuthoringText(string defaultText)
        {
            if (_tmpText == null) return;

            _tmpText.text = defaultText ?? string.Empty;
            if (_config != null)
            {
                TMP_FontAsset font = _config.GetFontAsync(_defaultLanguage, CancellationToken.None).Result;
                if (font != null) _tmpText.font = font;
                else if (_hasCache) _tmpText.font = _cachedFont;
            }
            else if (_hasCache)
            {
                _tmpText.font = _cachedFont;
            }

            _cachedText = _tmpText.text;
            _cachedFont = _tmpText.font;
            _hasCache = true;

            EditorUtility.SetDirty(_tmpText);
            ApplyTextId(_textId);
        }

        private void ApplyTextId(long id)
        {
            SerializedProperty prop = serializedObject.FindProperty("<TextId>k__BackingField");
            if (prop != null)
            {
                prop.longValue = id;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}