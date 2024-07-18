using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UniVue.Utils;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 多选
    /// 获取Toggle孩子身上的Text或TMP_Text组件中的值
    /// </summary>
    public sealed class FlagsEnumToggles : EnumUI
    {
        private ValueTuple<Toggle, string>[] _uis;

        public FlagsEnumToggles(ValueTuple<Toggle, string>[] ui, Type enumType, string propertyName, bool allowUIUpdateModel)
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
            Vue.Updater.Publisher = this;
            int v = 0;
            for (int i = 0; i < _uis.Length; i++)
            {
                if (_uis[i].Item1.isOn)
                {
                    v |= GetValue(_uis[i].Item2);
                }
            }
            _notifier?.NotifyModelUpdate(PropertyName, v);
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

        private void SetIsOn(string value, bool isOn)
        {
            for (int i = 0; i < _uis.Length; i++)
            {
                if (value.Equals(_uis[i].Item2))
                {
                    _uis[i].Item1.isOn = isOn;
                    return;
                }
            }
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

        public override void UpdateUI(int propertyValue)
        {
            _value = propertyValue;
            if (IsPublisher()) { return; }
            int count = EnumCount;
            for (int i = 0; i < count; i++)
            {
                string alias = GetAliasByIndex(i);
                int value = GetValue(alias);
                SetIsOn(alias, (propertyValue & value) == value);
            }
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
