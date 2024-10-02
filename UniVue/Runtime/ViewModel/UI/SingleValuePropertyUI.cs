using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniVue.ViewModel
{
    public abstract class SingleValuePropertyUI : PropertyUI
    {
        /// <summary>
        /// 是否允许UI更新模型数据
        /// </summary>
        protected bool _allowUIUpdateModel;

        protected SingleValuePropertyUI(string propertyName, bool allowUIUpdateModel) : base(propertyName)
        {
            _allowUIUpdateModel = allowUIUpdateModel;
        }

        public override void UpdateUI(List<int> propertyValue) { }

        public override void UpdateUI(List<float> propertyValue) { }

        public override void UpdateUI(IList propertyValue) { }

        public override void UpdateUI(List<string> propertyValue) { }

        public override void UpdateUI(List<bool> propertyValue) { }

        public override void UpdateUI(List<Sprite> propertyValue) { }
    }
}
