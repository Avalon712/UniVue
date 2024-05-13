using UnityEngine;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public abstract class StringPropertyUI<UI> : PropertyUI where UI : Component
    {
        /// <summary>
        /// 绑定的UI组件
        /// </summary>
        protected UI _ui;

        protected StringPropertyUI(UI ui,IModelNotifier notifier, string propertyName, bool allowUIUpdateModel) 
            : base(notifier, propertyName, allowUIUpdateModel)
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

        public override void Dispose() { _notifier = null;_propertyName = null;  _ui = default; }

        public sealed override void UpdateUI(int propertyValue) { }

        public sealed override void UpdateUI(float propertyValue) { }

        public sealed override void UpdateUI(Sprite propertyValue) { }

        public sealed override void UpdateUI(bool propertyValue) { }
    }
}
