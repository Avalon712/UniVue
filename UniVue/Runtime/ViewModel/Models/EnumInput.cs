using System;
using System.Collections.Generic;
using TMPro;

namespace UniVue.ViewModel
{
    public sealed class EnumInput : EnumUI
    {
        private TMP_InputField _input;

        public EnumInput(TMP_InputField ui, Type enumType, string propertyName, bool allowUIUpdateModel) : base(enumType, propertyName, allowUIUpdateModel)
        {
            _input = ui;
            if (allowUIUpdateModel) { ui.onEndEdit.AddListener(UpdateModel); }
        }

        public override void SetActive(bool active)
        {
            if (active != _input.gameObject.activeSelf)
            {
                _input.gameObject.SetActive(active);
            }
        }

        private void UpdateModel(string alias)
        {
            Vue.Updater.Publisher = this;
            _notifier?.NotifyModelUpdate(PropertyName, GetValue(alias));
        }

        public override void Unbind()
        {
            if (_allowUIUpdateModel) { _input.onEndEdit.RemoveListener(UpdateModel); }
            base.Unbind();
        }


        public override void UpdateUI(int propertyValue)
        {
            _value = propertyValue;
            if (!IsPublisher())
            {
                string v = GetAliasByValue(propertyValue);
                SetActive(!string.IsNullOrEmpty(v) || !Vue.Config.WhenValueIsNullThenHide);
                _input.text = v;
            }
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _input as T;
        }

    }

}
