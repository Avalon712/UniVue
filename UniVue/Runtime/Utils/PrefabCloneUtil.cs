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

            clone.transform.position = prefab.transform.position;
            clone.transform.rotation = prefab.transform.rotation;
            cloneRect.anchoredPosition = prefabRect.anchoredPosition;
            cloneRect.anchoredPosition3D = prefabRect.anchoredPosition3D;
            cloneRect.localPosition = prefabRect.localPosition;
            clone.transform.localScale = prefab.transform.localScale;
            clone.transform.localPosition = prefab.transform.localPosition;
            clone.name = prefab.name;

            return clone;
        }

        public static GameObject Clone(GameObject prefab,GameObject parent)
        {
            GameObject clone = GameObject.Instantiate(prefab, parent?.transform);

            Transform prefabTrans = prefab.transform;
            Transform cloneTrans = clone.transform;

            if(parent != null) { cloneTrans.SetParent(parent.transform); }

            clone.name = prefab.name;
            cloneTrans.localScale = prefabTrans.localScale;
            cloneTrans.localPosition = prefabTrans.localPosition;
            cloneTrans.localRotation = prefabTrans.localRotation;
            cloneTrans.position = prefabTrans.position;
            cloneTrans.rotation = prefabTrans.rotation;

            return clone;
        }
    }
}
