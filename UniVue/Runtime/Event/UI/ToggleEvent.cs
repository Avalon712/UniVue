using UnityEngine.UI;
using UniVue.Common;

namespace UniVue.Event
{
    public sealed class ToggleEvent : EventUI
    {
        private Toggle _toggle;

        /// <summary>
        /// 只有当Toggle.isOn=true时才触发此事件
        /// </summary>
        public bool OnlyTrue { get; set; }

        public ToggleEvent(string viewName, string eventName, Toggle toggle, ArgumentUI[] eventArgs = null)
            : base(UIType.Toggle, viewName, eventName, eventArgs)
        {
            _toggle = toggle;
            toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(bool v)
        {
            if ((OnlyTrue && v) || !OnlyTrue)
                Execute();
        }

        public override void Unregister()
        {
            _toggle.onValueChanged.RemoveListener(OnValueChanged);
            _toggle = null;
            base.Unregister();
        }

        public override T GetUI<T>()
        {
            return _toggle as T;
        }
    }
}
