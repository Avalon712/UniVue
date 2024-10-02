using TMPro;
using UniVue.Common;

namespace UniVue.Event
{
    public sealed class DropdownEvent : EventUI
    {
        private TMP_Dropdown _dropdown;

        public DropdownEvent(string viewName, string eventName, TMP_Dropdown dropdown, ArgumentUI[] eventArgs = null)
            : base(UIType.TMP_Dropdown, viewName, eventName, eventArgs)
        {
            _dropdown = dropdown;
            dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(int i)
        {
            Execute();
        }

        public override void Unregister()
        {
            _dropdown.onValueChanged.RemoveListener(OnValueChanged);
            _dropdown = null;
            base.Unregister();
        }

        public override T GetUI<T>()
        {
            return _dropdown as T;
        }
    }
}
