using TMPro;
using UnityEngine.UI;
using UniVue.UI;
using UniVue.UI.Widgets;

public sealed partial class VLoopListItem : LoopItem
{
    private ItemData _data;

    protected override void OnCreate()
    {
        SelectedToggle.onValueChanged.AddListener(isOn =>
        {
            if (_data != null && _data.IsSelected != isOn)
                _data.IsSelected = isOn;
        });
    }

    public void SetData(ItemData data)
    {
        ItemData old = _data;
        _data = data;
        if (old != null)
            Rebind(old, data);
        else
            Bind(data, Refresh)(); //绑定时立即渲染一次
    }

    private void Refresh()
    {
        if (_data == null) return;
        LabelTxt.text = _data.Label;
        SelectedToggle.isOn = _data.IsSelected;
    }
}

#region UniVue Auto-Generated — DO NOT MODIFY

partial class VLoopListItem
{
    [LazyInitUI("/BgImg")]
    public Image BgImg { get; }

    [LazyInitUI("/SelectedToggle")]
    public Toggle SelectedToggle { get; }

    [LazyInitUI("/SelectedToggle/LabelTxt")]
    public TextMeshProUGUI LabelTxt { get; }
}

#endregion // UniVue Auto-Generated