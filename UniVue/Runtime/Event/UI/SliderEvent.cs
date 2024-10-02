using UnityEngine.UI;
using UniVue.Common;

namespace UniVue.Event
{
    public sealed class SliderEvent : EventUI
    {
        private Slider _slider;

        public SliderEvent(string viewName, string eventName, Slider slider, ArgumentUI[] eventArgs = null)
            : base(UIType.Slider, viewName, eventName, eventArgs)
        {
            _slider = slider;
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(float newValue)
        {
            Execute();
        }

        public override void Unregister()
        {
            _slider.onValueChanged.RemoveListener(OnValueChanged);
            _slider = null;
            base.Unregister();
        }

        public override T GetUI<T>()
        {
            return _slider as T;
        }
    }
}
