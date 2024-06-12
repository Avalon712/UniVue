using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;

namespace UniVue.ViewModel.Models
{
    /// <summary>
    /// 模型的最小单元所对应的
    /// </summary>
    public abstract class PropertyUI : IUIUpdater
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


        public PropertyUI(string propertyName, bool allowUIUpdateModel)
        {
            _propertyName = propertyName;
            _allowUIUpdateModel = allowUIUpdateModel;
        }

        /// <summary>
        /// 为当前属性UI设置模型通知器
        /// </summary>
        /// <param name="notifier">IModelNotifier接口实现</param>
        public void SetModelNotifier(IModelNotifier notifier) => _notifier = notifier;

        /// <summary>
        /// 显示或隐藏UI的展示
        /// </summary>
        /// <param name="active">true:显示</param>
        public abstract void SetActive(bool active);

        /// <summary>
        /// 解除UI与模型的关系 
        /// </summary>
        public abstract void Unbind();

        /// <summary>
        /// 所有属性绑定的所有UI
        /// </summary>
        /// <typeparam name="T">T : Component</typeparam>
        /// <returns>IEnumerable<T></returns>
        public abstract IEnumerable<T> GetUI<T>() where T : Component;

        /// <summary>
        /// 获取当前属性UI绑定的属性的属性名称
        /// </summary>
        /// <returns></returns>
        public string PropertyName => _propertyName;

        /// <summary>
        /// 当前UI是否是发布UI更新消息的发布者
        /// </summary>
        /// <returns>true:是</returns>
        protected bool IsPublisher() => Vue.Updater.Publisher == this;

        public abstract void UpdateUI(int propertyValue);
        public abstract void UpdateUI(float propertyValue);
        public abstract void UpdateUI(string propertyValue);
        public abstract void UpdateUI(Sprite propertyValue);
        public abstract void UpdateUI(bool propertyValue);
    }

}
