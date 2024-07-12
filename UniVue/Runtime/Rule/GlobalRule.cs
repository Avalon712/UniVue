using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Utils;

namespace UniVue.Rule
{
    internal static class GlobalRule
    {
        /// <summary>
        /// 获取一个GameObject下所有的具有特殊命名的UI组件
        /// </summary>
        /// <param name="gameObject">要过滤的根对象</param>
        /// <returns>IEnumerable<ValueTuple<Component, UIType>></returns>
        public static IEnumerable<ValueTuple<Component, UIType>> Filter(GameObject gameObject)
        {
            char SKIP_ALL_DESCENDANT_SYMBOL = Vue.Config.SkipDescendantNodeSeparator;
            char SKIP_CURRENT_SYMBOL = Vue.Config.SkipCurrentNodeSeparator;

            Queue<Transform> queue = new Queue<Transform>();
            Transform root = gameObject.transform;
            ValueTuple<Component, UIType> result = new(null, UIType.None);
            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                Transform parent = queue.Dequeue();

                for (int i = 0; i < parent.childCount; i++)
                {
                    Transform child = parent.GetChild(i);

                    if (child.name.StartsWith(SKIP_ALL_DESCENDANT_SYMBOL) || child.name.EndsWith("View")) { continue; }

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
                        result.Item1 = gameObject.GetComponent<Image>();
                        result.Item2 = UIType.Image;
                        break;
                    }
                case UIType.TMP_Dropdown:
                    {
                        result.Item1 = gameObject.GetComponent<TMP_Dropdown>();
                        result.Item2 = UIType.TMP_Dropdown;
                        break;
                    }
                case UIType.TMP_Text:
                    {
                        result.Item1 = gameObject.GetComponent<TMP_Text>();
                        result.Item2 = UIType.TMP_Text;
                        break;
                    }
                case UIType.TMP_InputField:
                    {
                        result.Item1 = gameObject.GetComponent<TMP_InputField>();
                        result.Item2 = UIType.TMP_InputField;
                        break;
                    }
                case UIType.Button:
                    {
                        result.Item1 = gameObject.GetComponent<Button>();
                        result.Item2 = UIType.Button;
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
                        result.Item1 = gameObject.GetComponent<Slider>();
                        result.Item2 = UIType.Slider;
                        break;
                    }
            }

        }
    }
}
