using UnityEngine.UI;

namespace UniVue.Evt.Evts
{
    public sealed class SliderEvent : UIEvent
    {
        private Slider _slider;

        public SliderEvent(string viewName, string eventName,Slider slider) : base(viewName, eventName)
        {
            _slider = slider;
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(float newValue)
        {
            Trigger();
        }

        public override void Unregister()
        {
            _slider.onValueChanged.RemoveListener(OnValueChanged);
            _slider = null;
            base.Unregister();
        }

        public override T GetEventUI<T>()
        {
            return _slider as T;
        }
    }
}
