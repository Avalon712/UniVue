using TMPro;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public sealed class IntPropertyText : IntPropertyUI<TMP_Text>
    {
        public IntPropertyText(TMP_Text ui, IModelNotifier notifier, string propertyName) 
            : base(ui, notifier, propertyName, false)
        {
        }

        public override void UpdateUI(string propertyName, int propertyValue)
        {
            _ui.text = propertyValue.ToString();
        }
    }
}
