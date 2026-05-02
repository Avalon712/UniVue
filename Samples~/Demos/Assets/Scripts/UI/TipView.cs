using TMPro;
using UniVue.UI;

public sealed partial class TipView : BaseView
{
    public override int Layer { get; } = 5;

    protected override void OnInit() { }
}

#region UniVue Auto-Generated — DO NOT MODIFY

partial class TipView
{
    [LazyInitUI("/CloseBtn")]
    public CloseBtnUI CloseBtn { get; }

    [LazyInitUI("/MessageTxt")]
    public TextMeshProUGUI MessageTxt { get; }

    [LazyInitUI("/TitleTxt")]
    public TextMeshProUGUI TitleTxt { get; }
}

#endregion // UniVue Auto-Generated