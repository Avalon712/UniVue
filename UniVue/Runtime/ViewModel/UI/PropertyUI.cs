using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 模型的最小单元所对应的
    /// </summary>
    public abstract class PropertyUI
    {
        /// <summary>
        /// 获取当前属性UI绑定的属性的属性名称
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// 当前UI所属的UIBundle
        /// </summary>
        public ModelUI Bundle { get; internal set; }

        protected PropertyUI(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// 显示或隐藏UI的展示
        /// </summary>
        /// <param name="active">true:显示</param>
        public abstract void SetActive(bool active);

        /// <summary>
        /// 所有属性绑定的所有UI
        /// </summary>
        /// <typeparam name="T">T : Component</typeparam>
        /// <returns>IEnumerable<T></returns>
        public abstract IEnumerable<T> GetUI<T>() where T : Component;


        public abstract void UpdateUI(int propertyValue);
        public abstract void UpdateUI(float propertyValue);
        public abstract void UpdateUI(string propertyValue);
        public abstract void UpdateUI(Sprite propertyValue);
        public abstract void UpdateUI(bool propertyValue);
        public abstract void UpdateUI(List<int> propertyValue);
        public abstract void UpdateUI(List<float> propertyValue);
        public abstract void UpdateUI(IList propertyValue);
        public abstract void UpdateUI(List<string> propertyValue);
        public abstract void UpdateUI(List<bool> propertyValue);
        public abstract void UpdateUI(List<Sprite> propertyValue);
    }

}
