using System.Collections.Generic;
using UnityEngine;

namespace UniVue.Common
{
    public static class GameObjectUtil
    {
        public static void SetActive(GameObject obj, bool active)
        {
            if (obj.activeSelf != active)
            {
                obj.SetActive(active);
            }
        }

        /// <summary>
        /// 从自身开始查找一个指定名称的GameObject(深度优先)
        /// </summary>
        public static GameObject DepthFind(string name, GameObject self)
        {
            if (string.IsNullOrEmpty(name)) { return null; }

            if (self.name.Equals(name)) { return self; }

            int childNum = self.transform.childCount;
            for (int i = 0; i < childNum; i++)
            {
                GameObject curr = self.transform.GetChild(i).gameObject;
                if (curr.name.Equals(name))
                {
                    return curr;
                }
                else
                {
                    GameObject obj = DepthFind(name, curr);
                    if (obj != null) { return obj; }
                }
            }
            return null;
        }

        /// <summary>
        /// 找obj以及它所有的后代中所有的视图对象ViewObject
        /// </summary>
        /// <param name="obj">起点GameObject</param>
        /// <param name="viewObjects">所有的视图对象ViewObject</param>
        public static void DepthFindAllViewObjects(GameObject obj, Dictionary<string, GameObject> viewObjects)
        {
            //处理根对象
            if (Vue.IsViewObject(obj) && !viewObjects.ContainsKey(obj.name))
            {
                viewObjects.Add(obj.name, obj);
                //ThrowUtil.ThrowLog("已加载视图对象 - " + obj.name);
            }

            Transform parent = obj.transform;
            int childNum = parent.childCount;
            for (int i = 0; i < childNum; i++)
            {
                GameObject curr = parent.GetChild(i).gameObject;
                if (Vue.IsViewObject(curr))
                {
                    if (viewObjects.ContainsKey(curr.name))
                    {
                        ThrowUtil.ThrowException($"重复的视图名称{curr.name}，视图名称不允许重复");
                    }
                    viewObjects.Add(curr.name, curr);
                    //ThrowUtil.ThrowLog("已加载视图对象 - " + curr.name);
                }
                //如果还有后代则继续查找
                if (curr.transform.childCount > 0)
                    DepthFindAllViewObjects(curr, viewObjects);
            }
        }

        /// <summary>
        /// 找到指定viewObject的父视图对象
        /// </summary>
        /// <param name="viewObject"></param>
        /// <returns>父视图的视图名称，如果不存在则返回string.Empty</returns>
        public static string FindParentViewObject(GameObject viewObject)
        {
            if (viewObject == null) return string.Empty;
            GameObject parent = viewObject.transform.parent?.gameObject;
            if (Vue.IsViewObject(parent))
            {
                return parent.name;
            }
            return FindParentViewObject(parent);
        }

        public static GameObject RectTransformClone(GameObject prefab, Transform parent)
        {
            GameObject clone = GameObject.Instantiate(prefab, parent);

            RectTransform cloneRect = clone.GetComponent<RectTransform>();
            RectTransform prefabRect = prefab.GetComponent<RectTransform>();

            cloneRect.pivot = prefabRect.pivot;
            cloneRect.anchorMax = prefabRect.anchorMax;
            cloneRect.anchorMin = prefabRect.anchorMin;
            cloneRect.anchoredPosition = prefabRect.anchoredPosition;
            cloneRect.anchoredPosition3D = prefabRect.anchoredPosition3D;
            cloneRect.offsetMax = prefabRect.offsetMax;
            cloneRect.offsetMin = prefabRect.offsetMin;
            cloneRect.sizeDelta = prefabRect.sizeDelta;
            cloneRect.localScale = prefabRect.localScale;
            clone.name = prefab.name;

            return clone;
        }
    }
}
