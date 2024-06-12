using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UniVue.ViewModel.Models
{
    /// <summary>
    /// 布尔类型的数据绑定到UI或UI显示布尔类型的数据
    /// bool类型绑定的UI只能为Toggle
    /// </summary>
    public sealed class BoolPropertyToggle : PropertyUI
    {
        /// <summary>
        /// 绑定的UI组件
        /// </summary>
        private Toggle _toggle;

        public BoolPropertyToggle(Toggle toggle, string propertyName, bool allowUIUpdateModel) : base(propertyName, allowUIUpdateModel)
        {
            _toggle = toggle;
            if (allowUIUpdateModel)
            {
                //监听UI的更新
                toggle.onValueChanged.AddListener(UpdateModel);
            }
        }

        public override void SetActive(bool active)
        {
            if (active != _toggle.gameObject.activeSelf)
            {
                _toggle.gameObject.SetActive(active);
            }
        }

        private void UpdateModel(bool value)
        {
            Vue.Updater.Publisher = this;
            _notifier?.NotifyModelUpdate(_propertyName, value);
        }
        public override void Unbind()
        {
            if (_allowUIUpdateModel) { _toggle.onValueChanged.RemoveListener(UpdateModel); }
            _notifier = null; _propertyName = null; _toggle = null;
        }

        public override void UpdateUI(int propertyValue) { }

        public override void UpdateUI(float propertyValue) { }

        public override void UpdateUI(string propertyValue) { }

        public override void UpdateUI(Sprite propertyValue) { }

        public override void UpdateUI(bool propertyValue)
        {
            if (!IsPublisher())
                _toggle.isOn = propertyValue;
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _toggle as T;
        }
    }
}
