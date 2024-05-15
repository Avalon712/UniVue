using TMPro;

namespace UniVue.Evt.Evts
{
    public sealed class InputEvent : UIEvent
    {
        private TMP_InputField _input;

        public InputEvent(string viewName, string eventName,TMP_InputField input) : base(viewName, eventName)
        {
            _input = input;
            input.onEndEdit.AddListener(InputTriggerEvt);
        }

        private void InputTriggerEvt(string v)
        {
            Trigger();
        }

        public override void Unregister()
        {
            _input.onEndEdit.RemoveListener(InputTriggerEvt);
            _input = null;
            base.Unregister();
        }

        public override T GetEventUI<T>()
        {
            return _input as T;
        }
    }
}
