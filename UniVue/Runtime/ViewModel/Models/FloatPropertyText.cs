using TMPro;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public sealed class FloatPropertyText : FloatPropertyUI<TMP_Text>
    {
        public FloatPropertyText(TMP_Text ui, IModelNotifier notifier, string propertyName)
            : base(ui, notifier, propertyName, false)
        {
        }

        public override void UpdateUI(float propertyValue)
        {
            _ui.text = propertyValue.ToString();
        }
    }
}
