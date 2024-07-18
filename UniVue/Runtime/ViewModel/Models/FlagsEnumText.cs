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
            //for (int i = 0; i < _enums.Count; i++)
            //{
            //    if ((_enums[i].Item3 & propertyValue) == _enums[i].Item3)
            //    {
            //        if (builder.Length > 0)
            //        {
            //            builder.Append(separator);
            //        }
            //        builder.Append(_enums[i].Item2);
            //    }
            //}
            string str = builder.ToString();
            SetActive(!string.IsNullOrEmpty(str) || !Vue.Config.WhenValueIsNullThenHide);
            _text.text = str;
        }


    }
}
