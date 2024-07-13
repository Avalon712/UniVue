using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniVue.Model
{
    public interface IUINotifier
    {
        /// <summary>
        /// 通知UI更新数据 - int
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, int propertyValue);

        /// <summary>
        /// 通知UI更新数据 - string
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, string propertyValue);

        /// <summary>
        /// 通知UI更新数据 - bool
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, bool propertyValue);

        /// <summary>
        /// 通知UI更新数据 - float
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, float propertyValue);

        /// <summary>
        /// 通知UI更新数据 - Sprite
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, Sprite propertyValue);

        /// <summary>
        /// 通知UI更新数据 - List&lt;int&gt;
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, List<int> propertyValue);

        /// <summary>
        /// 通知UI更新数据 - List&lt;string&gt;
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, List<string> propertyValue);

        /// <summary>
        /// 通知UI更新数据- List&lt;bool&gt;
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, List<bool> propertyValue);

        /// <summary>
        /// 通知UI更新数据- List&lt;float&gt;
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, List<float> propertyValue);

        /// <summary>
        /// 通知UI更新数据- List&lt;Sprite&gt;
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, List<Sprite> propertyValue);

        /// <summary>
        /// 通知UI更新数据 - List&lt;Enum&gt;
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, IList propertyValue);

        /// <summary>
        /// 通知所有UI对此模型的所有绑定属性进行更新
        /// </summary>
        public void NotifyAll();
    }
}
