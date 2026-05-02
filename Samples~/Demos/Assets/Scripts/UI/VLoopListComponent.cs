using System.Collections.Generic;
using UniVue.Model;
using UniVue.UI;
using UniVue.UI.Widgets;

public sealed partial class VLoopListComponent : BaseComponent
{
    private List<ItemData> _items;

    protected override void OnCreate()
    {
        VLoopList.BindItemRender<VLoopListItem>(OnItemRender);
    }

    private void OnItemRender(int index, VLoopListItem item)
    {
        item.SetData(_items[index]);
    }

    public void SetData(List<ItemData> data)
    {
        _items = data;
        VLoopList.Count = data.Count;
    }
}

public sealed class ItemData : BaseModel
{
    public string Label { get; set; }

    public bool IsSelected { get; set; }

    public int Index { get; set; }
}

#region UniVue Auto-Generated — DO NOT MODIFY

partial class VLoopListComponent
{
    [LazyInitUI("/VLoopList")]
    public LoopList VLoopList { get; }
}

#endregion // UniVue Auto-Generated