using System.Collections.Generic;
using TMPro;
using UniVue.Common;

namespace UniVue.ViewModel
{
    public sealed class EnumInput : EnumUI
    {
        private TMP_InputField _input;

        public EnumInput(TMP_InputField ui, string enumTypeFullName, string propertyName, bool allowUIUpdateModel) : base(enumTypeFullName, propertyName, allowUIUpdateModel)
        {
            _input = ui;
            if (allowUIUpdateModel) { ui.onEndEdit.AddListener(UpdateModel); }
        }

        public override void SetActive(bool active)
        {
            GameObjectUtil.SetActive(_input.gameObject, active);
        }

        private void UpdateModel(string alias)
        {
            _value = GetIntValueByAlias(alias);
            Bundle?.UpdateModel(PropertyName, _value);
        }


        public override void UpdateUI(int propertyValue)
        {
            if (_value != propertyValue)
            {
                _value = propertyValue;
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
