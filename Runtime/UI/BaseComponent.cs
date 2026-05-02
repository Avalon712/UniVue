using UnityEngine;

namespace UniVue.UI
{
    [DisallowMultipleComponent]
    public abstract class BaseComponent : BaseUI
    {
        private string _name;

        /// <summary>
        /// 当前组件所属的界面
        /// </summary>
        public BaseView View { get; internal set; }

        /// <summary>
        /// 状态
        /// </summary>
        public bool Status { get; private set; }

        /// <summary>
        /// 组件名称（默认情况下GameObject.name）
        /// </summary>
        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(_name) && !Disposed) _name = UI.name;
                return _name;
            }
        }

        public void Show()
        {
            if (Status) return;
            CheckDisposedAndInitialized();
            Enable = true;
            UI.SetActive(true);
            Status = true;
            OnShow();
        }

        public void Hide()
        {
            if (!Status) return;
            CheckDisposedAndInitialized();
            Enable = false;
            UI.SetActive(false);
            Status = false;
            OnHide();
        }

        /// <summary>
        /// 设置组件的名称（默认情况下为GameObject的名称）
        /// </summary>
        /// <param name="componentName">组件名称</param>
        /// <param name="syncToGameObject">是否同步到GameObject的名称上</param>
        public void SetName(string componentName, bool syncToGameObject = false)
        {
            _name = componentName;
            if (syncToGameObject && !Disposed) UI.name = componentName;
        }

#region 子类重写

        /// <summary>
        /// 组件可见时回调
        /// </summary>
        protected virtual void OnShow() { }

        /// <summary>
        /// 组件隐藏时回调
        /// </summary>
        protected virtual void OnHide() { }

#endregion
    }
}