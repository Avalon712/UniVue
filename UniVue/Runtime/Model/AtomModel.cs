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
            var p = _property as BoolProperty;
            if (p != null)
                p.Value = propertyValue;
        }

        void IModelUpdater.UpdateModel(string propertyName, string propertyValue)
        {
            var p = _property as StringProperty;
            if (p != null)
                p.Value = propertyValue;
        }

        void IModelUpdater.UpdateModel(string propertyName, float propertyValue)
        {
            var p = _property as FloatProperty;
            if (p != null)
                p.Value = propertyValue;
        }

        void IModelUpdater.UpdateModel(string propertyName, int propertyValue)
        {
            var p = _property as IntProperty;
            if (p != null)
                p.Value = propertyValue;
        }
    }

}
