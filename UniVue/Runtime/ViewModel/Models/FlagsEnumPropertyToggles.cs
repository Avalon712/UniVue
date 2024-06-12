using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace UniVue.ViewModel.Models
{
    /// <summary>
    /// 多选
    /// 获取Toggle孩子身上的Text或TMP_Text组件中的值
    /// </summary>
    public sealed class FlagsEnumPropertyToggles : EnumPropertyUI<ValueTuple<Toggle, string>[]>
    {
        public FlagsEnumPropertyToggles(ValueTuple<Toggle, string>[] ui, Array array, string propertyName, bool allowUIUpdateModel)
            : base(ui, array, propertyName, allowUIUpdateModel)
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
        public override void SetActive(bool active) { }

        private void UpdateModel(bool isOn)
        {
            Vue.Updater.Publisher = this;
            int v = 0;
            for (int i = 0; i < _ui.Length; i++)
            {
                if (_ui[i].Item1.isOn)
                {
                    v |= GetValue(_ui[i].Item2);
                }
            }
            _notifier?.NotifyModelUpdate(_propertyName, v);
        }

        private void SetIsOn(string value, bool isOn)
        {
            for (int i = 0; i < _ui.Length; i++)
            {
                if (value.Equals(_ui[i].Item2))
                {
                    _ui[i].Item1.isOn = isOn;
                    return;
                }
            }
        }

        public override void Unbind()
        {
            if (_allowUIUpdateModel)
            {
                for (int i = 0; i < _ui.Length; i++)
                {
                    _ui[i].Item1.onValueChanged.RemoveListener(UpdateModel);
                }
            }

            base.Unbind();
        }

        public override void UpdateUI(int propertyValue)
        {
            if (IsPublisher()) { return; }

            for (int i = 0; i < _enums.Count; i++)
            {
                SetIsOn(_enums[i].Item2, (propertyValue & _enums[i].Item3) == _enums[i].Item3);
            }
        }

        public override IEnumerable<T> GetUI<T>()
        {
            if (_ui[0].Item1 is T)
            {
                for (int i = 0; i < _ui.Length; i++)
                {
                    yield return _ui[i].Item1 as T;
                }
            }
        }
    }

}
