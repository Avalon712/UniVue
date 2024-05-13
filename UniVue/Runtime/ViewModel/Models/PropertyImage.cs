using UnityEngine;
using UnityEngine.UI;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    public sealed class PropertyImage : PropertyUI
    {
        private Image _img;

        public PropertyImage(Image img,IModelNotifier notifier, string propertyName)
            : base(notifier, propertyName, false)
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

        public override void Dispose()
        {
            _img = null; _propertyName = null; _notifier = null;
        }

        public override void UpdateUI(int propertyValue){ }

        public override void UpdateUI(float propertyValue){ }

        public override void UpdateUI(string propertyValue){ }

        //为null时隐藏显示
        public override void UpdateUI(Sprite propertyValue)
        {
            SetActive(propertyValue != null || !Vue.Config.WhenValueIsNullThenHide);

            _img.sprite = propertyValue;
        }

        public override void UpdateUI(bool propertyValue) { }
    }
}
