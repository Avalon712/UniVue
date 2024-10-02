using UnityEngine.UI;
using UniVue.Common;

namespace UniVue.Event
{
    public sealed class ButtonEvent : EventUI
    {
        private Button _btn;

        public ButtonEvent(string viewName, string eventName, Button btn, ArgumentUI[] eventArgs = null)
            : base(UIType.Button, viewName, eventName, eventArgs)
        {
            _btn = btn;
            btn.onClick.AddListener(Execute);
        }

        public override T GetUI<T>()
        {
            return _btn as T;
        }

        public override void Unregister()
        {
            _btn.onClick.RemoveListener(Execute);
            _btn = null;
            base.Unregister();
        }
    }
}
