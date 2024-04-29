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

        public override void UpdateUI(string propertyName, string propertyValue)
        {
            _ui.text = propertyValue;
        }
    }
}
