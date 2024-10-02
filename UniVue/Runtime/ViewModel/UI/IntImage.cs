using System.Collections.Generic;
using UnityEngine.UI;

namespace UniVue.ViewModel
{
    public sealed class IntImage : IntUI<Image>
    {
        public IntImage(Image ui, string propertyName) : base(ui, propertyName, false)
        {
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _ui as T;
        }

        public override void UpdateUI(int propertyValue)
        {
            if (_value == propertyValue) return;
            _value = propertyValue;
            _ui.fillAmount = propertyValue;
            if (propertyValue <= 0 && Vue.Config.WhenFillAmountEqualZeroThenHide)
                SetActive(false);
            else if (propertyValue > 0)
                SetActive(true);
        }
    }
}
