using System.Collections.Generic;
using TMPro;
using UniVue.Common;

namespace UniVue.ViewModel
{
    public sealed class EnumDropdown : EnumUI
    {
        private TMP_Dropdown _ui;

        public EnumDropdown(TMP_Dropdown ui, string enumTypeFullName, string propertyName, bool allowUIUpdateModel)
            : base(enumTypeFullName, propertyName, allowUIUpdateModel)
        {
            _ui = ui;
            if (_allowUIUpdateModel) { ui.onValueChanged.AddListener(UpdateModel); }
            ShowAllAlias();
        }

        public override void SetActive(bool active)
        {
            GameObjectUtil.SetActive(_ui.gameObject, active);
        }

        private void UpdateModel(int optionIdx)
        {
            string alias = _ui.options[optionIdx].text;
            _value = GetIntValueByAlias(alias);
            Bundle?.UpdateModel(PropertyName, _value);
        }

        public override void UpdateUI(int propertyValue)
        {
            if (_value != propertyValue)
            {
                _value = propertyValue;
                _ui.captionText.text = GetAliasByValue(propertyValue);
                SetValueIndex();
            }
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _ui as T;
        }

        public override void OnLanguageEnvironmentChanged()
        {
            ShowAllAlias();
            base.OnLanguageEnvironmentChanged();
        }

        private void SetValueIndex()
        {
            for (int i = 0; i < _ui.options.Count; i++)
            {
                if (_ui.options[i].text == _ui.captionText.text)
                {
                    _ui.value = i;
                    break;
                }
            }
        }

        private void ShowAllAlias()
        {
            int count = ValueCount;
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
