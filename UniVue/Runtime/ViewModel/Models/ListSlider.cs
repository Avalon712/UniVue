using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Utils;

namespace UniVue.ViewModel
{
    public sealed class ListSlider : CollectionPropertyUI
    {
        private Slider[] _sliders;

        public ListSlider(string propertyName, Slider[] sliders) : base(propertyName)
        {
            _sliders = sliders;
        }

        public override IEnumerable<T> GetUI<T>()
        {
            for (int i = 0; i < _sliders.Length; i++)
            {
                yield return _sliders[i] as T;
            }
        }

        public override void SetActive(bool active)
        {
            for (int i = 0; i < _sliders.Length; i++)
            {
                ViewUtil.SetActive(_sliders[i].gameObject, active);
            }
        }

        public override void Unbind()
        {
            for (int i = 0; i < _sliders.Length; i++)
            {
                _sliders[i] = null;
            }
            _sliders = null;
        }

        public override void UpdateUI(List<int> propertyValue)
        {
            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;
            for (int i = 0; i < _sliders.Length; i++)
            {
                if (i < propertyValue.Count)
                    _sliders[i].value = propertyValue[i];
                ViewUtil.SetActive(_sliders[i].gameObject, i < propertyValue.Count | config);
            }
        }

        public override void UpdateUI(List<float> propertyValue)
        {
            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;
            for (int i = 0; i < _sliders.Length; i++)
            {
                if (i < propertyValue.Count)
                    _sliders[i].value = propertyValue[i];

                ViewUtil.SetActive(_sliders[i].gameObject, i < propertyValue.Count | config);
            }
        }

        public override void UpdateUI(IList propertyValue) { }

        public override void UpdateUI(List<string> propertyValue) { }

        public override void UpdateUI(List<bool> propertyValue) { }

        public override void UpdateUI(List<Sprite> propertyValue) { }
    }
}
