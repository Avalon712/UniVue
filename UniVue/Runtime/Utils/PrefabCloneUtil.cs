using UnityEngine;

namespace UniVue.Utils
{
    public sealed class PrefabCloneUtil
    {
        private PrefabCloneUtil() { }

        public static GameObject RectTransformClone(GameObject prefab, Transform parent)
        {
            GameObject clone = GameObject.Instantiate(prefab,parent);

            RectTransform cloneRect = clone.GetComponent<RectTransform>();
            RectTransform prefabRect = prefab.GetComponent<RectTransform>();

            cloneRect.pivot = prefabRect.pivot;
            cloneRect.anchorMax = prefabRect.anchorMax;
            cloneRect.anchorMin = cloneRect.anchorMin;
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
