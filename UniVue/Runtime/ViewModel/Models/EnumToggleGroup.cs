using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UniVue.Utils;

namespace UniVue.ViewModel
{
    /// <summary>
    /// Toggle绑定枚举时获取枚举值的方式: 
    /// 获取Toggle孩子身上的Text或TMP_Text组件中的值
    /// 单选效果
    /// </summary>
    public sealed class EnumToggleGroup : EnumUI
    {
        //Item2 : 当前枚举值的别名
        private ValueTuple<Toggle, string>[] _uis;

        public EnumToggleGroup(ValueTuple<Toggle, string>[] ui, Type enumType, string propertyName, bool allowUIUpdateModel)
            : base(enumType, propertyName, allowUIUpdateModel)
        {
            _uis = ui;
            if (_allowUIUpdateModel)
            {
                for (int i = 0; i < ui.Length; i++)
                {
                    ui[i].Item1.onValueChanged.AddListener(UpdateModel);
                }
            }
        }

        public override void SetActive(bool active)
        {
            for (int i = 0; i < _uis.Length; i++)
            {
                ViewUtil.SetActive(_uis[i].Item1.gameObject, active);
            }
        }

        private void UpdateModel(bool isOn)
        {
            if (isOn)
            {
                Vue.Updater.Publisher = this;
                ValueTuple<Toggle, string> toggle = GetActiveToggle();
                _notifier?.NotifyModelUpdate(PropertyName, GetValue(toggle.Item2));
            }
        }

        public override void UpdateUI(int propertyValue)
        {
            _value = propertyValue;
            if (!IsPublisher())
            {
                string v = GetAliasByValue(propertyValue);
                SetIsOn(v, true);
            }
        }

        internal override void UpdateUI()
        {
            for (int i = 0; i < _uis.Length; i++)
            {
                ValueTuple<Toggle, string> ui = _uis[i];
                ui.Item2 = GetNewAlias(ui.Item2);
                ui.Item1.GetComponent<TMP_Text>().text = ui.Item2;
                _uis[i] = ui;
            }
            UpdateUI(_value);
        }

        private void SetIsOn(string alias, bool isOn)
        {
            for (int i = 0; i < _uis.Length; i++)
            {
                if (alias.Equals(_uis[i].Item2))
                {
                    _uis[i].Item1.isOn = isOn;
                    break;
                }
            }
        }

        private ValueTuple<Toggle, string> GetActiveToggle()
        {
            for (int i = 0; i < _uis.Length; i++)
            {
                if (_uis[i].Item1.isOn) { return _uis[i]; }
            }
            return default;
        }

        public override void Unbind()
        {
            if (_allowUIUpdateModel)
            {
                for (int i = 0; i < _uis.Length; i++)
                {
                    _uis[i].Item1.onValueChanged.RemoveListener(UpdateModel);
                }
            }

            base.Unbind();
        }

        public override IEnumerable<T> GetUI<T>()
        {
            if (_uis[0].Item1 is T)
            {
                for (int i = 0; i < _uis.Length; i++)
                {
                    yield return _uis[i].Item1 as T;
                }
            }
        }
    }
}
