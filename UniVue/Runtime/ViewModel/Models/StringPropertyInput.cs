using TMPro;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public sealed class StringPropertyInput : StringPropertyUI<TMP_InputField>
    {

        public StringPropertyInput(TMP_InputField ui, IModelNotifier notifier, string propertyName, bool allowUIUpdateModel) : base(ui, notifier, propertyName, allowUIUpdateModel)
        {
            if (_allowUIUpdateModel) { ui.onEndEdit.AddListener(UpdateModel); }
        }

        private void UpdateModel(string value)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //指示不用更新当前的UI

            _notifier.NotifyModelUpdate(_propertyName, value);
        }

        public override void Dispose()
        {
            if (_allowUIUpdateModel) { _ui.onEndEdit.RemoveListener(UpdateModel); }
            base.Dispose();
        }

        public override void UpdateUI(string propertyValue)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //不用触发OnValueChanged事件

            SetActive(!string.IsNullOrEmpty(propertyValue) || !Vue.Config.WhenValueIsNullThenHide);

            _ui.text = propertyValue;
        }
    }
}
