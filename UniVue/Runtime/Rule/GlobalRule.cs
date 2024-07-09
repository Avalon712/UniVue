using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Utils;
using UniVue.View.Views;

namespace UniVue.Rule
{
    public static class GlobalRule
    {
        /// <summary>
        /// 获取一个GameObject下所有的具有特殊命名的UI组件
        /// </summary>
        /// <param name="gameObject">要找到组件的根对象</param>
        /// <param name="view">指定当前的GameObject是否是ViewObject，如果是则需要传递此参数</param>
        /// <param name="exclude">要排除的后代（这些GameObject的后代都不会进行查找）</param>
        /// <returns>IEnumerable<ValueTuple<Component, UIType>></returns>
        public static IEnumerable<ValueTuple<Component, UIType>> Filter(GameObject gameObject, IView view = null, params GameObject[] exclude)
        {
            char SKIP_ALL_DESCENDANT_SYMBOL = Vue.Config.SkipDescendantNodeSeparator;
            char SKIP_CURRENT_SYMBOL = Vue.Config.SkipCurrentNodeSeparator;

            Queue<Transform> queue = new Queue<Transform>();
            Transform root = gameObject.transform;
            ValueTuple<Component, UIType> result = new(null, UIType.None);
            queue.Enqueue(root);

            //获取所有要跳过查找的GameObject节点
            List<Transform> skipObjs = null;
            if (view != null && !string.IsNullOrEmpty(view.Root))
            {
                skipObjs = new List<Transform>();
                using (var v = view.GetNestedViews().GetEnumerator())
                {
                    while (v.MoveNext() && v.Current != null)
                    {
                        skipObjs.Add(v.Current.ViewObject.transform);
                    }
                }
            }

            if (exclude != null)
            {
                if (skipObjs == null)
                    skipObjs = new List<Transform>(exclude.Length);
                for (int i = 0; i < exclude.Length; i++)
                {
                    skipObjs.Add(exclude[i].transform);
                }
            }

            while (queue.Count > 0)
            {
                Transform parent = queue.Dequeue();

                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);

                    if (child.name.StartsWith(SKIP_ALL_DESCENDANT_SYMBOL)) { continue; }

                    if (skipObjs != null && skipObjs.Contains(child)) { continue; }

                    //非叶子节点再入队
                    if (child.childCount != 0)
                    {
                        queue.Enqueue(child);
                    }

                    if (child.name.StartsWith(SKIP_CURRENT_SYMBOL)) { continue; }

                    SetResult(ref result, child.gameObject);

                    if (result.Item1 != null)
                    {
                        yield return result;
                    }

                    result.Item1 = null;
                    result.Item2 = UIType.None;
                }
            }
        }

        private static void SetResult(ref ValueTuple<Component, UIType> result, GameObject gameObject)
        {
            //减少GetComponent()函数的调用
            switch (UITypeUtil.GetUIType(gameObject.name))
            {
                case UIType.Image:
                    {
                        Image image = gameObject.GetComponent<Image>();
                        if (image != null)
                        {
                            result.Item1 = image;
                            result.Item2 = UIType.Image;
                        }
                        break;
                    }
                case UIType.TMP_Dropdown:
                    {
                        TMP_Dropdown dropdown = gameObject.GetComponent<TMP_Dropdown>();
                        if (dropdown != null)
                        {
                            result.Item1 = dropdown;
                            result.Item2 = UIType.TMP_Dropdown;
                        }
                        break;
                    }
                case UIType.TMP_Text:
                    {
                        TMP_Text text = gameObject.GetComponent<TMP_Text>();
                        if (text != null)
                        {
                            result.Item1 = text;
                            result.Item2 = UIType.TMP_Text;
                        }
                        break;
                    }
                case UIType.TMP_InputField:
                    {
                        TMP_InputField input = gameObject.GetComponent<TMP_InputField>();
                        if (input != null)
                        {
                            result.Item1 = input;
                            result.Item2 = UIType.TMP_InputField;
                        }
                        break;
                    }
                case UIType.Button:
                    {
                        Button button = gameObject.GetComponent<Button>();
                        if (button != null)
                        {
                            result.Item1 = button;
                            result.Item2 = UIType.Button;
                        }
                        break;
                    }
                case UIType.Toggle:
                    {
                        Toggle toggle = gameObject.GetComponent<Toggle>();
                        if (toggle != null)
                        {
                            result.Item1 = toggle;
                            result.Item2 = toggle.group == null ? UIType.Toggle : UIType.ToggleGroup;
                        }
                        break;
                    }
                case UIType.Slider:
                    {
                        Slider slider = gameObject.GetComponent<Slider>();
                        if (slider != null)
                        {
                            result.Item1 = slider;
                            result.Item2 = UIType.Slider;
                        }
                        break;
                    }
            }

        }
    }
}
