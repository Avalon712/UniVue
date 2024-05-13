using System;
using System.Collections.Generic;
using TMPro;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public sealed class EnumPropertyDropdown : EnumPropertyUI<TMP_Dropdown>
    {
        public EnumPropertyDropdown(TMP_Dropdown ui, Array array, IModelNotifier notifier, string propertyName, bool allowUIUpdateModel)
            : base(ui, array, notifier, propertyName, allowUIUpdateModel)
        {
            if (_allowUIUpdateModel) { ui.onValueChanged.AddListener(UpdateModel); }
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
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //指示不用更新当前的UI

            string enumStr = _ui.options[optionIdx].text;
            _notifier.NotifyModelUpdate(_propertyName, GetValue(enumStr));
        }

        public override void Dispose()
        {
            if (_allowUIUpdateModel) { _ui.onValueChanged.RemoveListener(UpdateModel); }
            base.Dispose();
        }

        public override void UpdateUI(int propertyValue)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //指示不要触发OnValueChanged事件

           // LogUtil.Info($"EnumPropertyDropdown::UpdateUI({propertyName}, {propertyValue})");

            List<TMP_Dropdown.OptionData> optionDatas = _ui.options;
            string str = GetAlias(propertyValue);
            for (int i = 0; i < optionDatas.Count; i++)
            {
                if (optionDatas[i].text == str)
                {
                    _ui.value = i; //会触发OnValueChanged
                    break;
                }
            }
        }

    }
}
