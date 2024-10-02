using UnityEngine;
using UnityEngine.UI;

namespace UniVue.ViewModel
{
    public sealed class FloatSlider : FloatUI<Slider>
    {
        public FloatSlider(Slider ui, string propertyName, bool allowUIUpdateModel) : base(ui, propertyName, allowUIUpdateModel)
        {
            if (_allowUIUpdateModel) { ui.onValueChanged.AddListener(UpdateModel); }
        }

        private void UpdateModel(float value)
        {
            _value = value;
            Bundle?.UpdateModel(PropertyName, value);
        }

        public override void UpdateUI(float propertyValue)
        {
            if (!Mathf.Approximately(_value, propertyValue))
            {
                _value = propertyValue;
                _ui.value = propertyValue;
            }
        }
    }
}
