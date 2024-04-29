using UnityEngine;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    /// <summary>
    /// 单精度浮点型的属性绑定UI
    /// </summary>
    public abstract class FloatPropertyUI<UI> : PropertyUI
    {
        /// <summary>
        /// 绑定的UI组件
        /// </summary>
        protected UI _ui;

        protected FloatPropertyUI(UI ui,IModelNotifier notifier, string propertyName, bool allowUIUpdateModel) : base(notifier, propertyName, allowUIUpdateModel)
        {
            _ui = ui;
        }

        public sealed override void UpdateUI(string propertyName, bool propertyValue) { }

        public sealed override void UpdateUI(string propertyName, int propertyValue) { }

        public sealed override void UpdateUI(string propertyName, string propertyValue) { }

        public sealed override void UpdateUI(string propertyName, Sprite propertyValue) { }

        public override void Dispose() { _notifier = null;_propertyName=null ; _ui = default; }

    }
}
