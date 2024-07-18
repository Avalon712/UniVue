using UnityEngine;

namespace UniVue.ViewModel
{
    public abstract class CollectionPropertyUI : PropertyUI
    {
        protected CollectionPropertyUI(string propertyName) : base(propertyName)
        {
        }

        public override void UpdateUI(int propertyValue) { }

        public override void UpdateUI(float propertyValue) { }

        public override void UpdateUI(string propertyValue) { }

        public override void UpdateUI(Sprite propertyValue) { }

        public override void UpdateUI(bool propertyValue) { }

    }
}
