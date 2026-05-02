using System;
using TMPro;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.UI;

public sealed partial class OpComponent : BaseComponent
{
    public void BindOp(OpCode opcode, Action<OpCode> codeExecutor)
    {
        Bind(opcode, () => { TitleTxt.text = opcode.Name; })();
        ExeBtn.onClick.AddListener(() => codeExecutor.Invoke(opcode));
        CodeInput.onEndEdit.AddListener(code => opcode.Code = code);
    }

    protected override void OnShow()
    {
        RefreshUI(true);
    }

    public sealed class OpCode : BaseModel
    {
        public string Name { get; set; }

        public string Code { get; set; }
    }
}

#region UniVue Auto-Generated — DO NOT MODIFY

partial class OpComponent
{
    [LazyInitUI("/CodeInput")]
    public TMP_InputField CodeInput { get; }

    [LazyInitUI("/CodeInput/Text Area/Text")]
    public TextMeshProUGUI Text { get; }

    [LazyInitUI("/ExeBtn")]
    public Button ExeBtn { get; }

    [LazyInitUI("/TitleTxt")]
    public TextMeshProUGUI TitleTxt { get; }
}

#endregion // UniVue Auto-Generated