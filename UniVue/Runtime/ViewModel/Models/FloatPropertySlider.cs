using UnityEngine.UI;

namespace UniVue.ViewModel.Models
{
    public sealed class FloatPropertySlider : FloatPropertyUI<Slider>
    {
        public FloatPropertySlider(Slider ui,string propertyName, bool allowUIUpdateModel) : base(ui, propertyName, allowUIUpdateModel)
        {
            if (_allowUIUpdateModel) { ui.onValueChanged.AddListener(UpdateModel); }
        }

        private void UpdateModel(float value)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //指示不用更新当前的UI

            _notifier?.NotifyModelUpdate(_propertyName, value);
        }

        public override void Unbind()
        {
            if (_allowUIUpdateModel) { _ui.onValueChanged.RemoveListener(UpdateModel); }
            base.Unbind();
        }

        public override void UpdateUI(float propertyValue)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //不要触发OnValueChanged事件

            _ui.value = propertyValue;
        }
    }
}
