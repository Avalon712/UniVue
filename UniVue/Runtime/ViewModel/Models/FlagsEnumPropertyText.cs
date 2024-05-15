using System;
using TMPro;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public sealed class FlagsEnumPropertyText : EnumPropertyUI<TMP_Text>
    {
        public FlagsEnumPropertyText(TMP_Text ui, Array array, IModelNotifier notifier, string propertyName) 
            : base(ui, array, notifier, propertyName, false)
        {
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
            string str = string.Empty;
            for (int i = 0; i < _enums.Count; i++)
            {
                if ((_enums[i].Item3 & propertyValue) == _enums[i].Item3)
                {
                    str = string.Concat(str, _enums[i].Item2, '\\');
                }
            }
            SetActive(!string.IsNullOrEmpty(str) || !Vue.Config.WhenValueIsNullThenHide);

            _ui.text = str;
        }

    }
}
