using UnityEngine;

namespace UniVue.Model
{
    public interface IUIUpdater
    {
        /// <summary>
        /// 更新UI
        /// </summary>
        /// <typeparam name="T">int/enum</typeparam>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void UpdateUI(string propertyName, int propertyValue);

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <typeparam name="T">float</typeparam>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void UpdateUI(string propertyName, float propertyValue);

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <typeparam name="T">string</typeparam>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void UpdateUI(string propertyName, string propertyValue);

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <typeparam name="T">更新Image专用</typeparam>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void UpdateUI(string propertyName, Sprite propertyValue);

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <typeparam name="T">bool</typeparam>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void UpdateUI(string propertyName, bool propertyValue);
    }
}
