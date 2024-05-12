using UnityEngine.UI;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public sealed class IntPropertySlider : IntPropertyUI<Slider>
    {
        public IntPropertySlider(Slider ui, IModelNotifier notifier, string propertyName, bool allowUIUpdateModel)
            : base(ui, notifier, propertyName, allowUIUpdateModel)
        {
            if (_allowUIUpdateModel) { ui.onValueChanged.AddListener(UpdateModel); }
        }

        private void UpdateModel(float value)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //指示不用更新当前的UI

            _notifier.NotifyModelUpdate(_propertyName, value);
        }


        public override void Dispose()
        {
            if (_allowUIUpdateModel) { _ui.onValueChanged.RemoveListener(UpdateModel); }
            base.Dispose();
        }

        public override void UpdateUI(int propertyValue)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //不要触发OnValueChanged事件

            _ui.value = propertyValue;
        }
    }
}
