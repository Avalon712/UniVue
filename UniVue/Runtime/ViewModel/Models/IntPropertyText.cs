using TMPro;

namespace UniVue.ViewModel.Models
{
    public sealed class IntPropertyText : IntPropertyUI<TMP_Text>
    {
        public IntPropertyText(TMP_Text ui,string propertyName)  : base(ui,propertyName, false)
        {
        }

        public override void UpdateUI(int propertyValue)
        {
            _ui.text = propertyValue.ToString();
        }
    }
}
