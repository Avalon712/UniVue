using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.UI;
using UniVue.Utils;

/// <summary>
/// 每个层级都使用独立的Canvas进行渲染
/// </summary>
public sealed class MyCustomLayerMgr : IUILayerMgr
{
    private readonly Canvas _canvasPrefab;
    private readonly List<Transform> _layers = new(8);

    public MyCustomLayerMgr(Canvas canvasPrefab)
    {
        _canvasPrefab = canvasPrefab;
        Root = GameObjectUtils.CreateGameObject("UI Canvas");
        HideLayer = GameObjectUtils.CreateGameObject("HideLayer", Root.transform);
        HideLayer.SetActive(false);
        HideLayer.hideFlags = HideFlags.HideInHierarchy;
    }

    public GameObject Root { get; }
    public GameObject HideLayer { get; }

    public string GetLayerName(int layer)
    {
        return $"Canvas {layer}";
    }


    public GameObject GetLayerRoot(int layer)
    {
        string layerName = GetLayerName(layer);
        foreach (Transform transform in _layers)
        {
            if (transform.name == layerName)
                return transform.gameObject;
        }

        return CreateNewLayer(layer, layerName);
    }

    private GameObject CreateNewLayer(int layer, string layerName)
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

        GameObject newLayerObj = GameObjectUtils.RectTransformClone(_canvasPrefab.gameObject, Root.transform);
        newLayerObj.GetComponent<Canvas>().sortingOrder = layer;
        newLayerObj.name = layerName;
        RectTransform newLayer = newLayerObj.transform as RectTransform;
        _layers.Add(newLayer);

        _layers.Sort((l1, l2) => string.Compare(l1.name, l2.name, StringComparison.Ordinal));

        for (int i = 0; i < _layers.Count; i++) _layers[i].SetSiblingIndex(i);

        HideLayer.transform.SetAsLastSibling();
        return newLayerObj;
    }
}