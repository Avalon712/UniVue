using UnityEngine.UI;

namespace UniVue.ViewModel
{
    public sealed class FloatImage : FloatUI<Image>
    {
        public FloatImage(Image ui, string propertyName) : base(ui, propertyName, false)
        {
        }

        public override void UpdateUI(float propertyValue)
        {
            _ui.fillAmount = propertyValue;

            if (propertyValue <= 0 && Vue.Config.whenFillAmountEqualZeroThenHide)
                SetActive(false);
            else if (propertyValue > 0)
                SetActive(propertyValue > 0);
        }
    }
}
