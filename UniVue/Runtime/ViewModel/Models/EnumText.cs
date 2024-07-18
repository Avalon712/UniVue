using System;
using System.Collections.Generic;
using TMPro;

namespace UniVue.ViewModel
{
    public sealed class EnumText : EnumUI
    {
        private TMP_Text _text;

        public EnumText(TMP_Text ui, Type enumType, string propertyName) : base(enumType, propertyName, false)
        {
            _text = ui;
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _text as T;
        }

        public override void SetActive(bool active)
        {
            if (active != _text.gameObject.activeSelf)
            {
                _text.gameObject.SetActive(active);
            }
        }


        public override void UpdateUI(int propertyValue)
        {
            _value = propertyValue;
            string v = GetAliasByValue(propertyValue);

            SetActive(!string.IsNullOrEmpty(v) || !Vue.Config.WhenValueIsNullThenHide);

            _text.text = v;
        }

    }
}
