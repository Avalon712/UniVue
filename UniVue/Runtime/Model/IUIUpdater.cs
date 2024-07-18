using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniVue.Model
{
    public interface IUIUpdater
    {
        public void UpdateUI(int propertyValue);

        public void UpdateUI(float propertyValue);

        public void UpdateUI(string propertyValue);

        public void UpdateUI(Sprite propertyValue);

        public void UpdateUI(bool propertyValue);

        public void UpdateUI(List<int> propertyValue);

        public void UpdateUI(List<float> propertyValue);

        /// <summary>
        /// 更新List&lt;Enum&gt;属性类型的UI
        /// </summary>
        /// <typeparam name="T">这个泛型必须是枚举类型</typeparam>
        public void UpdateUI(IList propertyValue);

        public void UpdateUI(List<string> propertyValue);

        public void UpdateUI(List<bool> propertyValue);

        public void UpdateUI(List<Sprite> propertyValue);
    }
}
