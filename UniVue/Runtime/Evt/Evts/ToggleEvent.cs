using UnityEngine.UI;

namespace UniVue.Evt.Evts
{
    public sealed class ToggleEvent : UIEvent
    {
        private Toggle _toggle;

        public ToggleEvent(string viewName, string eventName,Toggle toggle) : base(viewName, eventName)
        {
            _toggle = toggle;
            toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(bool v)
        {
            if (v) { Trigger(); } //只有开启状态才触发事件
        }

        public override void Unregister()
        {
            _toggle.onValueChanged.RemoveListener(OnValueChanged);
            _toggle = null;
            base.Unregister();
        }

        public override T GetEventUI<T>()
        {
            return _toggle as T;
        }
    }
}
