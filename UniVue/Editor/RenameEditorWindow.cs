using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UniVue.Rule;
using UniVue.Utils;

namespace UniVue.Editor
{
    internal sealed class RenameEditorWindow : EditorWindow
    {
        public  List<GameObject> _viewObjects;
        private SerializedProperty _serializedObjs;
        private SerializedObject _window;
        private NamingFormat _format = NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix;

        private Vector2 _scrollPos = Vector2.zero;

        [MenuItem("UniVue/RenameEditor")]
        public static void OpenEditorWindow()
        {
            var window = GetWindow<RenameEditorWindow>("重命名编辑器");
            window.position = new Rect(320, 240, 370, 310);
            window.Show();

            window._window = new SerializedObject(window);
            window._serializedObjs = window._window.FindProperty("_viewObjects");
        }


        private void OnGUI()
        {
            _window.Update(); //Update必须在EditorGUILayout.Xxx之前

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            EditorGUILayout.LabelField("编辑器功能说明");
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextArea(
                "此编辑器功能将会对所有没有特殊作用的UI组件进行重命名，即在\n" +
                "名称前添加特殊字符以此来大幅减少对UI组件的查找次数。命名有\n" +
                "一定的规则，请勿随意命名导致无法正确处理UI组件行为。名称前\n" +
                "有字符'~'的GameObject及其后代都不会被进行组件查找；名称前\n" +
                "有字符'@'的GameObject不会被进行组件查找，但其后代会。", GUILayout.Height(80));
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("视图对象ViewObject");
            EditorGUILayout.PropertyField(_serializedObjs);
            EditorGUILayout.Space();


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("命名格式");
            _format = (NamingFormat)EditorGUILayout.EnumFlagsField(_format);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUILayout.Button("重命名")){
                Rename(_format); 
            }
            EditorGUILayout.Space();

            if (GUILayout.Button("清空特殊命名")) 
            {
                for (int i = 0; i < _viewObjects.Count; i++)
                {
                    Clear(_viewObjects[i]);
                }
            }

            EditorGUILayout.EndScrollView();

            _window.ApplyModifiedProperties();
        }


        private void Clear(GameObject root)
        {
            if (root.name.StartsWith('~')) { root.name = root.name.Substring(1); }
            if (root.name.StartsWith('@')) { root.name = root.name.Substring(1); }

            Transform transform = root.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                Clear(transform.GetChild(i).gameObject);
            }
        }

        private void Rename(NamingFormat format)
        {
            for (int i = 0; i < _viewObjects.Count; i++)
            {
                List<CustomTuple<GameObject, bool>> result = new List<CustomTuple<GameObject, bool>>();

                DeapthSearch(_viewObjects[i], result, format);

                //查看是否正确匹配
                //for (int i = 0; i < result.Count; i++)
                //{
                //    Debug.Log("索引"+i+": "+result[i].Item1.name + " ======>  " + result[i].Item2);
                //}

                for (int j = 0; j < result.Count; j++)
                {
                    CustomTuple<GameObject, bool> tuple = result[j];

                    //当前GameObject能够标记~的条件是:当前GameObject匹配不成功同时其后代（如果有）都匹配不成功
                    int lastDescendantIdx = j; //从i+1到lastDescendantIdx都是tuple的后代元素
                    bool allNoMatch = AllDescendantNoMatch(j, tuple, result, ref lastDescendantIdx);
                    if (!tuple.Item2 && j > 0 && allNoMatch)
                    {
                        tuple.Item1.name = '~' + tuple.Item1.name;
                        j = lastDescendantIdx;
                    }

                    //当前GameObject能被标记@的条件是：当前GameoObject匹配不成功但是其后代有匹配成功的
                    //如果当前是视图对象（以View结尾的GameObject的名称也不要特殊命名）
                    if (!tuple.Item2 && !allNoMatch && !tuple.Item1.name.EndsWith("View")) { tuple.Item1.name = '@' + tuple.Item1.name; }
                }
            }
            
            Debug.Log("UI组件重命名完成!");
        }

        /// <summary>
        /// 子代是否都匹配不成功
        /// </summary>
        private bool AllDescendantNoMatch(int ancestorIdx,CustomTuple<GameObject, bool> ancestor,List<CustomTuple<GameObject, bool>> result,ref int lastDescendantIdx)
        {
            Transform transform = ancestor.Item1.transform;

            if (transform.childCount == 0) { return true; }

            //查看当前元素的所有子孩子是否都不匹配
            for (int j = ancestorIdx + 1; j < result.Count; j++)
            {
                if (IsAncestor(transform, result[j].Item1.transform))
                {
                    if (result[j].Item2) { return false; }
                }
                else
                {
                    lastDescendantIdx = j - 1;
                    break;
                }
            }

            return true;
        }

        private bool IsAncestor(Transform ancestor,Transform descendant)
        {
            while (descendant != null)
            {
                if (descendant.parent == ancestor) { return true; }
                else { descendant = descendant.parent; }
            }
            return false;
        }

        //bool指示当前GameObject名称是否匹配成功
        private void DeapthSearch(GameObject root,List<CustomTuple<GameObject,bool>> result,NamingFormat format)
        {
            result.Add(new CustomTuple<GameObject, bool>(root, NamingRuleEngine.FullFuzzyMatch(format, root.name)));

            Transform transform = root.transform;
            for (int i = 0; i < transform.childCount; i++)
            {
                DeapthSearch(transform.GetChild(i).gameObject, result, format);
            }
        }


    }
}
