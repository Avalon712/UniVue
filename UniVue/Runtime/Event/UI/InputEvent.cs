using TMPro;
using UniVue.Common;

namespace UniVue.Event
{
    public sealed class InputEvent : EventUI
    {
        private TMP_InputField _input;

        public InputEvent(string viewName, string eventName, TMP_InputField input, ArgumentUI[] eventArgs = null)
            : base(UIType.TMP_InputField, viewName, eventName, eventArgs)
        {
            _input = input;
            input.onEndEdit.AddListener(InputTriggerEvt);
        }

        private void InputTriggerEvt(string v)
        {
            Execute();
        }

        public override void Unregister()
        {
            _input.onEndEdit.RemoveListener(InputTriggerEvt);
            _input = null;
            base.Unregister();
        }

        public override T GetUI<T>()
        {
            return _input as T;
        }
    }
}
