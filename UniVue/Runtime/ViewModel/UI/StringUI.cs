using System.Collections.Generic;
using UnityEngine;

namespace UniVue.ViewModel
{
    public abstract class StringUI<UI> : SingleValuePropertyUI where UI : Component
    {
        protected string _value;
        /// <summary>
        /// 绑定的UI组件
        /// </summary>
        protected UI _ui;

        protected StringUI(UI ui, string propertyName, bool allowUIUpdateModel) : base(propertyName, allowUIUpdateModel)
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

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _ui as T;
        }

        public sealed override void UpdateUI(int propertyValue) { }

        public sealed override void UpdateUI(float propertyValue) { }

        public sealed override void UpdateUI(Sprite propertyValue) { }

        public sealed override void UpdateUI(bool propertyValue) { }
    }
}
