using TMPro;

namespace UniVue.ViewModel
{
    public sealed class StringInput : StringUI<TMP_InputField>
    {
        public StringInput(TMP_InputField ui, string propertyName, bool allowUIUpdateModel) : base(ui, propertyName, allowUIUpdateModel)
        {
            if (_allowUIUpdateModel) { ui.onEndEdit.AddListener(UpdateModel); }
        }

        private void UpdateModel(string value)
        {
            _value = value;
            Bundle?.UpdateModel(PropertyName, value);
        }

        public override void UpdateUI(string propertyValue)
        {
            if (_value != propertyValue)
            {
                _value = propertyValue;
                SetActive(!string.IsNullOrEmpty(propertyValue) || !Vue.Config.WhenValueIsNullThenHide);
                _ui.text = propertyValue;
            }
        }
    }
}
