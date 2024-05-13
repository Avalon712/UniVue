using System;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Utils;

namespace UniVue.ViewModel.Models
{
    /// <summary>
    /// Toggle绑定枚举时获取枚举值的方式: 
    /// 获取Toggle孩子身上的Text或TMP_Text组件中的值
    /// 单选效果
    /// </summary>
    public sealed class EnumPropertyToggleGroup : EnumPropertyUI<CustomTuple<Toggle, string>[]>
    {
        public EnumPropertyToggleGroup(CustomTuple<Toggle, string>[] ui, Array array, IModelNotifier notifier, string propertyName, bool allowUIUpdateModel)
            : base(ui, array, notifier, propertyName, allowUIUpdateModel)
        {
            if (_allowUIUpdateModel)
            {
                for (int i = 0; i < ui.Length; i++)
                {
                    ui[i].Item1.onValueChanged.AddListener(UpdateModel);
                }
            }
        }

        //空实现
        public override void SetActive(bool active){ }

        private void UpdateModel(bool isOn)
        {
            if (isOn)
            {
                if (!_needUpdate) { _needUpdate = true; return; }
                _needUpdate = false; //指示不用更新当前的UI

                CustomTuple<Toggle, string> toggle = GetActiveToggle();
                _notifier.NotifyModelUpdate(_propertyName, GetValue(toggle.Item2));
            }
        }

        public override void UpdateUI(int propertyValue)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //不要触发OnValueChanged事件

            string v = GetAlias(propertyValue);
            SetIsOn(v, GetName(propertyValue), true);
        }

        private void SetIsOn(string value, string name, bool isOn)
        {
            for (int i = 0; i < _ui.Length; i++)
            {
                if (value.Equals(_ui[i].Item2) || name.Equals(_ui[i].Item2))
                {
                    _ui[i].Item1.isOn = isOn;
                    break;
                }
            }
        }

        private CustomTuple<Toggle, string> GetActiveToggle()
        {
            for (int i = 0; i < _ui.Length; i++)
            {
                if (_ui[i].Item1.isOn) { return _ui[i]; }
            }
            return null;
        }

        public override void Dispose()
        {
            if (_allowUIUpdateModel)
            {
                for (int i = 0; i < _ui.Length; i++)
                {
                    _ui[i].Item1.onValueChanged.RemoveListener(UpdateModel);
                }
            }

            for (int i = 0; i < _ui.Length; i++)
            {
                _ui[i].Dispose();
                _ui[i] = null;
            }

            base.Dispose();
        }

    }
}
