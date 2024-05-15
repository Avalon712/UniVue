using UnityEngine.UI;

namespace UniVue.Evt.Evts
{
    public sealed class ButtonEvent : UIEvent
    {
        private Button _btn;

        public ButtonEvent(string viewName, string eventName, Button btn): base(viewName, eventName)
        {
            _btn = btn; 
            btn.onClick.AddListener(Trigger);
        }

        public override T GetEventUI<T>()
        {
            return _btn as T;
        }

        public override void Unregister()
        {
            _btn.onClick.RemoveListener(Trigger);
            _btn = null;
            base.Unregister();
        }
    }
}
