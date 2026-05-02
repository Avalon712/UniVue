using UniVue.UI;

public sealed partial class GMView : BaseView
{
    public override int Layer { get; } = 9;

    protected override void OnInit()
    {
        CloseViewOp.BindOp(new OpComponent.OpCode { Name = "Close View" }, op => UIMgr.Close(op.Code));
        OpenViewOp.BindOp(new OpComponent.OpCode { Name = "Open View" }, op => UIMgr.Open(op.Code));
    }
}

#region UniVue Auto-Generated — DO NOT MODIFY

partial class GMView
{
    [LazyInitUI("/CloseViewOp")]
    public OpComponent CloseViewOp { get; }

    [LazyInitUI("/OpenViewOp")]
    public OpComponent OpenViewOp { get; }
}

#endregion // UniVue Auto-Generated