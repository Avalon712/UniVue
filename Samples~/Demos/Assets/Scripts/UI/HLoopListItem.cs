using TMPro;
using UniVue.UI;
using UniVue.UI.Widgets;

public partial class HLoopListItem : LoopItem
{
    private ItemData _data;

    public void SetData(ItemData data)
    {
        ItemData old = _data;
        _data = data;
        if (old == null)
            Bind(data, Refresh, nameof(data.Index))();
        else
            Rebind(old, data);
    }

    private void Refresh()
    {
        if (_data == null) return;
        IndexTxt.text = _data.Index.ToString();
    }
}

#region UniVue Auto-Generated — DO NOT MODIFY

partial class HLoopListItem
{
    [LazyInitUI("/IndexTxt")]
    public TextMeshProUGUI IndexTxt { get; }
}

#endregion // UniVue Auto-Generated