using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniVue.Model
{
    public interface IUINotifier
    {
        /// <summary>
        /// 通知UI更新数据
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, int propertyValue);

        /// <summary>
        /// 通知UI更新数据
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, string propertyValue);

        /// <summary>
        /// 通知UI更新数据
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, bool propertyValue);

        /// <summary>
        /// 通知UI更新数据
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, float propertyValue);

        /// <summary>
        /// 通知UI更新数据
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, Sprite propertyValue);

        /// <summary>
        /// 通知UI更新数据
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, List<int> propertyValue);

        /// <summary>
        /// 通知UI更新数据
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, List<string> propertyValue);

        /// <summary>
        /// 通知UI更新数据
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, List<bool> propertyValue);

        /// <summary>
        /// 通知UI更新数据
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, List<float> propertyValue);

        /// <summary>
        /// 通知UI更新数据
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate(string propertyName, List<Sprite> propertyValue);

        /// <summary>
        /// 通知UI更新数据
        /// </summary>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        public void NotifyUIUpdate<T>(string propertyName, List<T> propertyValue) where T : Enum;
    }
}
