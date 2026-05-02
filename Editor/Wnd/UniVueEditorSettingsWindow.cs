using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UniVue.Editor
{
    public sealed class UniVueEditorSettingsWindow : EditorWindow
    {
        private static readonly UGUIType[] AllUGUITypes = (UGUIType[])Enum.GetValues(typeof(UGUIType));

        private static readonly UGUIType[] DefaultUITypes =
        {
            UGUIType.Image, UGUIType.RawImage, UGUIType.Text,
            UGUIType.Button, UGUIType.Toggle, UGUIType.Slider,
            UGUIType.Dropdown, UGUIType.InputField,
            UGUIType.TMP_InputField, UGUIType.TMP_Dropdown,
            UGUIType.TextMeshProUGUI, UGUIType.ToggleGroup
        };

        private static readonly string[] DefaultTypeSuffixes =
        {
            "Image,Img,img,image",
            "RawImage,RImg,rimg,rawimage",
            "Txt,Text,txt,text",
            "Button,Btn,button,btn",
            "Toggle,toggle",
            "Slider,slider",
            "Dropdown,dropdown",
            "Input,input",
            "Input,input",
            "Dropdown,dropdown",
            "Txt,Text,txt,text",
            "ToggleGroup"
        };

        private Vector2 _scrollPos;

        private UniVueEditorSettings _settings;
        private bool _typeSuffixFoldout = true;

        private void OnEnable()
        {
            _settings = UniVueEditorSettings.instance;
        }

        private void OnDestroy()
        {
            _settings.SaveSettings();
        }

        private void OnGUI()
        {
            if (_settings == null)
                _settings = UniVueEditorSettings.instance;

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("UniVue Editor Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(6);

            EditorGUI.BeginChangeCheck();

            DrawDirectoryField("RedPointKey 导出目录", ref _settings.redPointKeyExportDirectory,
                               "选择 RedPointKey.g.cs 存放目录");

            EditorGUILayout.Space(4);

            _settings.redPointKeyNamespace = EditorGUILayout.TextField(
                                                                       new GUIContent("RedPointKey 命名空间",
                                                                            "RedPointKey.g.cs 的命名空间"),
                                                                       _settings.redPointKeyNamespace ?? "");

            EditorGUILayout.Space(4);

            EditorGUILayout.Space(8);

            DrawTypeSuffixMap();

            if (EditorGUI.EndChangeCheck())
                _settings.SaveSettings();

            EditorGUILayout.EndScrollView();
        }

        [MenuItem("UniVue/Windows/EditorSettings")]
        public static void ShowWindow()
        {
            UniVueEditorSettingsWindow wnd = GetWindow<UniVueEditorSettingsWindow>("UniVue 设置");
            wnd.minSize = new Vector2(460, 200);
            wnd.Show();
        }

        private void DrawTypeSuffixMap()
        {
            _settings.uiTypes ??= Array.Empty<UGUIType>();
            _settings.typeSuffixes ??= Array.Empty<string>();

            int count = _settings.uiTypes.Length;
            HashSet<UGUIType> usedTypes = new(_settings.uiTypes);
            bool allUsed = usedTypes.Count >= AllUGUITypes.Length;

            EditorGUILayout.BeginHorizontal();
            _typeSuffixFoldout = EditorGUILayout.Foldout(_typeSuffixFoldout, $"UI 类型后缀映射 ({count})", true);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("使用默认设置", GUILayout.Width(120), GUILayout.Height(22)))
            {
                _settings.uiTypes = (UGUIType[])DefaultUITypes.Clone();
                _settings.typeSuffixes = (string[])DefaultTypeSuffixes.Clone();
                GUI.changed = true;
            }

            EditorGUI.BeginDisabledGroup(allUsed);
            if (GUILayout.Button("+", GUILayout.Width(24)))
            {
                UGUIType newType = AllUGUITypes.First(t => !usedTypes.Contains(t));
                ArrayUtility.Add(ref _settings.uiTypes, newType);
                ArrayUtility.Add(ref _settings.typeSuffixes, "");
                GUI.changed = true;
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            if (!_typeSuffixFoldout) return;

            EditorGUI.indentLevel++;
            int removeIndex = -1;

            for (int i = 0; i < _settings.uiTypes.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();

                UGUIType current = _settings.uiTypes[i];
                UGUIType[] available = AllUGUITypes
                                      .Where(t => t == current || !usedTypes.Contains(t))
                                      .ToArray();
                int selectedIdx = Array.IndexOf(available, current);
                string[] displayNames = available.Select(t => t.ToString()).ToArray();

                EditorGUI.BeginChangeCheck();
                int newIdx = EditorGUILayout.Popup(selectedIdx, displayNames, GUILayout.Width(180));
                if (EditorGUI.EndChangeCheck() && newIdx != selectedIdx)
                {
                    usedTypes.Remove(current);
                    _settings.uiTypes[i] = available[newIdx];
                    usedTypes.Add(available[newIdx]);
                    GUI.changed = true;
                }

                _settings.typeSuffixes[i] = EditorGUILayout.TextField(_settings.typeSuffixes[i] ?? "");

                if (GUILayout.Button("-", GUILayout.Width(24)))
                    removeIndex = i;

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;

            if (removeIndex >= 0)
            {
                ArrayUtility.RemoveAt(ref _settings.uiTypes, removeIndex);
                ArrayUtility.RemoveAt(ref _settings.typeSuffixes, removeIndex);
                GUI.changed = true;
            }
        }

        private static void DrawDirectoryField(string label, ref string value, string folderPanelTitle)
        {
            EditorGUILayout.BeginHorizontal();
            value = EditorGUILayout.TextField(new GUIContent(label), value ?? "");
            if (GUILayout.Button("浏览", GUILayout.Width(50)))
            {
                string defaultDir = string.IsNullOrEmpty(value) ? Application.dataPath : value;
                string selected = EditorUtility.OpenFolderPanel(folderPanelTitle, defaultDir, "");
                if (!string.IsNullOrEmpty(selected))
                {
                    value = selected;
                    GUI.changed = true;
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}