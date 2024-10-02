using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Common;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 布尔类型的数据绑定到UI或UI显示布尔类型的数据
    /// bool类型绑定的UI只能为Toggle
    /// </summary>
    public sealed class BoolToggle : SingleValuePropertyUI
    {
        /// <summary>
        /// 绑定的UI组件
        /// </summary>
        private Toggle _toggle;

        public BoolToggle(Toggle toggle, string propertyName, bool allowUIUpdateModel) : base(propertyName, allowUIUpdateModel)
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
            GameObjectUtil.SetActive(_toggle.gameObject, active);
        }

        private void UpdateModel(bool value)
        {
            Bundle?.UpdateModel(PropertyName, value);
        }


        public override void UpdateUI(int propertyValue) { }

        public override void UpdateUI(float propertyValue) { }

        public override void UpdateUI(string propertyValue) { }

        public override void UpdateUI(Sprite propertyValue) { }

        public override void UpdateUI(bool propertyValue)
        {
            if (_toggle.isOn != propertyValue)
                _toggle.isOn = propertyValue;
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _toggle as T;
        }
    }
}
