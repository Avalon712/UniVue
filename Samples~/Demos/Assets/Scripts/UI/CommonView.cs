using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.UI;
using UniVue.UI.Widgets;

public sealed partial class CommonView : BaseView
{
    private readonly List<ItemData> _items = new(1000);
    public override int Layer { get; } = 1;

    protected override void OnInit()
    {
        enableUpdatePerSecond = true;

        for (int i = 0; i < 1000; i++)
            _items.Add(new ItemData { Label = $"Label {i}", IsSelected = i % 2 == 0, Index = i });

        OpComponent.BindOp(new OpComponent.OpCode { Name = "Invoke Method" }, code =>
        {
            if (code.Code == "VL")
            {
                //这里Add之后在OnOpen中去获取可能会获得为null，因为这里具有延时，调用不一定同步
                AddComponent<VLoopListComponent>(Container, (success, component) =>
                {
                    if (!success) return;
                    component.SetData(_items);
                });
            }

            if (code.Code == "HL")
            {
                AddComponent<LoopList>("HLoopListComponent", Container, (success, component) =>
                {
                    if (!success) return;
                    component.BindItemRender<HLoopListItem>((index, item) =>
                    {
                        item.SetData(_items[index]);
                    });
                    component.Count = _items.Count;
                });
            }

            if (code.Code == "VG")
            {
                AddComponent<LoopGrid>("VLoopGridComponent", Container, (success, component) =>
                {
                    if (!success) return;
                    component.BindItemRender<LoopGridItem>((index, item) =>
                    {
                        item.SetData(_items[index]);
                    });
                    component.Count = _items.Count;
                });
            }

            if (code.Code == "HG")
            {
                AddComponent<LoopGrid>("HLoopGridComponent", Container, (success, component) =>
                {
                    if (!success) return;
                    component.BindItemRender<LoopGridItem>((index, item) =>
                    {
                        item.SetData(_items[index]);
                    });
                    component.Count = _items.Count;
                });
            }
        });
    }

    protected override void OnUpdatePerSecond()
    {
        for (int i = 0; i < _items.Count; i++)
        {
            ItemData item = _items[i];
            item.Label = $"Label {i} {DateTime.Now.Second}";
        }
    }

    protected override void OnOpen()
    {
        if (TryGetViewComponent(out VLoopListComponent component))
        {
            component.SetData(_items);
            component.Show();
        }

        if (TryGetViewComponent(out LoopList loopList))
        {
            loopList.Count = _items.Count;
            loopList.Show();
        }

        foreach (LoopGrid grid in GetViewComponents<LoopGrid>())
        {
            grid.Count = _items.Count;
            grid.Show();
        }

        base.OnOpen();
        RefreshUI(true);
    }
}

#region UniVue Auto-Generated — DO NOT MODIFY

partial class CommonView
{
    [LazyInitUI("/#Container")]
    public RectTransform Container { get; }

    [LazyInitUI("/CloseBtnUI")]
    public CloseBtnUI CloseBtnUI { get; }

    [LazyInitUI("/OpComponent")]
    public OpComponent OpComponent { get; }
}

#endregion // UniVue Auto-Generated