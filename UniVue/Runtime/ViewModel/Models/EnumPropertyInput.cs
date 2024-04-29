using System;
using TMPro;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public sealed class EnumPropertyInput : EnumPropertyUI<TMP_InputField>
    {
        public EnumPropertyInput(TMP_InputField ui, Array array, IModelNotifier notifier, string propertyName, bool allowUIUpdateModel) : base(ui, array, notifier, propertyName, allowUIUpdateModel)
        {
            if (allowUIUpdateModel) { ui.onEndEdit.AddListener(UpdateModel); }
        }

        private void UpdateModel(string str) 
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //指示不用更新当前的UI

            _notifier.NotifyModelUpdate(_propertyName, GetValue(str));
        }

        public override void Dispose()
        {
            if (_allowUIUpdateModel) { _ui.onEndEdit.RemoveListener(UpdateModel); }
            base.Dispose();
        }


        public override void UpdateUI(string propertyName, int propertyValue)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //不要触发OnVauleChanged

            _ui.text = GetAlias(propertyValue);
        }

    }

}
