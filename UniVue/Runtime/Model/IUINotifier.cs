using System;
using System.Collections.Generic;
using System.Reflection;
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

        /// <summary>
        /// 通知所有UI对此模型的所有绑定属性进行更新
        /// 注意：基于反射的反射无法通知List&lt;T&gt; where T : Enum类型进行更新
        /// </summary>
        public void NotifyAll()
        {
            bool flag = false;
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                PropertyInfo propertyInfo = propertyInfos[i];
                object value = propertyInfo.GetValue(this);

                switch (propertyInfo.PropertyType.FullName)
                {
                    case "System.Single":
                        NotifyUIUpdate(propertyInfo.Name, Convert.ToSingle(value));
                        flag = true;
                        break;
                    case "System.Int32":
                        NotifyUIUpdate(propertyInfo.Name, Convert.ToInt32(value));
                        flag = true;
                        break;
                    case "System.String":
                        NotifyUIUpdate(propertyInfo.Name, value as string);
                        flag = true;
                        break;
                    case "System.Boolean":
                        NotifyUIUpdate(propertyInfo.Name, Convert.ToBoolean(value));
                        flag = true;
                        break;
                    case "UnityEngine.Sprite":
                        NotifyUIUpdate(propertyInfo.Name, value as Sprite);
                        flag = true;
                        break;
                }

                if (flag) { flag = false; continue; }

                if (propertyInfo.PropertyType.IsEnum)
                {
                    NotifyUIUpdate(propertyInfo.Name, Convert.ToInt32(value));
                }
                else if (propertyInfo.PropertyType == typeof(List<int>))
                {
                    NotifyUIUpdate(propertyInfo.Name, value as List<int>);
                }
                else if (propertyInfo.PropertyType == typeof(List<float>))
                {
                    NotifyUIUpdate(propertyInfo.Name, value as List<float>);
                }
                else if (propertyInfo.PropertyType == typeof(List<bool>))
                {
                    NotifyUIUpdate(propertyInfo.Name, value as List<bool>);
                }
                else if (propertyInfo.PropertyType == typeof(List<string>))
                {
                    NotifyUIUpdate(propertyInfo.Name, value as List<string>);
                }

            }
        }
    }
}
