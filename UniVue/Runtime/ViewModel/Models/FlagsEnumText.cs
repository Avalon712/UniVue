using System;
using System.Collections.Generic;
using System.Text;
using TMPro;

namespace UniVue.ViewModel.Models
{
    public sealed class FlagsEnumText : EnumUI<TMP_Text>
    {
        public FlagsEnumText(TMP_Text ui, Array array, string propertyName) : base(ui, array, propertyName, false)
        {
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _ui as T;
        }

        public override void SetActive(bool active)
        {
            if (active != _ui.gameObject.activeSelf)
            {
                _ui.gameObject.SetActive(active);
            }
        }

        public override void UpdateUI(int propertyValue)
        {
            string separator = Vue.Config.FlagsEnumSeparator;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < _enums.Count; i++)
            {
                if ((_enums[i].Item3 & propertyValue) == _enums[i].Item3)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(separator);
                    }
                    builder.Append(_enums[i].Item2);
                }
            }
            string str = builder.ToString();
            SetActive(!string.IsNullOrEmpty(str) || !Vue.Config.WhenValueIsNullThenHide);
            _ui.text = str;
        }

    }
}
