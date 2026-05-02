using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Common;
using UniVue.Internal;
using UniVue.Utils;

namespace UniVue.UI
{
    [DisallowMultipleComponent]
    public abstract class BaseView : BaseUI
    {
        /// <summary>
        /// OnCreate时记录所有最初子界面、组件的状态，等待下一次重新打开时恢复为创建时的状态
        /// </summary>
        private Dictionary<BaseUI, bool> _recordCreateStatus;

        private List<BaseUI> _viewUIs;

        /// <summary>
        /// UI层级，层级高的界面会显示在层级低的前面
        /// </summary>
        public abstract int Layer { get; }

        /// <summary>
        /// 父界面，如果为Null，则说明是根视图
        /// </summary>
        public BaseView Parent { get; private set; }

        /// <summary>
        /// 子界面的生命周期由父界面管理
        /// </summary>
        public IndexableCollectionIterator<BaseView, BaseUI> ChildViews => new(_viewUIs);

        /// <summary>
        /// 当前View的所有组件
        /// </summary>
        public IndexableCollectionIterator<BaseComponent, BaseUI> Components => new(_viewUIs);

        /// <summary>
        /// 界面名称
        /// </summary>
        public string ViewName { get; private set; }

        /// <summary>
        /// 如果是子view，则打开的参数是来自父界面
        /// </summary>
        protected object[] Args { get; private set; }

        /// <summary>
        /// true-打开状态  false-关闭状态
        /// </summary>
        public bool Status { get; private set; }

        /// <summary>
        /// 打开静态子界面，即在UI预制体中就已经包含了这个界面
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="callback">界面打开完成后回调</param>
        /// <param name="args"></param>
        /// <returns>true-打开成功 false-打开失败</returns>
        public bool OpenChildView(string viewName)
        {
            return OpenChildView(viewName, Array.Empty<object>());
        }

        /// <summary>
        /// 打开静态子界面，即在UI预制体中就已经包含了这个界面
        /// </summary>
        /// <param name="viewName"></param>
        /// <param name="args"></param>
        /// <returns>true-打开成功 false-打开失败</returns>
        public bool OpenChildView(string viewName, params object[] args)
        {
            CheckDisposedAndInitialized();
            if (Disposed)
            {
                LogUtil.Warn($"{ViewName}[{GetType().FullName}]已经被销毁!，无法打开子View{viewName}");
                return false;
            }

            foreach (BaseView childView in ChildViews)
            {
                if (childView.ViewName == viewName && !childView.Status)
                {
                    childView.OnOpenInternal(args);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 动态打开子界面，这个子界面是后面动态加载的（如果已经加载过这个类型的界面则不会再加载）
        /// </summary>
        /// <param name="mountNode">子界面的挂载点，如果为null，则挂载到UI节点</param>
        /// <param name="callback">界面打开完成后回调（参数1-true打开成功 false-打开失败）</param>
        /// <typeparam name="T">界面类型（GameObject身上没有时会自动挂载此脚本）</typeparam>
        public void OpenChildView<T>(Transform mountNode = null, Action<bool> callback = null)
            where T : BaseView
        {
            OpenChildView<T>(mountNode, callback, Array.Empty<object>());
        }

        /// <summary>
        /// 动态打开子界面，这个子界面是后面动态加载的（如果已经加载过这个类型的界面则不会再加载）
        /// </summary>
        /// <param name="mountNode">子界面的挂载点，如果为null，则挂载到UI节点</param>
        /// <param name="callback">界面打开完成后回调（参数1-true打开成功 false-打开失败）</param>
        /// <param name="args">界面传递参数</param>
        /// <typeparam name="T">界面类型（GameObject身上没有时会自动挂载此脚本）</typeparam>
        public void OpenChildView<T>(Transform mountNode = null, Action<bool> callback = null, params object[] args)
            where T : BaseView
        {
            CheckDisposedAndInitialized();
            if (Disposed)
            {
                callback?.Invoke(false);
                LogUtil.Warn($"{ViewName}[{GetType().FullName}]已经被销毁!，无法打开子View{typeof(T).FullName}");
                return;
            }

            foreach (BaseView childView in ChildViews)
            {
                if (childView.GetType() == typeof(T))
                {
                    childView.OnOpenInternal(args);
                    callback?.Invoke(true);
                    return;
                }
            }

            if (mountNode)
            {
                //判断挂载点是否属于当前View
                Transform viewNode = GameObjectUtils.UpFind(trans => trans.GetComponent<BaseView>(), mountNode);
                if (!viewNode)
                {
                    ExceptionUtils.ThrowIfTrue(true, "当前挂载节点不属于当前View层级的任何一个节点，无法正确挂载！");
                    callback?.Invoke(false);
                    return;
                }

                BaseView viewAdd = viewNode.GetComponent<BaseView>();
                if (viewAdd != this)
                {
                    LogUtil.Warn($"挂载点一个不属于当前View{ViewName}[{GetType().FullName}]，将调用挂载点所属的View{viewAdd.ViewName}[{viewAdd.GetType().FullName}]的OpenChildView<T>()方法实现");
                    viewAdd.OpenChildView<T>(mountNode, callback, args);
                    return;
                }
            }

            IUIPrefabLoader loader = UIMgr.Loader;
            loader.LoadUIPrefabAsync(typeof(T), viewPrefab =>
            {
                if (!viewPrefab)
                {
                    LogUtil.Exception(new Exception($"加载UI预制体失败，界面类型：{typeof(T).FullName}"));
                    return;
                }

                GameObject viewObj =
                    GameObjectUtils.RectTransformClone(viewPrefab, !mountNode ? UI.transform : mountNode);
                viewObj.name = viewPrefab.name;
                BaseView childView = viewObj.GetComponent<BaseView>();
                if (!childView)
                    childView = viewObj.AddComponent<T>();
                childView.Parent = this;
                childView.ViewName = viewObj.name;
                _viewUIs.Add(childView);
                childView.OnCreateInternal(viewObj);
                childView.OnOpenInternal(args);
                callback?.Invoke(true);
            });
        }

        public void CloseChildView(string viewName)
        {
            CheckDisposedAndInitialized();
            if (Disposed) return;
            foreach (BaseView childView in ChildViews)
            {
                if (childView.ViewName == viewName)
                {
                    childView.OnCloseInternal();
                    return;
                }
            }
        }

        /// <summary>
        /// 动态添加组件（按照组件类型的名称(typeof(T).Name)加载UI预制体）
        /// </summary>
        /// <param name="mountNode">挂载点，这个挂载点必须属性当前UI节点</param>
        /// <param name="callback">回调，参数1-是否添加成功 参数2-被添加的组件</param>
        /// <typeparam name="T">组件类型，如果GameObject身上没有挂载此组件则会自动添加此组件</typeparam>
        public void AddComponent<T>(Transform mountNode = null, Action<bool, T> callback = null) where T : BaseComponent
        {
            AddComponent(typeof(T).Name, mountNode, callback);
        }

        /// <summary>
        /// 动态添加组件（按照指定的组件的名称(uiName)加载UI预制体）
        /// </summary>
        /// <param name="uiName">加载的预制体的名称</param>
        /// <param name="mountNode">挂载点，这个挂载点必须属性当前UI节点</param>
        /// <param name="callback">回调，参数1-是否添加成功 参数2-被添加的组件</param>
        /// <typeparam name="T">组件类型，如果GameObject身上没有挂载此组件则会自动添加此组件</typeparam>
        public void AddComponent<T>(string uiName, Transform mountNode = null, Action<bool, T> callback = null)
            where T : BaseComponent
        {
            CheckDisposedAndInitialized();
            if (Disposed)
            {
                LogUtil.Warn($"{ViewName}[{GetType().FullName}]已经被销毁!，无法添加组件{typeof(T).FullName}");
                callback?.Invoke(false, null);
                return;
            }

            if (mountNode)
            {
                //判断挂载点是否属于当前View
                Transform viewNode = GameObjectUtils.UpFind(trans => trans.GetComponent<BaseView>(), mountNode);
                if (!viewNode)
                {
                    callback?.Invoke(false, null);
                    ExceptionUtils.ThrowIfTrue(true, "当前挂载节点不属于当前View层级的任何一个节点，无法正确挂载！");
                    return;
                }

                BaseView viewAdd = viewNode.GetComponent<BaseView>();
                if (viewAdd != this)
                {
                    LogUtil.Warn($"挂载点一个不属于当前View{ViewName}[{GetType().FullName}]，现在已经改为调用挂载点所属的View{viewAdd.ViewName}[{viewAdd.GetType().FullName}]的AddComponent<T>()方法实现");
                    viewAdd.AddComponent(mountNode, callback);
                    return;
                }
            }

            IUIPrefabLoader loader = UIMgr.Loader;
            loader.LoadUIPrefabAsync(uiName, uiPrefab =>
            {
                if (!uiPrefab)
                {
                    callback?.Invoke(false, null);
                    LogUtil.Exception(new Exception($"加载UI预制体失败，界面类型：{typeof(T).FullName}"));
                    return;
                }

                GameObject uiObj = GameObjectUtils.RectTransformClone(uiPrefab, !mountNode ? UI.transform : mountNode);
                T component = uiObj.GetComponent<T>();
                if (!component)
                    component = uiObj.AddComponent<T>();

                DoCreateInitialization(uiObj.transform, false);

                _viewUIs.Add(component);
                component.View = this;
                component.OnCreateInternal(uiObj);
                component.Show();
                callback?.Invoke(true, component);
            });
        }

        /// <summary>
        /// 显示指定名称的组件
        /// <para>如果有多个同名的组件则都会被打开</para>
        /// </summary>
        /// <param name="componentName">组件名称<see cref="BaseComponent.Name" /></param>
        public void ShowViewComponent(string componentName)
        {
            foreach (BaseComponent component in Components)
            {
                if (component.Name == componentName)
                    component.Show();
            }
        }

        /// <summary>
        /// 显示指定名称和类型组件
        /// <para>所有同名并且类型一致的组件都会被打开</para>
        /// </summary>
        /// <param name="componentName">组件名称<see cref="BaseComponent.Name" /></param>
        public void ShowViewComponent<T>(string componentName) where T : BaseComponent
        {
            foreach (BaseComponent component in Components)
            {
                if (component.Name == componentName && component is T)
                    component.Show();
            }
        }

        /// <summary>
        /// 显示指定类型的组件
        /// <para>如果有多个同类型的组件，则都会被打开</para>
        /// </summary>
        public void ShowViewComponent<T>() where T : BaseComponent
        {
            foreach (BaseComponent component in Components)
            {
                if (component is T)
                    component.Show();
            }
        }

        /// <summary>
        /// 隐藏指定名称的组件
        /// <para>如果有多个同名的组件则都会被隐藏</para>
        /// </summary>
        /// <param name="componentName">组件名称<see cref="BaseComponent.Name" /></param>
        public void HideViewComponent(string componentName)
        {
            foreach (BaseComponent component in Components)
            {
                if (component.Name == componentName)
                    component.Hide();
            }
        }

        /// <summary>
        /// 隐藏指定名称和类型的组件
        /// <para>所有同名并且类型一致的组件都会被隐藏</para>
        /// </summary>
        /// <param name="componentName">组件名称<see cref="BaseComponent.Name" /></param>
        public void HideViewComponent<T>(string componentName) where T : BaseComponent
        {
            foreach (BaseComponent component in Components)
            {
                if (component.Name == componentName && component is T)
                    component.Hide();
            }
        }

        /// <summary>
        /// 隐藏指定类型的组件
        /// <para>所有类型一致的组件都会被隐藏</para>
        /// </summary>
        public void HideViewComponent<T>() where T : BaseComponent
        {
            foreach (BaseComponent component in Components)
            {
                if (component is T)
                    component.Hide();
            }
        }

        /// <summary>
        /// 获取指定名称的组件
        /// </summary>
        /// <param name="componentName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetViewComponent<T>(string componentName) where T : BaseComponent
        {
            foreach (BaseComponent component in Components)
            {
                if (component.Name == componentName && component is T componentT)
                    return componentT;
            }

            return null;
        }

        /// <summary>
        /// 获取指定名称的组件
        /// </summary>
        /// <param name="componentName"></param>
        /// <param name="componentT"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetViewComponent<T>(string componentName, out T componentT) where T : BaseComponent
        {
            componentT = null;
            foreach (BaseComponent component in Components)
            {
                if (component.Name == componentName && component is T t)
                {
                    componentT = t;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取指定类型的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetViewComponent<T>() where T : BaseComponent
        {
            foreach (BaseComponent component in Components)
            {
                if (component is T componentT)
                    return componentT;
            }

            return null;
        }

        /// <summary>
        /// 获取指定类型的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryGetViewComponent<T>(out T componentT) where T : BaseComponent
        {
            componentT = null;

            foreach (BaseComponent component in Components)
            {
                if (component is T t)
                {
                    componentT = t;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取指定类型的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IndexableCollectionIterator<T, BaseUI> GetViewComponents<T>() where T : BaseComponent
        {
            return new IndexableCollectionIterator<T, BaseUI>(_viewUIs);
        }

        /// <summary>
        /// 关闭当前界面
        /// </summary>
        public void Close()
        {
            CheckDisposedAndInitialized();
            if (!Parent)
                UIMgr.Close(ViewName);
            else
                OnCloseInternal();
        }

#region 内部初始化、生命周期回调

        internal void OnOpenInternal(object[] args)
        {
            if (Status || Disposed) return;
            Enable = true;
            Args = args;
            Status = true;
            //恢复创建时的状态
            foreach (KeyValuePair<BaseUI, bool> kv in _recordCreateStatus)
            {
                BaseUI ui = kv.Key;
                bool active = kv.Value;
                if (!active) continue;
                if (ui is BaseView subView)
                    subView.OnOpenInternal(args);
                else if (ui is BaseComponent component)
                    component.Show();
            }

            OnOpen();
        }

        internal void OnCloseInternal()
        {
            if (!Status || Disposed) return;
            Enable = false;
            Status = false;
            KillAllCoroutines();
            KillAllTimers();
            //关闭所有子界面和组件
            foreach (BaseView childView in ChildViews) childView.OnCloseInternal();
            foreach (BaseComponent component in Components) component.Hide();
            OnClose();
        }

#endregion

#region 生命周期

        protected sealed override void OnCreate()
        {
            _viewUIs = InternalObjectPool<List<BaseUI>>.Shared.Rent();
            _recordCreateStatus = InternalObjectPool<Dictionary<BaseUI, bool>>.Shared.Rent();
            _recordCreateStatus.Clear();
            _viewUIs.Clear();

            ViewName = UI.name;

            //对所有的Component和ChildView执行初始化
            DoCreateInitialization(UI.transform, true);

            OnInit();
        }

        private void DoCreateInitialization(Transform parent, bool recordInitStatus)
        {
            foreach (Transform child in parent)
            {
                if (child.TryGetComponent(out BaseUI ui))
                {
                    if (recordInitStatus)
                        _recordCreateStatus[ui] = child.gameObject.activeSelf;

                    _viewUIs.Add(ui);

                    if (ui is BaseView subView)
                    {
                        subView.Parent = this;
                        subView.ViewName = child.name; //子界面的名称和GameObject的名称一致
                        subView.OnCreateInternal(child.gameObject);
                    }
                    else
                    {
                        if (ui is BaseComponent component)
                            component.View = this;
                        ui.OnCreateInternal(child.gameObject);
                        DoCreateInitialization(child, recordInitStatus); //嵌套的组件或其他继承自BaseUI也属于当前界面，组件之间不存在父子关系
                    }
                }
                else
                {
                    DoCreateInitialization(child, recordInitStatus);
                }
            }
        }

        protected sealed override void OnDispose()
        {
            foreach (BaseUI ui in _viewUIs)
            {
                ui.OnDisposeInternal();
                if (ui is BaseComponent component) component.View = null;
            }

            OnRelease();

            _recordCreateStatus.Clear();
            _viewUIs.Clear();
            InternalObjectPool<List<BaseUI>>.Shared.Return(ref _viewUIs);
            InternalObjectPool<Dictionary<BaseUI, bool>>.Shared.Return(ref _recordCreateStatus);
        }

#endregion

#region 暴露给子类的生命周期

        /// <summary>
        /// 初始化回调
        /// </summary>
        protected virtual void OnInit() { }

        /// <summary>
        /// 界面被打开时回调
        /// </summary>
        protected virtual void OnOpen()
        {
            UI.SetActive(true);
        }

        /// <summary>
        /// 界面被关闭时回调
        /// </summary>
        protected virtual void OnClose()
        {
            UI.SetActive(false);
        }

        /// <summary>
        /// 界面被销毁时回调
        /// </summary>
        protected virtual void OnRelease() { }

#endregion
    }
}