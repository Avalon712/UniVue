using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Utils;
using Object = UnityEngine.Object;

namespace UniVue.UI
{
    /// <summary>
    /// 将场景中的Canvas作为根节点，所有层级的渲染都使用此Canvas
    /// </summary>
    public sealed class DefaultLayerMgr : IUILayerMgr
    {
        private readonly List<Transform> _layers = new(8);

#if UNITY_EDITOR
        private DrivenRectTransformTracker _tracker;
#endif

        private DefaultLayerMgr()
        {
#if UNITY_EDITOR
            _tracker = new DrivenRectTransformTracker();
#endif

            Canvas canvas = Object.FindObjectOfType<Canvas>(true);
            if (!canvas)
                throw new NullReferenceException("Canvas not found！默认的UILayerMgr的实现中，场景里应该存在一个主Canvas作为LayerMgr的根节点");
            Root = canvas.gameObject;
            Root.SetActive(true);

            HideLayer = GameObjectUtils.CreateRectTransformGameObject("Hide Layer", canvas.transform);
            HideLayer.SetActive(false);
            HideLayer.hideFlags = HideFlags.HideInHierarchy;
        }

        public static DefaultLayerMgr Default { get; } = new();

        public GameObject Root { get; }

        public GameObject HideLayer { get; }

        public string GetLayerName(int layer)
        {
            return $"Layer{layer}";
        }

        public GameObject GetLayerRoot(int layer)
        {
            string layerName = GetLayerName(layer);
            foreach (Transform transform in _layers)
            {
                if (transform.name == layerName)
                    return transform.gameObject;
            }

            return CreateNewLayer(layerName);
        }


        private GameObject CreateNewLayer(string layerName)
        {
            _layers.Clear();
            Transform root = Root.transform;
            int layerCount = root.childCount;
            for (int i = 0; i < layerCount; i++)
            {
                Transform transform = root.GetChild(i);
                if (transform.gameObject == HideLayer) continue;
                _layers.Add(transform);
            }

            RectTransform canvas = Root.GetComponent<Canvas>().transform as RectTransform;
            GameObject newLayerObj = GameObjectUtils.CreateRectTransformGameObject(layerName, canvas);
            RectTransform newLayer = newLayerObj.transform as RectTransform;
            newLayer.sizeDelta = canvas.sizeDelta;

#if UNITY_EDITOR
            _tracker.Add(canvas, newLayer, DrivenTransformProperties.All);
#endif
            _layers.Add(newLayer);

            _layers.Sort((l1, l2) => string.Compare(l1.name, l2.name, StringComparison.Ordinal));

            for (int i = 0; i < _layers.Count; i++) _layers[i].SetSiblingIndex(i);

            HideLayer.transform.SetAsLastSibling();
            return newLayerObj;
        }
    }
}