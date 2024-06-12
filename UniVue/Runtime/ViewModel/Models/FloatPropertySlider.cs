using UnityEngine.UI;

namespace UniVue.ViewModel.Models
{
    public sealed class FloatPropertySlider : FloatPropertyUI<Slider>
    {
        public FloatPropertySlider(Slider ui, string propertyName, bool allowUIUpdateModel) : base(ui, propertyName, allowUIUpdateModel)
        {
            if (_allowUIUpdateModel) { ui.onValueChanged.AddListener(UpdateModel); }
        }

        private void UpdateModel(float value)
        {
            Vue.Updater.Publisher = this;
            _notifier?.NotifyModelUpdate(_propertyName, value);
        }

        public override void Unbind()
        {
            if (_allowUIUpdateModel) { _ui.onValueChanged.RemoveListener(UpdateModel); }
            base.Unbind();
        }

        public override void UpdateUI(float propertyValue)
        {
            if (!IsPublisher())
                _ui.value = propertyValue;
        }
    }
}
