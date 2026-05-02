using System;
using UnityEngine;
using UniVue.UI;

public sealed class MyUIPrefabLoader : IUIPrefabLoader
{
    public void LoadUIPrefabAsync(Type uiType, Action<GameObject> callback)
    {
        LoadUIPrefabAsync(uiType.Name, callback);
    }

    public void LoadUIPrefabAsync(string uiName, Action<GameObject> callback)
    {
        ResourceRequest resourceRequest = Resources.LoadAsync<GameObject>(uiName);
        resourceRequest.completed += op =>
        {
            if (op.isDone) callback.Invoke((GameObject)resourceRequest.asset);
        };
    }
}