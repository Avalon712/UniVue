using TMPro;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public sealed class StringPropertyText : StringPropertyUI<TMP_Text>
    {
        public StringPropertyText(TMP_Text ui, IModelNotifier notifier, string propertyName) 
            : base(ui, notifier, propertyName, false)
        {
        }

        public override void UpdateUI(string propertyValue)
        {
            SetActive(!string.IsNullOrEmpty(propertyValue) || !Vue.Config.WhenValueIsNullThenHide);

            _ui.text = propertyValue;
        }
    }
}
