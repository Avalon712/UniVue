using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UniVue.Utils
{
    public static class GameObjectUtils
    {
        public static void SetActive(GameObject obj, bool active)
        {
            if (obj.activeSelf != active) obj.SetActive(active);
        }

        /// <summary>
        /// 从自身开始查找一个指定名称的GameObject(深度优先)
        /// </summary>
        public static GameObject DepthFind(string name, GameObject self)
        {
            if (string.IsNullOrEmpty(name)) return null;
            return DepthFind(go => go.name == name, self);
        }

        /// <summary>
        /// 从自身开始查找一个符合匹配条件的GameObject(深度优先)
        /// </summary>
        public static GameObject DepthFind(Func<GameObject, bool> matchCondition, GameObject self)
        {
            if (matchCondition == null || !self) return null;

            if (matchCondition.Invoke(self)) return self;

            int childNum = self.transform.childCount;
            for (int i = 0; i < childNum; i++)
            {
                GameObject curr = self.transform.GetChild(i).gameObject;
                if (matchCondition.Invoke(curr)) return curr;

                GameObject obj = DepthFind(matchCondition, curr);
                if (obj != null) return obj;
            }

            return null;
        }

        /// <summary>
        /// 从自身开始向上查找符合查找条件的GameObject
        /// </summary>
        /// <param name="matchCondition">匹配条件</param>
        /// <param name="self"></param>
        /// <returns></returns>
        public static Transform UpFind(Func<Transform, bool> matchCondition, Transform self)
        {
            if (matchCondition == null || !self) return null;
            while (self)
            {
                bool matchResult = matchCondition.Invoke(self);
                Transform target = matchResult ? self : self.parent;
                self = target;
                if (matchResult) return target;
            }

            return self;
        }

        public static GameObject FindByPath(string path, GameObject self)
        {
            if (string.IsNullOrEmpty(path) || !self) return null;

            static ReadOnlySpan<char> ConsumePathSegment(in ReadOnlySpan<char> path, ref int index)
            {
                int len = path.Length;
                while (index < len && path[index] == '/') index++;
                if (index >= len) return ReadOnlySpan<char>.Empty;
                int start = index;
                while (index < len && path[index] != '/') index++;
                return path.Slice(start, index - start);
            }

            static bool NameEqualsOrdinal(in ReadOnlySpan<char> span, string name)
            {
                if (name == null) return span.IsEmpty;
                if (span.Length != name.Length) return false;
                for (int i = 0; i < span.Length; i++)
                {
                    if (span[i] != name[i])
                        return false;
                }

                return true;
            }

            static Transform FindDirectChildByName(Transform parent, in ReadOnlySpan<char> nameSpan)
            {
                int count = parent.childCount;
                for (int i = 0; i < count; i++)
                {
                    Transform child = parent.GetChild(i);
                    if (NameEqualsOrdinal(nameSpan, child.name))
                        return child;
                }

                return null;
            }

            Transform root = self.transform;
            ReadOnlySpan<char> full = path.AsSpan();
            int index = 0;
            ReadOnlySpan<char> first = ConsumePathSegment(full, ref index);
            if (first.IsEmpty) return null;

            if (index >= full.Length)
                return root.gameObject;

            Transform current = root;
            while (index < full.Length)
            {
                ReadOnlySpan<char> segment = ConsumePathSegment(full, ref index);
                if (segment.IsEmpty)
                    return null;

                current = FindDirectChildByName(current, segment);
                if (current == null) return null;
            }

            return current.gameObject;
        }

        public static void KeepTheSameWithPrefab(RectTransform prefab, RectTransform clone)
        {
            if (!prefab || !clone) return;
            clone.pivot = prefab.pivot;
            clone.anchorMax = prefab.anchorMax;
            clone.anchorMin = prefab.anchorMin;
            clone.anchoredPosition = prefab.anchoredPosition;
            clone.anchoredPosition3D = prefab.anchoredPosition3D;
            clone.offsetMax = prefab.offsetMax;
            clone.offsetMin = prefab.offsetMin;
            clone.sizeDelta = prefab.sizeDelta;
            clone.localScale = prefab.localScale;
            clone.name = prefab.name;
        }

        public static GameObject RectTransformClone(GameObject prefab, Transform parent)
        {
            GameObject clone = Object.Instantiate(prefab, parent);
            KeepTheSameWithPrefab(prefab.GetComponent<RectTransform>(), clone.GetComponent<RectTransform>());
            return clone;
        }

        public static GameObject CreateGameObject(string name, Transform parent = null)
        {
            GameObject go = new(name);
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            return go;
        }

        public static GameObject CreateRectTransformGameObject(string name, Transform parent = null)
        {
            GameObject go = new(name, typeof(RectTransform));
            if (!go.TryGetComponent(out RectTransform rt))
            {
                rt = go.AddComponent<RectTransform>();
            }

            rt.SetParent(parent);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.position = Vector3.zero;
            rt.rotation = Quaternion.identity;
            rt.localScale = Vector3.one;
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            return go;
        }
    }
}