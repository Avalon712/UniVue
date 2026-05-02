using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Editor;
using UniVue.UI;

public sealed class MyCustomCodeGenRule : UICodeGenRule
{
    public override int Order { get; }

    protected override bool Filter(GameObject prefab, BaseUI baseUI)
    {
        return true;
    }

    protected override bool TryGenProperties(Type clazz, GameObject go, HashSet<GeneratedProperty> properties)
    {
        Traverse(go, (path, visitedGo) =>
        {
            if (visitedGo.TryGetComponent(out BaseUI _)) return false;
            string name = visitedGo.name;
            if (name.StartsWith("#"))
                properties.Add(new GeneratedProperty(typeof(RectTransform).FullName, name.Replace("#", ""), path));
            return true;
        });
        return true;
    }
}