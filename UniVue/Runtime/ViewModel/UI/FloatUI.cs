using System.Collections.Generic;
using UnityEngine;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 单精度浮点型的属性绑定UI
    /// </summary>
    public abstract class FloatUI<UI> : SingleValuePropertyUI where UI : Component
    {
        protected float _value = float.MinValue;
        /// <summary>
        /// 绑定的UI组件
        /// </summary>
        protected UI _ui;

        protected FloatUI(UI ui, string propertyName, bool allowUIUpdateModel) : base(propertyName, allowUIUpdateModel)
        {
            _ui = ui;
        }

        public override void SetActive(bool active)
        {
            if (active != _ui.gameObject.activeSelf)
            {
                _ui.gameObject.SetActive(active);
            }
        }

        public sealed override void UpdateUI(bool propertyValue) { }

        public sealed override void UpdateUI(int propertyValue) { }

        public sealed override void UpdateUI(string propertyValue) { }

        public sealed override void UpdateUI(Sprite propertyValue) { }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _ui as T;
        }
    }
}
