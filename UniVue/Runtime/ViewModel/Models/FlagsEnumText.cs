using System;
using System.Collections.Generic;
using System.Text;
using TMPro;

namespace UniVue.ViewModel
{
    public sealed class FlagsEnumText : EnumUI
    {
        private TMP_Text _text;

        public FlagsEnumText(TMP_Text ui, Type enumType, string propertyName) : base(enumType, propertyName, false)
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
            string separator = Vue.Config.FlagsEnumSeparator;
            StringBuilder builder = new StringBuilder();
            int count = EnumCount;
            for (int i = 0; i < count; i++)
            {
                string alias = GetAliasByIndex(i);
                int value = GetValue(alias);
                if ((value & propertyValue) == value)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(separator);
                    }
                    builder.Append(alias);
                }
            }
            string str = builder.ToString();
            SetActive(!string.IsNullOrEmpty(str) || !Vue.Config.WhenValueIsNullThenHide);
            _text.text = str;
        }


    }
}
