using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Common;

namespace UniVue.ViewModel
{
    public sealed class ListImage : CollectionPropertyUI
    {
        private Image[] _images;

        public ListImage(string propertyName, Image[] images) : base(propertyName)
        {
            _images = images;
        }

        public override IEnumerable<T> GetUI<T>()
        {
            for (int i = 0; i < _images.Length; i++)
            {
                yield return _images[i] as T;
            }
        }

        public override void SetActive(bool active)
        {
            for (int i = 0; i < _images.Length; i++)
            {
                GameObjectUtil.SetActive(_images[i].gameObject, active);
            }
        }

        public override void UpdateUI(List<Sprite> propertyValue)
        {
            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;
            for (int i = 0; i < _images.Length; i++)
            {
                if (i < propertyValue.Count)
                    _images[i].sprite = propertyValue[i];

                GameObjectUtil.SetActive(_images[i].gameObject, i < propertyValue.Count | config);
            }
        }


        public override void UpdateUI(List<int> propertyValue) { }

        public override void UpdateUI(List<float> propertyValue) { }

        public override void UpdateUI(IList propertyValue) { }

        public override void UpdateUI(List<string> propertyValue) { }

        public override void UpdateUI(List<bool> propertyValue) { }
    }
}
