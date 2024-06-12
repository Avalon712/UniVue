using System;
using System.Collections.Generic;
using TMPro;

namespace UniVue.ViewModel.Models
{
    public sealed class EnumPropertyText : EnumPropertyUI<TMP_Text>
    {
        public EnumPropertyText(TMP_Text ui, Array array, string propertyName) : base(ui, array, propertyName, false)
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
            string v = GetAlias(propertyValue);

            SetActive(!string.IsNullOrEmpty(v) || !Vue.Config.WhenValueIsNullThenHide);

            _ui.text = v;
        }

    }
}
