using System.Collections.Generic;
using UnityEngine.UI;

namespace UniVue.ViewModel.Models
{
    public sealed class IntPropertySlider : IntPropertyUI<Slider>
    {
        public IntPropertySlider(Slider ui, string propertyName, bool allowUIUpdateModel) : base(ui, propertyName, allowUIUpdateModel)
        {
            if (_allowUIUpdateModel) { ui.onValueChanged.AddListener(UpdateModel); }
        }

        private void UpdateModel(float value)
        {
            Vue.Updater.Publisher = this;
            _notifier?.NotifyModelUpdate(_propertyName, (int)value);
        }


        public override void Unbind()
        {
            if (_allowUIUpdateModel) { _ui.onValueChanged.RemoveListener(UpdateModel); }
            base.Unbind();
        }

        public override void UpdateUI(int propertyValue)
        {
            if (!IsPublisher())
                _ui.value = propertyValue;
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _ui as T;
        }
    }
}
