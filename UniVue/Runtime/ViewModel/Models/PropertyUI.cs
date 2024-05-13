using UnityEngine;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    /// <summary>
    /// 模型的最小单元所对应的
    /// </summary>
    public abstract class PropertyUI :  IUIUpdater
    {
        /// <summary>
        /// 是否允许UI更新模型数据
        /// </summary>
        protected bool _allowUIUpdateModel;
        /// <summary>
        /// 该属性UI所属UI模块
        /// </summary>
        protected IModelNotifier _notifier;
        /// <summary>
        /// 属性名
        /// </summary>
        protected string _propertyName;
        /// <summary>
        /// 指示当前是否需要进行UI更新
        /// </summary>
        protected bool _needUpdate = true;

        public PropertyUI(IModelNotifier notifier,string propertyName,bool allowUIUpdateModel)
        {
            _propertyName = propertyName;
            _notifier = notifier;
            _allowUIUpdateModel = allowUIUpdateModel;
        }

        /// <summary>
        /// 显示或隐藏UI的展示
        /// </summary>
        /// <param name="active">true:显示</param>
        public abstract void SetActive(bool active);

        /// <summary>
        /// 解除UI与模型的关系 
        /// </summary>
        public abstract void Dispose();

        public string GetPropertyName()
        {
            return _propertyName;
        }

        public abstract void UpdateUI(int propertyValue);
        public abstract void UpdateUI(float propertyValue);
        public abstract void UpdateUI(string propertyValue);
        public abstract void UpdateUI(Sprite propertyValue);
        public abstract void UpdateUI(bool propertyValue);
    }

}
