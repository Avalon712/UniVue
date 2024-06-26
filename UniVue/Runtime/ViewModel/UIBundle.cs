using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;
using UniVue.Utils;
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
        public bool active { get; private set; } = true;

        public IBindableModel Model { get; private set; }

        public List<PropertyUI> ProertyUIs { get; private set; }

        public UIBundle(IBindableModel model, List<PropertyUI> properties)
        {
            Model = model;
            ProertyUIs = properties;
            //为每个属性UI设置模型通知器
            for (int i = 0; i < properties.Count; i++)
                properties[i].SetModelNotifier(this);
            properties.TrimExcess();//清理内存
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, int propertyValue)
        {
            List<PropertyUI> properties = ProertyUIs;
            for (int i = 0; i < properties.Count; i++)
            {
                if (propertyName.Equals(properties[i].PropertyName))
                {
                    properties[i].UpdateUI(propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, float propertyValue)
        {
            List<PropertyUI> properties = ProertyUIs;
            for (int i = 0; i < properties.Count; i++)
            {
                if (propertyName.Equals(properties[i].PropertyName))
                {
                    properties[i].UpdateUI(propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, string propertyValue)
        {
            List<PropertyUI> properties = ProertyUIs;
            for (int i = 0; i < properties.Count; i++)
            {
                if (propertyName.Equals(properties[i].PropertyName))
                {
                    properties[i].UpdateUI(propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, bool propertyValue)
        {
            List<PropertyUI> properties = ProertyUIs;
            for (int i = 0; i < properties.Count; i++)
            {
                if (propertyName.Equals(properties[i].PropertyName))
                {
                    properties[i].UpdateUI(propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, Sprite propertyValue)
        {
            List<PropertyUI> properties = ProertyUIs;
            for (int i = 0; i < properties.Count; i++)
            {
                if (propertyName.Equals(properties[i].PropertyName))
                {
                    properties[i].UpdateUI(propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, List<int> propertyValue)
        {
            List<PropertyUI> properties = ProertyUIs;
            int k = 0;
            bool congfig = Vue.Config.WhenListLessUICountThenHideSurplus;

            for (int i = 0; i < properties.Count; i++)
            {
                if (propertyName.Equals(properties[i].PropertyName))
                {
                    if (k < propertyValue.Count)
                    {
                        properties[i].SetActive(true);
                        properties[i].UpdateUI(propertyValue[k++]);
                    }
                    else if (congfig)
                    {
                        properties[i].SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, List<float> propertyValue)
        {
            List<PropertyUI> properties = ProertyUIs;
            int k = 0;
            bool congfig = Vue.Config.WhenListLessUICountThenHideSurplus;

            for (int i = 0; i < properties.Count; i++)
            {
                if (propertyName.Equals(properties[i].PropertyName))
                {
                    if (k < propertyValue.Count)
                    {
                        properties[i].SetActive(true);
                        properties[i].UpdateUI(propertyValue[k++]);
                    }
                    else if (congfig)
                    {
                        properties[i].SetActive(false);
                    }
                }
            }
        }

        public void UpdateUI<T>(string propertyName, List<T> propertyValue) where T : Enum
        {
            List<PropertyUI> properties = ProertyUIs;
            int k = 0;
            bool congfig = Vue.Config.WhenListLessUICountThenHideSurplus;

            for (int i = 0; i < properties.Count; i++)
            {
                if (propertyName.Equals(properties[i].PropertyName))
                {
                    if (k < propertyValue.Count)
                    {
                        properties[i].SetActive(true);
                        properties[i].UpdateUI(Convert.ToInt32(propertyValue[k++]));
                    }
                    else if (congfig)
                    {
                        properties[i].SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, List<string> propertyValue)
        {
            List<PropertyUI> properties = ProertyUIs;
            int k = 0;
            bool congfig = Vue.Config.WhenListLessUICountThenHideSurplus;

            for (int i = 0; i < properties.Count; i++)
            {
                if (propertyName.Equals(properties[i].PropertyName))
                {
                    if (k < propertyValue.Count)
                    {
                        properties[i].SetActive(true);
                        properties[i].UpdateUI(propertyValue[k++]);
                    }
                    else if (congfig)
                    {
                        properties[i].SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, List<bool> propertyValue)
        {
            List<PropertyUI> properties = ProertyUIs;
            int k = 0;
            bool congfig = Vue.Config.WhenListLessUICountThenHideSurplus;

            for (int i = 0; i < properties.Count; i++)
            {
                if (propertyName.Equals(properties[i].PropertyName))
                {
                    if (k < propertyValue.Count)
                    {
                        properties[i].SetActive(true);
                        properties[i].UpdateUI(propertyValue[k++]);
                    }
                    else if (congfig)
                    {
                        properties[i].SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI(string propertyName, List<Sprite> propertyValue)
        {
            List<PropertyUI> properties = ProertyUIs;
            int k = 0;
            bool congfig = Vue.Config.WhenListLessUICountThenHideSurplus;

            for (int i = 0; i < properties.Count; i++)
            {
                if (propertyName.Equals(properties[i].PropertyName))
                {
                    if (k < propertyValue.Count)
                    {
                        properties[i].SetActive(true);
                        properties[i].UpdateUI(propertyValue[k++]);
                    }
                    else if (congfig)
                    {
                        properties[i].SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// 重新绑定模型，能够重新绑定的条件是新绑定的模型与旧模型必须是同一类型但是不是同一对象
        /// </summary>
        public void Rebind<T>(T model) where T : IBindableModel
        {
            if (model.GetType() == Model.GetType())
            {
                Model = model;
                active = true;
            }
#if UNITY_EDITOR
            else
                LogUtil.Warning("只要类型一致才允许进行重新绑定");
#endif
        }


        public void Unbind()
        {
            active = false;
        }

        public void Destroy()
        {
            List<PropertyUI> properties = ProertyUIs;
            for (int i = 0; i < properties.Count; i++)
            {
                properties[i].Unbind();
            }
            properties.Clear();
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
