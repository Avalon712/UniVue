using System;
using System.Collections.Generic;
using TMPro;

namespace UniVue.ViewModel
{
    public sealed class EnumDropdown : EnumUI
    {
        private TMP_Dropdown _ui;

        public EnumDropdown(TMP_Dropdown ui, Type enumType, string propertyName, bool allowUIUpdateModel)
            : base(enumType, propertyName, allowUIUpdateModel)
        {
            _ui = ui;
            if (_allowUIUpdateModel) { ui.onValueChanged.AddListener(UpdateModel); }
            ShowAllAlias();
        }

        public override void SetActive(bool active)
        {
            if (active != _ui.gameObject.activeSelf)
            {
                _ui.gameObject.SetActive(active);
            }
        }

        private void UpdateModel(int optionIdx)
        {
            Vue.Updater.Publisher = this;
            string alias = _ui.options[optionIdx].text;
            _notifier?.NotifyModelUpdate(PropertyName, GetValue(alias));
        }

        public override void Unbind()
        {
            if (_allowUIUpdateModel) { _ui.onValueChanged.RemoveListener(UpdateModel); }
            base.Unbind();
        }

        public override void UpdateUI(int propertyValue)
        {
            _value = propertyValue;
            if (IsPublisher()) { return; }
            _ui.captionText.text = GetAliasByValue(propertyValue);
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _ui as T;
        }

        internal override void UpdateUI()
        {
            ShowAllAlias();
            UpdateUI(_value);
        }

        private void ShowAllAlias()
        {
            int count = EnumCount;
            for (int i = 0; i < count; i++)
            {
                if (i >= _ui.options.Count)
                    _ui.options.Add(new TMP_Dropdown.OptionData(GetAliasByIndex(i)));
                else
                    _ui.options[i].text = GetAliasByIndex(i);
            }
            if (_ui.options.Count > count)
            {
                _ui.options.RemoveRange(count, _ui.options.Count - count);
            }
        }
    }
}
