using UnityEngine;

namespace UniVue.Model
{
    public interface IUIUpdater
    {
        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="propertyValue">属性值</param>
        public void UpdateUI(int propertyValue);

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="propertyValue">属性值</param>
        public void UpdateUI(float propertyValue);

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="propertyValue">属性值</param>
        public void UpdateUI(string propertyValue);

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="propertyValue">属性值</param>
        public void UpdateUI(Sprite propertyValue);

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="propertyValue">属性值</param>
        public void UpdateUI(bool propertyValue);
    }
}
