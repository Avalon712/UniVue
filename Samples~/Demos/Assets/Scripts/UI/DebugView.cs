using TMPro;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.UI;

public partial class DebugView : BaseView
{
    private Data _data;
    public override int Layer { get; } = 7;

    protected override void OnInit()
    {
        enableUpdate = true;
        enableUpdatePerSecond = true;

        _data = new Data();

        MoneySlider.maxValue = 1000;
        MoneySlider.minValue = 0;

        MoneySlider.onValueChanged.AddListener(v => _data.Money = v);
        NameInputOp.BindOp(new OpComponent.OpCode { Code = _data.Name, Name = "Name Input" },
                           op => _data.Name = op.Code);
        LanguageDropdown.onValueChanged.AddListener(index => _data.Language = LanguageDropdown.options[index].text);
        GenderToggle.onValueChanged.AddListener(v => _data.Gender = v);

        Bind(_data, () => NameTxt.text = $"Name: {_data.Name}", nameof(_data.Name));
        Bind(_data, () => AgeTxt.text = $"Age: {_data.Age}", nameof(_data.Age));
        Bind(_data, () => GenderTxt.text = $"Gender: {(_data.Gender ? "boy" : "girl")}", nameof(_data.Gender));
        Bind(_data, () => LangTxt.text = $"Language: {_data.Language}", nameof(_data.Language));
        Bind(_data, () =>
        {
            MoneyTxt.text = $"Money: ${_data.Money:F2}";
            MoneySlider.value = _data.Money;
        });
        Bind(_data, () => DataTxt.text = _data.ToString());
    }

    protected override void OnOpen()
    {
        RefreshUI(true);
    }

    protected override void OnUpdate(in float deltaTime)
    {
        ProfierTxt.text = $"Rendering Count: {UIMgr.Renderer.WaitExecuteRenderingCount}";
    }

    protected override void OnUpdatePerSecond()
    {
        _data.Age++;
    }

    private sealed class Data : BaseModel
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public bool Gender { get; set; }

        public string Language { get; set; }

        public float Money { get; set; }

        public override string ToString()
        {
            return
                $"{nameof(Name)} = {Name},\n {nameof(Age)} = {Age},\n {nameof(Gender)} = {(Gender ? "boy" : "girl")},\n{nameof(Language)} = {Language},\n {nameof(Money)} = {Money:F2}";
        }
    }
}

#region UniVue Auto-Generated — DO NOT MODIFY

partial class DebugView
{
    [LazyInitUI("/AgeTxt")]
    public TextMeshProUGUI AgeTxt { get; }

    [LazyInitUI("/CloseBtnUI")]
    public CloseBtnUI CloseBtnUI { get; }

    [LazyInitUI("/GenderToggle")]
    public Toggle GenderToggle { get; }

    [LazyInitUI("/GenderToggle/GenderTxt")]
    public TextMeshProUGUI GenderTxt { get; }

    [LazyInitUI("/Image")]
    public Image Image { get; }

    [LazyInitUI("/Image (1)/ProfierTxt")]
    public TextMeshProUGUI ProfierTxt { get; }

    [LazyInitUI("/Image/DataTxt")]
    public TextMeshProUGUI DataTxt { get; }

    [LazyInitUI("/LangTxt")]
    public TextMeshProUGUI LangTxt { get; }

    [LazyInitUI("/LanguageDropdown")]
    public TMP_Dropdown LanguageDropdown { get; }

    [LazyInitUI("/MoneySlider")]
    public Slider MoneySlider { get; }

    [LazyInitUI("/MoneyTxt")]
    public TextMeshProUGUI MoneyTxt { get; }

    [LazyInitUI("/NameInputOp")]
    public OpComponent NameInputOp { get; }

    [LazyInitUI("/NameTxt")]
    public TextMeshProUGUI NameTxt { get; }
}

#endregion // UniVue Auto-Generated