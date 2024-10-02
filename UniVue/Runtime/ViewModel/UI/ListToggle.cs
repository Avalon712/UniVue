using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Common;

namespace UniVue.ViewModel
{
    public sealed class ListToggle : CollectionPropertyUI
    {
        private Toggle[] _toggles;

        public ListToggle(string propertyName, Toggle[] toggles) : base(propertyName)
        {
            _toggles = toggles;
        }

        public override IEnumerable<T> GetUI<T>()
        {
            for (int i = 0; i < _toggles.Length; i++)
            {
                yield return _toggles[i] as T;
            }
        }

        public override void SetActive(bool active)
        {
            for (int i = 0; i < _toggles.Length; i++)
            {
                GameObjectUtil.SetActive(_toggles[i].gameObject, active);
            }
        }

        public override void UpdateUI(List<bool> propertyValue)
        {
            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;
            for (int i = 0; i < _toggles.Length; i++)
            {
                GameObjectUtil.SetActive(_toggles[i].gameObject, i < propertyValue.Count | config);
                if (i < propertyValue.Count)
                    _toggles[i].isOn = propertyValue[i];
            }
        }


        public override void UpdateUI(List<int> propertyValue) { }

        public override void UpdateUI(List<float> propertyValue) { }

        public override void UpdateUI(IList propertyValue) { }

        public override void UpdateUI(List<string> propertyValue) { }


        public override void UpdateUI(List<Sprite> propertyValue) { }
    }
}
