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

        public override void Dispose()
        {
            _img = null; _propertyName = null; _notifier = null;
        }

        public override void UpdateUI(string propertyName, int propertyValue){ }

        public override void UpdateUI(string propertyName, float propertyValue){ }

        public override void UpdateUI(string propertyName, string propertyValue){ }

        //为null时隐藏显示
        public override void UpdateUI(string propertyName, Sprite propertyValue)
        {
            if(propertyValue == null) { _img.gameObject.SetActive(false); }
            else
            {
                if (!_img.gameObject.activeSelf) { _img.gameObject.SetActive(true); }
            }
            _img.sprite = propertyValue;
        }

        public override void UpdateUI(string propertyName, bool propertyValue) { }
    }
}
