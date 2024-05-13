using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace UniVue.Model
{
    /// <summary>
    /// 基础模型数据
    /// </summary>
    public abstract class BaseModel : IBindableModel
    {
        public void Bind(string viewName, bool allowUIUpdateModel=true)
        {
            Vue.Router.GetView(viewName).BindModel(this, allowUIUpdateModel);
        }

        /// <summary>
        /// 通知所有UI进行更新
        /// </summary>
        public virtual void NotifyAll()
        {
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                PropertyInfo propertyInfo = propertyInfos[i];
                object value = propertyInfo.GetValue(this);
                if (propertyInfo.PropertyType == typeof(int) || propertyInfo.PropertyType.IsEnum)
                {
                    NotifyUIUpdate(propertyInfo.Name, Convert.ToInt32(value));
                }
                else if(propertyInfo.PropertyType == typeof(float))
                {
                    NotifyUIUpdate(propertyInfo.Name, Convert.ToSingle(value));
                }
                else if(propertyInfo.PropertyType == typeof(string))
                {
                    NotifyUIUpdate(propertyInfo.Name, value as string);
                }
                else if(propertyInfo.PropertyType == typeof(bool))
                {
                    NotifyUIUpdate(propertyInfo.Name, Convert.ToBoolean(value));
                }else if(propertyInfo.PropertyType == typeof(Sprite))
                {
                    NotifyUIUpdate(propertyInfo.Name, value as Sprite);
                }
            }
        }


        public void NotifyUIUpdate(string propertyName, int propertyValue)
        {
            Vue.Updater.UpdateUI(this, propertyName, propertyValue);
        }

        public void NotifyUIUpdate(string propertyName, string propertyValue)
        {
            Vue.Updater.UpdateUI(this, propertyName, propertyValue);
        }

        public void NotifyUIUpdate(string propertyName, bool propertyValue)
        {
            Vue.Updater.UpdateUI(this, propertyName, propertyValue);
        }

        public void NotifyUIUpdate(string propertyName, float propertyValue)
        {
            Vue.Updater.UpdateUI(this, propertyName, propertyValue);
        }

        public void NotifyUIUpdate(string propertyName, Sprite propertyValue) 
        {
            Vue.Updater.UpdateUI(this, propertyName, propertyValue);
        }

        public void NotifyUIUpdate(string propertyName, List<int> propertyValue)
        {
            Vue.Updater.UpdateUI(this, propertyName, propertyValue);
        }

        public void NotifyUIUpdate(string propertyName, List<string> propertyValue)
        {
            Vue.Updater.UpdateUI(this, propertyName, propertyValue);
        }

        public void NotifyUIUpdate(string propertyName, List<bool> propertyValue)
        {
            Vue.Updater.UpdateUI(this, propertyName, propertyValue);
        }

        public void NotifyUIUpdate(string propertyName, List<float> propertyValue)
        {
            Vue.Updater.UpdateUI(this, propertyName, propertyValue);
        }

        public void NotifyUIUpdate(string propertyName, List<Sprite> propertyValue)
        {
            Vue.Updater.UpdateUI(this, propertyName, propertyValue);
        }

        public void NotifyUIUpdate<T>(string propertyName, List<T> propertyValue) where T : Enum
        {
            Vue.Updater.UpdateUI(this, propertyName, propertyValue);
        }

        public void Unbind()
        {
            Vue.Updater.Unbind(this);
        }

        public virtual void UpdateModel(string propertyName, int propertyValue)
        {
            UpdateModel<int>(propertyName, propertyValue);
            NotifyUIUpdate(propertyName, propertyValue);
        }

        public virtual void UpdateModel(string propertyName, string propertyValue)
        {
            UpdateModel<string>(propertyName, propertyValue);
            NotifyUIUpdate(propertyName, propertyValue);
        }

        public virtual void UpdateModel(string propertyName, float propertyValue)
        {
            UpdateModel<float>(propertyName, propertyValue);
            NotifyUIUpdate(propertyName, propertyValue);
        }

        public virtual void UpdateModel(string propertyName, bool propertyValue)
        {
            UpdateModel<bool>(propertyName, propertyValue);
            NotifyUIUpdate(propertyName, propertyValue);
        }

        private void UpdateModel<T>(string propertyName,T propertyValue)
        {
            PropertyInfo[] propertyInfos = this.GetType().GetProperties();
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                if (propertyInfos[i].Name == propertyName)
                {
                    if (propertyInfos[i].PropertyType.IsEnum)
                    {
                        propertyInfos[i].SetValue(this, Enum.ToObject(propertyInfos[i].PropertyType,propertyValue));
                    }
                    else
                    {
                        propertyInfos[i].SetValue(this, propertyValue);
                    }
                    return;
                }
            }
        }
    }
}
