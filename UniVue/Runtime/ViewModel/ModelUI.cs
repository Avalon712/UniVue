using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Internal;
using UniVue.Model;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 一个ModelUI负责管理一个视图下某个模型的所有PropertyUI
    /// </summary>
    public sealed class ModelUI
    {
        public bool active { get; private set; } = true;

        public string ModelName { get; private set; }

        public IBindableModel Model { get; private set; }

        public List<PropertyUI> ProertyUIs { get; private set; }

        public ModelUI(string modelName, IBindableModel model, List<PropertyUI> properties)
        {
            Model = model;
            ModelName = modelName;
            ProertyUIs = properties;
            //为每个属性UI设置模型通知器
            for (int i = 0; i < properties.Count; i++)
                properties[i].Bundle = this;

            model.UpdateAll(this);
        }


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


        public void UpdateUI(string propertyName, List<int> propertyValue)
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


        public void UpdateUI(string propertyName, List<float> propertyValue)
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
        /// 更新List&lt;Enum&gt;属性类型的UI
        /// </summary>
        /// <typeparam name="T">这个泛型必须是枚举类型</typeparam>
        public void UpdateUI(string propertyName, IList propertyValue)
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


        public void UpdateUI(string propertyName, List<string> propertyValue)
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


        public void UpdateUI(string propertyName, List<bool> propertyValue)
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


        public void UpdateUI(string propertyName, List<Sprite> propertyValue)
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
        /// 重新绑定模型，能够重新绑定的条件是新绑定的模型与旧模型必须是同一类型但是不是同一对象
        /// </summary>
        public bool TryRebind(string modelName, IBindableModel model)
        {
            if (!active || (model.TypeInfo.typeFullName == Model.TypeInfo.typeFullName && modelName == ModelName))
            {
                Model = model;
                model.UpdateAll(this);
                active = true;
                return true;
            }
            return false;
        }


        public void Unbind()
        {
            active = false;
        }

        public void Destroy()
        {
            ProertyUIs.Clear();
            CachePool.AddCache(InternalType.List_PropertyUI, ProertyUIs, false);
            Model = null;
        }

        public void UpdateModel(string propertyName, int propertyValue)
        {
            if (active)
                Model.UpdateModel(propertyName, propertyValue);
        }

        public void UpdateModel(string propertyName, string propertyValue)
        {
            if (active)
                Model.UpdateModel(propertyName, propertyValue);
        }

        public void UpdateModel(string propertyName, bool propertyValue)
        {
            if (active)
                Model.UpdateModel(propertyName, propertyValue);
        }

        public void UpdateModel(string propertyName, float propertyValue)
        {
            if (active)
                Model.UpdateModel(propertyName, propertyValue);
        }
    }
}
