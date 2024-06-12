using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;
using UniVue.ViewModel.Models;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 一个UIBundel负责管理一个视图下某个模型的所有PropertyUI
    /// </summary>
    /// <remarks>
    /// 一个视图可能有多个UIBundle，其UIBundle的数量取决于视图绑定的模型数量（不同的）。
    /// 如果你想实现那些自定义的UI更新逻辑，你可以通过继承PropertyUI，然后实现你的UI更新逻辑，再手动
    /// 创建一个UIBundle对象去管理你的PropertyUI，之后将创建的UIBundle交给ViewUpdater进行管理，这样就能
    /// 实现数据模型的双向绑定，同时实现了你想要的UI更新效果。
    /// </remarks>
    public sealed class UIBundle : IModelNotifier
    {
        /// <summary>
        /// 当前UIBundle的状态
        /// </summary>
        public bool active { get; private set; } = true;

        /// <summary>
        /// UIBundle绑定的视图的名称
        /// </summary>
        public string ViewName { get; private set; }

        /// <summary>
        /// 绑定的模型名称
        /// </summary>
        public string ModelName { get; private set; }

        /// <summary>
        /// 模型更新通知器
        /// </summary>
        public IBindableModel Model { get; private set; }


        private List<PropertyUI> _properties;

        internal List<PropertyUI> ProertyUIs => _properties;

        public UIBundle(string modelName, string viewName, IBindableModel model, List<PropertyUI> properties)
        {
            ViewName = viewName;
            Model = model;
            ModelName = modelName;
            _properties = properties;
            //为每个属性UI设置模型通知器
            for (int i = 0; i < properties.Count; i++)
                properties[i].SetModelNotifier(this);
            _properties.TrimExcess();//清理内存
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, int propertyValue)
        {
            for (int i = 0; i < _properties.Count; i++)
            {
                if (propertyName.Equals(_properties[i].PropertyName))
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
            for (int i = 0; i < _properties.Count; i++)
            {
                if (propertyName.Equals(_properties[i].PropertyName))
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
            for (int i = 0; i < _properties.Count; i++)
            {
                if (propertyName.Equals(_properties[i].PropertyName))
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
            for (int i = 0; i < _properties.Count; i++)
            {
                if (propertyName.Equals(_properties[i].PropertyName))
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
            for (int i = 0; i < _properties.Count; i++)
            {
                if (propertyName.Equals(_properties[i].PropertyName))
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

            for (int i = 0; i < _properties.Count; i++)
            {
                if (propertyName.Equals(_properties[i].PropertyName))
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

            for (int i = 0; i < _properties.Count; i++)
            {
                if (propertyName.Equals(_properties[i].PropertyName))
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

            for (int i = 0; i < _properties.Count; i++)
            {
                if (propertyName.Equals(_properties[i].PropertyName))
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

            for (int i = 0; i < _properties.Count; i++)
            {
                if (propertyName.Equals(_properties[i].PropertyName))
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

            for (int i = 0; i < _properties.Count; i++)
            {
                if (propertyName.Equals(_properties[i].PropertyName))
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

            for (int i = 0; i < _properties.Count; i++)
            {
                if (propertyName.Equals(_properties[i].PropertyName))
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
            active = true;
        }


        public void Unbind()
        {
            active = false;
        }

        public void Destroy()
        {
            for (int i = 0; i < _properties.Count; i++)
            {
                _properties[i].Unbind();
            }
            _properties.Clear();
            _properties = null;
            Model = null;
        }

        public void NotifyModelUpdate(string propertyName, int propertyValue)
        {
            if (active)
                Model.UpdateModel(propertyName, propertyValue);
        }

        public void NotifyModelUpdate(string propertyName, string propertyValue)
        {
            if (active)
                Model.UpdateModel(propertyName, propertyValue);
        }

        public void NotifyModelUpdate(string propertyName, bool propertyValue)
        {
            if (active)
                Model.UpdateModel(propertyName, propertyValue);
        }

        public void NotifyModelUpdate(string propertyName, float propertyValue)
        {
            if (active)
                Model.UpdateModel(propertyName, propertyValue);
        }
    }
}
