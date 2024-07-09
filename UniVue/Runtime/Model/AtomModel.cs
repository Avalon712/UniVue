using System;
using System.Reflection;
using UniVue.Utils;
using UniVue.ViewModel;

namespace UniVue.Model
{
    /// <summary>
    /// UniVue可支持的最小模型单元（这个类不会进行反射操作，没有装箱消耗）
    /// </summary>
    /// <remarks>
    /// 使用此类可以实现更新细腻度的数据绑定。
    /// 即通过对int、float、bool、string、枚举、Sprite、List&lt;int&gt;、
    /// List&lt;bool&gt;、List&lt;float&gt;、List&lt;string&gt;、List&lt;Sprite&gt;、
    /// List&lt;枚举&gt;这种最小支持绑定单元指定一个模型名和属性名，
    /// 使得成为一个能够进行数据绑定的虚拟模型。
    /// 如果你还想支持其它类型请自行实现IAtomProperty&lt;T&gt;接口。
    /// </remarks>
    public sealed class AtomModel<T> : IBindableModel
    {
        private string _modelName;
        private IAtomProperty<T> _property;

        public string PropertyName => _property.PropertyName;

        /// <summary>
        /// 绑定的值
        /// </summary>
        public T Value
        {
            get => _property.Value;
            set => _property.Value = value;
        }

        /// <summary>
        /// 最小可绑定原子数据模型
        /// </summary>
        /// <remarks>如果要创建Lis</remarks>
        /// <param name="modelName">模型名称，如果为null则将默认为当前类的类型名称</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="wrapper">属性包装器</param>
        internal AtomModel(string modelName)
        {
            _modelName = string.IsNullOrEmpty(modelName) ? "AtomModel" : modelName;
        }

        internal void SetAtomProperty(IAtomProperty<T> property)
        {
            _property = property;
        }

        public void Bind(string viewName, bool allowUIUpdateModel, string modelName = null, bool forceRebind = false)
        {
            Vue.Router.GetView(viewName).BindModel(this, allowUIUpdateModel, _modelName, forceRebind);
        }

        void IUINotifier.NotifyAll()
        {
            _property.NotifyUIUpdate();
        }

        void IModelUpdater.UpdateModel(string propertyName, bool propertyValue)
        {
            if (_property.BindType == BindableType.Bool)
            {
                BoolProperty p = _property as BoolProperty;
                p.Value = propertyValue;
            }
        }

        void IModelUpdater.UpdateModel(string propertyName, string propertyValue)
        {
            if (_property.BindType == BindableType.String)
            {
                StringProperty p = _property as StringProperty;
                p.Value = propertyValue;
            }
        }

        void IModelUpdater.UpdateModel(string propertyName, float propertyValue)
        {
            if (_property.BindType == BindableType.Float)
            {
                FloatProperty p = _property as FloatProperty;
                p.Value = propertyValue;
            }
        }

        void IModelUpdater.UpdateModel(string propertyName, int propertyValue)
        {
            if (_property.BindType == BindableType.Enum)
            {
                FieldInfo enumField = _property.GetType().GetField("_value", BindingFlags.Instance | BindingFlags.NonPublic);
                enumField.SetValue(_property, Enum.ToObject(enumField.FieldType, propertyValue));
                _property.NotifyUIUpdate();
            }
            else if (_property.BindType == BindableType.Int)
            {
                IntProperty p = _property as IntProperty;
                p.Value = propertyValue;
            }
        }

        void IConsumableModel.UpdateUI(string propertyName, UIBundle bundle)
        {
            if (_property.PropertyName.Equals(propertyName))
            {
                ModelUtil.UpdateUI(propertyName, Value, bundle);
            }
        }

        void IConsumableModel.UpdateAll(UIBundle bundle)
        {
            ModelUtil.UpdateUI(_property.PropertyName, Value, bundle);
        }
    }

}
