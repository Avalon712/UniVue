using TMPro;

namespace UniVue.ViewModel.Models
{
    public sealed class StringPropertyText : StringPropertyUI<TMP_Text>
    {
        public StringPropertyText(TMP_Text ui, string propertyName) : base(ui, propertyName, false)
        {
        }

        public override void UpdateUI(string propertyValue)
        {
            SetActive(!string.IsNullOrEmpty(propertyValue) || !Vue.Config.WhenValueIsNullThenHide);

            _ui.text = propertyValue;
        }
    }
}
