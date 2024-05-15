using System;
using TMPro;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public sealed class EnumPropertyText : EnumPropertyUI<TMP_Text>
    {
        public EnumPropertyText(TMP_Text ui, Array array, IModelNotifier notifier, string propertyName)
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
            string v = GetAlias(propertyValue);

            SetActive(!string.IsNullOrEmpty(v) || !Vue.Config.WhenValueIsNullThenHide);

            _ui.text = v;
        }

    }
}
