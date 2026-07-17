using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UniVue.I18n;

namespace UniVue.Editor
{
    [CustomEditor(typeof(I18NConfig))]
    public sealed class I18NConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _defaultLanguageProp;
        private SerializedProperty _fontsProp;

        private void OnEnable()
        {
            _defaultLanguageProp = serializedObject.FindProperty("defaultLanguage");
            _fontsProp = serializedObject.FindProperty("fonts");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            LanguageTable table = LanguageTable.GetOrCreate();
            List<string> languageTypes = table.languageTypes ?? new List<string>();

            DrawDefaultLanguage(table);

            EditorGUILayout.Space(6);

            DrawFonts(languageTypes);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDefaultLanguage(LanguageTable table)
        {
            // 默认语言只读，数据来自 LanguageTable
            string tableDefault = table.defaultLanguage ?? string.Empty;
            if (_defaultLanguageProp.stringValue != tableDefault)
            {
                _defaultLanguageProp.stringValue = tableDefault;
            }

            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("默认语言", GUILayout.Width(90));
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.LabelField(
                        string.IsNullOrEmpty(table.defaultLanguage) ? "(未设置)" : table.defaultLanguage,
                        EditorStyles.boldLabel);
                }
            }
        }

        private void DrawFonts(List<string> languageTypes)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("字体配置", EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    using (new EditorGUI.DisabledScope(_fontsProp.arraySize >= languageTypes.Count))
                    {
                        if (GUILayout.Button("＋ 添加", GUILayout.Width(70)))
                        {
                            AddFontElement(languageTypes);
                        }
                    }
                }

                if (languageTypes.Count == 0)
                {
                    EditorGUILayout.HelpBox("LanguageTable 尚未定义任何语种，请先在 I18N Editor 窗口中添加。", MessageType.Info);
                    return;
                }

                if (_fontsProp.arraySize == 0)
                {
                    EditorGUILayout.LabelField("暂无字体配置。", EditorStyles.centeredGreyMiniLabel);
                    return;
                }

                int removeIndex = -1;
                for (int i = 0; i < _fontsProp.arraySize; i++)
                {
                    if (DrawFontElement(i, languageTypes))
                    {
                        removeIndex = i;
                    }
                }

                if (removeIndex >= 0)
                {
                    _fontsProp.DeleteArrayElementAtIndex(removeIndex);
                }
            }
        }

        /// <summary>
        /// 绘制单个字体项，返回 true 表示请求删除该项
        /// </summary>
        private bool DrawFontElement(int index, List<string> languageTypes)
        {
            SerializedProperty element = _fontsProp.GetArrayElementAtIndex(index);
            SerializedProperty languageProp = element.FindPropertyRelative("language");
            SerializedProperty fontProp = element.FindPropertyRelative("font");

            bool requestRemove = false;

            using (new EditorGUILayout.HorizontalScope())
            {
                // 该项可选择的语种 = 全部语种 - 其他项已占用的语种（不允许一个语种多个字体）
                HashSet<string> usedByOthers = new HashSet<string>();
                for (int j = 0; j < _fontsProp.arraySize; j++)
                {
                    if (j == index) continue;
                    string used = _fontsProp.GetArrayElementAtIndex(j).FindPropertyRelative("language").stringValue;
                    if (!string.IsNullOrEmpty(used)) usedByOthers.Add(used);
                }

                List<string> options = languageTypes.Where(l => !usedByOthers.Contains(l)).ToList();

                int selected = options.IndexOf(languageProp.stringValue);
                if (selected < 0) selected = 0;

                int next = EditorGUILayout.Popup(selected, options.ToArray(), GUILayout.Width(160));
                languageProp.stringValue = options.Count > 0 ? options[Mathf.Clamp(next, 0, options.Count - 1)] : string.Empty;

                EditorGUILayout.PropertyField(fontProp, GUIContent.none);

                if (GUILayout.Button("×", GUILayout.Width(22)))
                {
                    requestRemove = true;
                }
            }

            return requestRemove;
        }

        private void AddFontElement(List<string> languageTypes)
        {
            HashSet<string> used = new HashSet<string>();
            for (int i = 0; i < _fontsProp.arraySize; i++)
            {
                string lang = _fontsProp.GetArrayElementAtIndex(i).FindPropertyRelative("language").stringValue;
                if (!string.IsNullOrEmpty(lang)) used.Add(lang);
            }

            string firstAvailable = languageTypes.FirstOrDefault(l => !used.Contains(l)) ?? string.Empty;

            int newIndex = _fontsProp.arraySize;
            _fontsProp.InsertArrayElementAtIndex(newIndex);
            SerializedProperty element = _fontsProp.GetArrayElementAtIndex(newIndex);
            element.FindPropertyRelative("language").stringValue = firstAvailable;
            element.FindPropertyRelative("font").objectReferenceValue = null;
        }
    }
}
