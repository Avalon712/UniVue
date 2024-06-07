using System;
using TMPro;

namespace UniVue.ViewModel.Models
{
    public sealed class EnumPropertyInput : EnumPropertyUI<TMP_InputField>
    {
        public EnumPropertyInput(TMP_InputField ui, Array array, string propertyName, bool allowUIUpdateModel) : base(ui, array, propertyName, allowUIUpdateModel)
        {
            if (allowUIUpdateModel) { ui.onEndEdit.AddListener(UpdateModel); }
        }

        public override void SetActive(bool active)
        {
            if (active != _ui.gameObject.activeSelf)
            {
                _ui.gameObject.SetActive(active);
            }
        }

        private void UpdateModel(string str) 
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //指示不用更新当前的UI

            _notifier?.NotifyModelUpdate(_propertyName, GetValue(str));
        }

        public override void Unbind()
        {
            if (_allowUIUpdateModel) { _ui.onEndEdit.RemoveListener(UpdateModel); }
            base.Unbind();
        }


        public override void UpdateUI(int propertyValue)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //不要触发OnVauleChanged

            string v = GetAlias(propertyValue);
            SetActive(!string.IsNullOrEmpty(v) || !Vue.Config.WhenValueIsNullThenHide);

            _ui.text = v;
        }

    }

}
