using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UniVue.ViewModel
{
    public sealed class SpriteImage : SingleValuePropertyUI
    {
        private Image _img;

        public SpriteImage(Image img, string propertyName) : base(propertyName, false)
        {
            _img = img;
        }

        public override void SetActive(bool active)
        {
            if (active != _img.gameObject.activeSelf)
            {
                _img.gameObject.SetActive(active);
            }
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _img as T;
        }

        public override void UpdateUI(int propertyValue) { }

        public override void UpdateUI(float propertyValue) { }

        public override void UpdateUI(string propertyValue) { }

        //为null时隐藏显示
        public override void UpdateUI(Sprite propertyValue)
        {
            SetActive(propertyValue != null || !Vue.Config.WhenValueIsNullThenHide);

            _img.sprite = propertyValue;
        }

        public override void UpdateUI(bool propertyValue) { }
    }
}
