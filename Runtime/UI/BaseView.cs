#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Common;
using UniVue.Internal;

#endregion

namespace UniVue.UI
{
    [DisallowMultipleComponent]
    public abstract class BaseView : BaseUI
    {
        private List<BaseUI> _childViews;
        private bool _closeInvoked = true;
        private bool _openInvoked;

        /// <summary>
        /// UI层级，层级高的界面会显示在层级低的前面
        /// </summary>
        public virtual int Layer { get; } = 0;

        /// <summary>
        /// 界面关闭后仍可存活的时间（秒）
        /// <remarks>仅对根视图有效(此值如果小于UIMgr初始化时设定的值则取当前值，否则取UIMgr初始化时设定的值)</remarks>
        /// </summary>
        public virtual int LiveTimeAfterClosed { get; } = 30;

        /// <summary>
        /// 父界面，如果为Null，则说明是根视图
        /// </summary>
        public BaseView ParentView
        {
            get
            {
                CheckDisposed();
                BaseUI parent = Parent;
                BaseView result = null;
                while (parent)
                {
                    if (parent is BaseView view)
                    {
                        result = view;
                        break;
                    }

                    parent = parent.Parent;
                }

                return result;
            }
        }

        public IndexableCollectionIterator<BaseView, BaseUI> ChildViews
        {
            get
            {
                CheckDisposed();
                if (_childViews == null)
                    _childViews = InternalObjectPool<List<BaseUI>>.Shared.Rent();
                _childViews.Clear();

                static void GetChildView(List<BaseUI> list, BaseUI parent)
                {
                    foreach (BaseUI child in parent.Children)
                        if (child is BaseView view)
                            list.Add(view);
                        else
                            GetChildView(list, child);
                }

                GetChildView(_childViews, this);
                return new IndexableCollectionIterator<BaseView, BaseUI>(_childViews);
            }
        }

        /// <summary>
        /// 当前View的所有组件
        /// </summary>
        public IndexableCollectionIterator<BaseComponent, BaseUI> Components => new(Children);

        /// <summary>
        /// 打开参数
        /// </summary>
        protected object[] Args { get; private set; }

        public void Open()
        {
            CheckDisposed();
            if (UIMgr.IsRootView(this))
                UIMgr.Open(Name);
            else
                OnOpenInternal(Array.Empty<object>());
        }

        public void Open(params object[] args)
        {
            CheckDisposed();
            if (UIMgr.IsRootView(this))
                UIMgr.Open(Name, null, args);
            else
                OnOpenInternal(args);
        }

        /// <summary>
        /// 关闭当前界面
        /// </summary>
        public void Close()
        {
            CheckDisposed();
            if (UIMgr.IsRootView(this))
                UIMgr.Close(Name);
            else
                OnCloseInternal();
        }

        internal void OnOpenInternal(object[] args)
        {
            CheckDisposed();
            if (!_openInvoked)
            {
                _openInvoked = true;
                _closeInvoked = false;
                Args = args;
                OnOpen();
            }
            else
            {
                OnReopen(args);
            }
        }

        internal void OnCloseInternal()
        {
            CheckDisposed();
            if (!_closeInvoked)
            {
                _closeInvoked = true;
                _openInvoked = false;
                OnClose();
            }
        }

        internal void OnReceiveMessageInternal(object sender, object message)
        {
            if (Disposed) return;
            OnReceiveMessage(sender, message);
        }

        #region 生命周期

        /// <summary>
        /// 界面被打开时回调
        /// </summary>
        protected virtual void OnOpen()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 界面已经被打开时又再此触发打开界面操作时调用
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnReopen(object[] args)
        {
        }

        /// <summary>
        /// 界面被关闭时回调
        /// </summary>
        protected virtual void OnClose()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 界面被销毁时调用
        /// </summary>
        /// <remarks>子类如果重写此方法需要调用父类的OnDispose()方法已释放父类占用的资源</remarks>
        protected override void OnDispose()
        {
            _childViews?.Clear();
            InternalObjectPool<List<BaseUI>>.Shared.Return(ref _childViews);
        }

        /// <summary>
        /// 接收到外部消息时回调
        /// <remarks>只有根视图才能接受到此回调</remarks>
        /// </summary>
        /// <param name="sender">消息发送中</param>
        /// <param name="message">外部传递的消息</param>
        protected virtual void OnReceiveMessage(object sender, object message)
        {
        }

        #endregion
    }
}