using TMPro;

namespace UniVue.ViewModel.Models
{
    public sealed class StringText : StringUI<TMP_Text>
    {
        public StringText(TMP_Text ui, string propertyName) : base(ui, propertyName, false)
        {
        }

        public override void UpdateUI(string propertyValue)
        {
            SetActive(!string.IsNullOrEmpty(propertyValue) || !Vue.Config.WhenValueIsNullThenHide);

            _ui.text = propertyValue;
        }
    }
}
