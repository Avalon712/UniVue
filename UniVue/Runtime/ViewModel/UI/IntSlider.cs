using System.Collections.Generic;
using UnityEngine.UI;

namespace UniVue.ViewModel
{
    public sealed class IntSlider : IntUI<Slider>
    {
        public IntSlider(Slider ui, string propertyName, bool allowUIUpdateModel) : base(ui, propertyName, allowUIUpdateModel)
        {
            if (_allowUIUpdateModel) { ui.onValueChanged.AddListener(UpdateModel); }
        }

        private void UpdateModel(float value)
        {
            _value = (int)value;
            Bundle?.UpdateModel(PropertyName, _value);
        }

        public override void UpdateUI(int propertyValue)
        {
            if (_value != propertyValue)
            {
                _value = propertyValue;
                _ui.value = propertyValue;
            }
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _ui as T;
        }
    }
}
