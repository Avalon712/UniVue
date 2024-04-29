using UnityEngine;
using UniVue.Model;
using UniVue.ViewModel.Models;

namespace UniVue.ViewModel
{
    public sealed class UIBundle : IModelNotifier 
    {
        /// <summary>
        /// 如果UIBundle绑定的是视图则是视图的名称，不是则是绑定的GameObject的name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 模型更新通知器
        /// </summary>
        public IBindableModel Model { get; private set; }


        private PropertyUI[] _properties;

        public UIBundle(string name,IBindableModel model) 
        {
            Name = name;
            Model = model;
        }

        internal void SetProperties(PropertyUI[] properties)
        {
            _properties = properties;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName,int propertyValue)
        {
            for (int i = 0; i < _properties.Length; i++)
            {
                if (propertyName.Equals(_properties[i].GetPropertyName()))
                {
                    _properties[i].UpdateUI(propertyName, propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, float propertyValue)
        {
            for (int i = 0; i < _properties.Length; i++)
            {
                if (propertyName.Equals(_properties[i].GetPropertyName()))
                {
                    _properties[i].UpdateUI(propertyName, propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, string propertyValue)
        {
            for (int i = 0; i < _properties.Length; i++)
            {
                if (propertyName.Equals(_properties[i].GetPropertyName()))
                {
                    _properties[i].UpdateUI(propertyName, propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, bool propertyValue)
        {
            for (int i = 0; i < _properties.Length; i++)
            {
                if (propertyName.Equals(_properties[i].GetPropertyName()))
                {
                    _properties[i].UpdateUI(propertyName, propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, Sprite propertyValue)
        {
            for (int i = 0; i < _properties.Length; i++)
            {
                if (propertyName.Equals(_properties[i].GetPropertyName()))
                {
                    _properties[i].UpdateUI(propertyName, propertyValue);
                }
            }
        }


        /// <summary>
        /// 重新绑定模型，能够重新绑定的条件是新绑定的模型与旧模型必须是同一类型但是不是同一对象
        /// </summary>
        public void Rebind<T>(T model) where T : IBindableModel
        {
            Model = model;
            model.NotifyAll();
        }

     

        public void Dispose() 
        {
            for (int i = 0; i < _properties.Length; i++)
            {
                _properties[i].Dispose();
                _properties[i] = null;
            }
            _properties = null;
            Model = null;
        }

        public void NotifyModelUpdate(string propertyName, int propertyValue)
        {
            Model.UpdateModel(propertyName, propertyValue);
        }

        public void NotifyModelUpdate(string propertyName, string propertyValue)
        {
            Model.UpdateModel(propertyName, propertyValue);
        }

        public void NotifyModelUpdate(string propertyName, bool propertyValue)
        {
            Model.UpdateModel(propertyName, propertyValue);
        }

        public void NotifyModelUpdate(string propertyName, float propertyValue)
        {
            Model.UpdateModel(propertyName, propertyValue);
        }
    }
}
