using TMPro;

namespace UniVue.Evt.Evts
{
    public sealed class DropdownEvent : UIEvent
    {
        private TMP_Dropdown _dropdown;

        public DropdownEvent(string viewName, string eventName, TMP_Dropdown dropdown, EventArg[] eventArgs = null) : base(viewName, eventName, eventArgs)
        {
            _dropdown = dropdown;
            dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(int i)
        {
            Trigger();
        }

        public override void Unregister()
        {
            _dropdown.onValueChanged.RemoveListener(OnValueChanged);
            _dropdown = null;
            base.Unregister();
        }

        public override T GetEventUI<T>()
        {
            return _dropdown as T;
        }
    }
}
