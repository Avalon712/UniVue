using UnityEngine;

namespace UniVue.ViewModel.Models
{
    public abstract class IntPropertyUI<UI> : PropertyUI where UI : class
    {
        /// <summary>
        /// 绑定的UI组件
        /// </summary>
        protected UI _ui;

        protected IntPropertyUI(UI ui, string propertyName, bool allowUIUpdateModel) : base(propertyName, allowUIUpdateModel)
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

        public override void Unbind()
        {
            _notifier = null; _propertyName = null; _ui = default;
        }

        public sealed override void UpdateUI(float propertyValue) { }

        public sealed override void UpdateUI(string propertyValue) { }

        public sealed override void UpdateUI(Sprite propertyValue) { }

        public sealed override void UpdateUI(bool propertyValue) { }
    }
}
