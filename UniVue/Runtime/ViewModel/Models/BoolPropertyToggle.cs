﻿using UnityEngine;
using UnityEngine.UI;
using UniVue.Model;

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

        public BoolPropertyToggle(Toggle toggle,IModelNotifier notifier, string propertyName, bool allowUIUpdateModel) : base(notifier, propertyName, allowUIUpdateModel)
        {
            _toggle = toggle;
            if (allowUIUpdateModel)
            {
                //监听UI的更新
                toggle.onValueChanged.AddListener(UpdateModel);
            }
        }

        private void UpdateModel(bool value)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //指示不用更新当前的UI

            _notifier.NotifyModelUpdate(_propertyName, value);
        }
        public override void Dispose()
        {
            if (_allowUIUpdateModel) { _toggle.onValueChanged.RemoveListener(UpdateModel); }
            _notifier = null;_propertyName = null; _toggle = null;
        }

        public override void UpdateUI(string propertyName, int propertyValue) { }

        public override void UpdateUI(string propertyName, float propertyValue) { }

        public override void UpdateUI(string propertyName, string propertyValue) { }

        public override void UpdateUI(string propertyName, Sprite propertyValue) { }

        public override void UpdateUI(string propertyName, bool propertyValue)
        {
            if (!_needUpdate) { _needUpdate = true; return; }
            _needUpdate = false; //不要触发OnValueChanged事件

            _toggle.isOn = propertyValue;
        }
    }
}
