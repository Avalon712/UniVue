using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Utils;

namespace UniVue.Editor
{
    public sealed class UGUIHeplerEditor : EditorWindow
    {
        public List<GameObject> _objects;
        private SerializedProperty _serializedObjs;
        private SerializedObject _window;
        private Vector2 _scrollPos = Vector2.zero;
        private TMP_FontAsset _replacedFont;

        [MenuItem("UniVue/UGUIHeplerEditor")]
        public static void OpenEditorWindow()
        {
            var window = GetWindow<UGUIHeplerEditor>("UGUI工具编辑器");
            window.position = new Rect(320, 240, 420, 400);
            window.Show();

            window._window = new SerializedObject(window);
            window._serializedObjs = window._window.FindProperty(nameof(_objects));
        }

        public void OnGUI()
        {
            _window.Update(); //Update必须在EditorGUILayout.Xxx之前

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("要操作的GameObjet");
            EditorGUILayout.PropertyField(_serializedObjs);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("注：所有取消Raycast Target属性的操作不会对可交互的组件生效");
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("UI组件的字体设置");
            _replacedFont = (TMP_FontAsset)EditorGUILayout.ObjectField(_replacedFont, typeof(TMP_FontAsset),false);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button("TMP_Text字体替换"))
            {
                if (_replacedFont != null && _objects != null)
                {
                    for (int i = 0; i < _objects.Count; i++)
                    {
                        List<TMP_Text> texts = ComponentFindUtil.GetAllComponents<TMP_Text>(_objects[i]);
                        for (int j = 0; j < texts.Count; j++)
                        {
                            texts[j].font = _replacedFont;
                        }
                    }
                }
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("TMP_InputField字体替换"))
            {
                if (_replacedFont != null && _objects != null)
                {
                    for (int i = 0; i < _objects.Count; i++)
                    {
                        List<TMP_InputField> inputs = ComponentFindUtil.GetAllComponents<TMP_InputField>(_objects[i]);
                        for (int j = 0; j < inputs.Count; j++)
                        {
                            inputs[j].fontAsset = _replacedFont;
                        }
                    }
                }
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("取消所有TMP_Text组件的Raycast Target属性"))
            {
                if (_objects != null)
                {
                    for (int i = 0; i < _objects.Count; i++)
                    {
                        List<TMP_Text> texts = ComponentFindUtil.GetAllComponents<TMP_Text>(_objects[i]);
                        for (int j = 0; j < texts.Count; j++)
                        {
                            if (!ContainsCtrlUI(texts[j]))
                            {
                                texts[j].raycastTarget = false;
                            }
                        }
                    }
                }
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("勾选所有Image组件的Raycast Target属性"))
            {
                if (_objects!=null)
                {
                    for (int i = 0; i < _objects.Count; i++)
                    {
                        List<Image> imgs = ComponentFindUtil.GetAllComponents<Image>(_objects[i]);
                        for (int j = 0; j < imgs.Count; j++)
                        {
                            if (!ContainsCtrlUI(imgs[j]))
                            {
                                imgs[j].raycastTarget = true;
                            }
                        }
                    }
                }
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("取消所有TMP_Text组件的Rich Text属性"))
            {
                if (_objects != null)
                {
                    for (int i = 0; i < _objects.Count; i++)
                    {
                        List<TMP_Text> texts = ComponentFindUtil.GetAllComponents<TMP_Text>(_objects[i]);
                        for (int j = 0; j < texts.Count; j++)
                        {
                            texts[j].richText = false;
                        }
                    }
                }
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("取消所有Slider组件的Interactable属性"))
            {
                if (_objects != null)
                {
                    for (int i = 0; i < _objects.Count; i++)
                    {
                        List<Slider> texts = ComponentFindUtil.GetAllComponents<Slider>(_objects[i]);
                        for (int j = 0; j < texts.Count; j++)
                        {
                            texts[j].interactable = false;
                        }
                    }
                }
            }
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("注：此操作会对所有图片进行一次拷贝再打成图集");
            if (GUILayout.Button("将每个GameObject下引用的所有图片打成图集，然后引用图集中的图片"))
            {
                if (_objects != null)
                {
                    for (int i = 0; i < _objects.Count; i++)
                    {
                       //TODO
                    }
                }
            }
            EditorGUILayout.Space();

            EditorGUILayout.EndScrollView();

            _window.ApplyModifiedProperties();
        }

        private bool ContainsCtrlUI(TMP_Text text)
        {
            if (text.GetComponent<Button>() != null) { return true; }
            if(text.GetComponent<Toggle>() != null) { return true; }
            if (text.GetComponent<Slider>() != null) { return true; }
            if (text.GetComponent<TMP_InputField>() != null) { return true; }
            return false;
        }

        private bool ContainsCtrlUI(Image img)
        {
            if (img.GetComponent<Button>() != null) { return true; }
            if (img.GetComponent<Toggle>() != null) { return true; }
            if (img.GetComponent<Slider>() != null) { return true; }
            if (img.GetComponent<TMP_InputField>() != null) { return true; }
            return false;
        }
    }
}
