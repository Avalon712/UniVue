using System;
using System.Collections.Generic;
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

        public void SetProperties(PropertyUI[] properties)
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
                    _properties[i].UpdateUI(propertyValue);
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
                    _properties[i].UpdateUI(propertyValue);
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
                    _properties[i].UpdateUI(propertyValue);
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
                    _properties[i].UpdateUI(propertyValue);
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
                    _properties[i].UpdateUI(propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, List<int> propertyValue)
        {
            int k = 0; 
            bool congfig = Vue.Config.WhenListLessThanUIThenHide;
            
            for (int i = 0; i < _properties.Length; i++)
            {
                if (propertyName.Equals(_properties[i].GetPropertyName()))
                {
                    if (k < propertyValue.Count)
                    {
                        _properties[i].SetActive(true);
                        _properties[i].UpdateUI(propertyValue[k++]);
                    }
                    else if (congfig)
                    {
                        _properties[i].SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, List<float> propertyValue)
        {
            int k = 0;
            bool congfig = Vue.Config.WhenListLessThanUIThenHide;

            for (int i = 0; i < _properties.Length; i++)
            {
                if (propertyName.Equals(_properties[i].GetPropertyName()))
                {
                    if (k < propertyValue.Count)
                    {
                        _properties[i].SetActive(true);
                        _properties[i].UpdateUI(propertyValue[k++]); 
                    }
                    else if (congfig)
                    {
                        _properties[i].SetActive(false);
                    }
                }
            }
        }

        public void UpdateUI<T>(string propertyName, List<T> propertyValue) where T : Enum
        {
            int k = 0;
            bool congfig = Vue.Config.WhenListLessThanUIThenHide;

            for (int i = 0; i < _properties.Length; i++)
            {
                if (propertyName.Equals(_properties[i].GetPropertyName()))
                {
                    if (k < propertyValue.Count)
                    {
                        _properties[i].SetActive(true);
                        _properties[i].UpdateUI(Convert.ToInt32(propertyValue[k++]));
                    }
                    else if (congfig)
                    {
                        _properties[i].SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, List<string> propertyValue)
        {
            int k = 0;
            bool congfig = Vue.Config.WhenListLessThanUIThenHide;

            for (int i = 0; i < _properties.Length; i++)
            {
                if (propertyName.Equals(_properties[i].GetPropertyName()))
                {
                    if (k < propertyValue.Count)
                    {
                        _properties[i].SetActive(true);
                        _properties[i].UpdateUI(propertyValue[k++]);
                    }
                    else if (congfig)
                    {
                        _properties[i].SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, List<bool> propertyValue)
        {
            int k = 0;
            bool congfig = Vue.Config.WhenListLessThanUIThenHide;

            for (int i = 0; i < _properties.Length; i++)
            {
                if (propertyName.Equals(_properties[i].GetPropertyName()))
                {
                    if (k < propertyValue.Count)
                    {
                        _properties[i].SetActive(true);
                        _properties[i].UpdateUI(propertyValue[k++]);
                    }
                    else if (congfig)
                    {
                        _properties[i].SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, List<Sprite> propertyValue)
        {
            int k = 0;
            bool congfig = Vue.Config.WhenListLessThanUIThenHide;

            for (int i = 0; i < _properties.Length; i++)
            {
                if (propertyName.Equals(_properties[i].GetPropertyName()))
                {
                    if (k < propertyValue.Count)
                    {
                        _properties[i].SetActive(true);
                        _properties[i].UpdateUI(propertyValue[k++]);
                    }
                    else if (congfig)
                    {
                        _properties[i].SetActive(false);
                    }
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
