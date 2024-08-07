﻿using System;
using System.Collections.Generic;
using System.Reflection;
using UniVue.Utils;
using UniVue.ViewModel;

namespace UniVue.Model
{
    /// <summary>
    /// 可以对BindableType中的所有类型进行组合，以形成更加丰富、灵活的数据绑定
    /// </summary>
    /// <remarks>此模型支持List&lt;T&gt; where T : Enum类型的数据绑定</remarks>
    public sealed class GroupModel : IBindableModel
    {
        private string _modelName;
        private List<INotifiableProperty> _properties;

        public GroupModel(string modelName, int propertyCount)
        {
            _modelName = modelName == null ? "GroupModel" : modelName;
            _properties = new List<INotifiableProperty>(propertyCount);
        }

        /// <summary>
        /// 添加属性
        /// </summary>
        /// <typeparam name="T">属性类型（必须是BindableType枚举中定义的数据类型）</typeparam>
        /// <param name="property">实现了IAtomProperty<T>接口的类型</param>
        /// <returns>GroupModel</returns>
        public GroupModel AddProperty<T>(IAtomProperty<T> property)
        {
            _properties.Add(property);
            return this;
        }

        /// <summary>
        /// 移除属性
        /// </summary>
        /// <typeparam name="T">属性类型（即IAtomProperty&lt;T&gt;中T的类型）</typeparam>
        /// <param name="propertyName">属性名称</param>
        public void RemoveProperty<T>(string propertyName)
        {
            _properties.RemoveAll(p => p.GetPropertyName() == propertyName);
        }

        /// <summary>
        /// 获取属性值
        /// </summary>
        /// <typeparam name="T">属性类型（即IAtomProperty&lt;T&gt;中T的类型）</typeparam>
        /// <param name="propertyName">属性名称</param>
        public T GetPropertyValue<T>(string propertyName)
        {
            IAtomProperty<T> property = _properties.Find(p => p.GetPropertyName() == propertyName) as IAtomProperty<T>;
            CheckNull(propertyName, property);
            return property.Value;
        }

        /// <summary>
        /// 设置属性值
        /// </summary>
        /// <typeparam name="T">属性类型（即IAtomProperty&lt;T&gt;中T的类型）</typeparam>
        /// <param name="propertyName">属性名称</param>
        /// <param name="propertyValue">属性值</param>
        public void SetPropertyValue<T>(string propertyName, T propertyValue)
        {
            IAtomProperty<T> property = _properties.Find(p => p.GetPropertyName() == propertyName) as IAtomProperty<T>;
            CheckNull(propertyName, property);
            property.Value = propertyValue;
        }

        private void CheckNull<T>(string propertyName, IAtomProperty<T> property)
        {
            if (property == null)
                throw new Exception($"不存在属性名称为{propertyName}，类型为{typeof(T).Name}的属性");
        }

        public void Bind(string viewName, bool allowUIUpdateModel, string modelName = null, bool forceRebind = false)
        {
            Vue.Router.GetView(viewName).BindModel(this, allowUIUpdateModel, _modelName, forceRebind);
        }

        void IUINotifier.NotifyAll()
        {
            for (int i = 0; i < _properties.Count; i++)
            {
                _properties[i].NotifyUIUpdate();
            }
        }

        void IModelUpdater.UpdateModel(string propertyName, bool propertyValue)
        {
            SetPropertyValue(propertyName, propertyValue);
        }

        void IModelUpdater.UpdateModel(string propertyName, string propertyValue)
        {
            SetPropertyValue(propertyName, propertyValue);
        }

        void IModelUpdater.UpdateModel(string propertyName, float propertyValue)
        {
            SetPropertyValue(propertyName, propertyValue);
        }

        void IModelUpdater.UpdateModel(string propertyName, int propertyValue)
        {
            //如果是枚举类型，通过反射进行赋值
            INotifiableProperty property = _properties.Find(p => p.GetPropertyName() == propertyName);
            if (property != null && property.BindType == BindableType.Enum)
            {
                FieldInfo enumField = property.GetType().GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);
                enumField.SetValue(property, Enum.ToObject(enumField.FieldType, propertyValue));
                property.NotifyUIUpdate();
            }
            else
                SetPropertyValue(propertyName, propertyValue);
        }

        void IConsumableModel.UpdateAll(UIBundle bundle)
        {
            for (int i = 0; i < _properties.Count; i++)
            {
                ModelUtil.UpdateUI(_properties[i].GetPropertyName(), _properties[i].GetPropertyValue(), bundle);
            }
        }

        void IConsumableModel.UpdateUI(string propertyName, UIBundle bundle)
        {
            INotifiableProperty property = _properties.Find(p => p.GetPropertyName() == propertyName);

            if (property != null)
                ModelUtil.UpdateUI(property.GetPropertyName(), property.GetPropertyValue(), bundle);
        }
    }
}
