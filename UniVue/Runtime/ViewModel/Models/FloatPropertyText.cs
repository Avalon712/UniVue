using TMPro;

namespace UniVue.ViewModel.Models
{
    public sealed class FloatPropertyText : FloatPropertyUI<TMP_Text>
    {
        public FloatPropertyText(TMP_Text ui, string propertyName) : base(ui, propertyName, false)
        {
        }

        public override void UpdateUI(float propertyValue)
        {
            _ui.text = propertyValue.ToString("F" + Vue.Config.FloatKeepBit);
        }
    }
}
