using UnityEngine;

namespace UniVue.ViewModel
{
    public abstract class IntUI<UI> : SingleValuePropertyUI where UI : class
    {
        protected int _value = int.MinValue;
        /// <summary>
        /// 绑定的UI组件
        /// </summary>
        protected UI _ui;

        protected IntUI(UI ui, string propertyName, bool allowUIUpdateModel) : base(propertyName, allowUIUpdateModel)
        {
            _ui = ui;
        }
        public override void SetActive(bool active)
        {
            if (active != (_ui as Component)?.gameObject.activeSelf)
            {
                (_ui as Component)?.gameObject.SetActive(active);
            }
        }

        public sealed override void UpdateUI(float propertyValue) { }

        public sealed override void UpdateUI(string propertyValue) { }

        public sealed override void UpdateUI(Sprite propertyValue) { }

        public sealed override void UpdateUI(bool propertyValue) { }
    }
}
