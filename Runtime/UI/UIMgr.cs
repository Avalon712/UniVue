using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniVue.Common;
using UniVue.Event;
using UniVue.Internal;
using UniVue.Timer;
using UniVue.Utils;
using Object = UnityEngine.Object;

namespace UniVue.UI
{
    public static class UIMgr
    {
        private readonly struct LazyDisposeInfo : IEquatable<LazyDisposeInfo>, IComparable<LazyDisposeInfo>
        {
            public readonly string viewName;
            public readonly float closedTimeTick;
            public readonly float liveTime;

            public LazyDisposeInfo(string viewName, float liveTime)
            {
                this.viewName = viewName;
                closedTimeTick = Time.realtimeSinceStartup;
                this.liveTime = liveTime;
            }
            
            public bool Equals(LazyDisposeInfo other)
            {
                return viewName == other.viewName;
            }

            public override bool Equals(object obj)
            {
                return obj is LazyDisposeInfo other && Equals(other);
            }

            public override int GetHashCode()
            {
                return viewName.GetHashCode();
            }

            public int CompareTo(LazyDisposeInfo other)
            {
                return closedTimeTick.CompareTo(other.closedTimeTick);
            }
            
            public static implicit operator LazyDisposeInfo(string viewName)
            {
                return new LazyDisposeInfo(viewName, 0);
            }
        }
        
        /// <summary>
        /// 根视图打开事件，事件参数-界面名称
        /// </summary>
        public const string EVENT_OPEN_VIEW = nameof(EVENT_OPEN_VIEW);

        /// <summary>
        /// 根视图关闭事件，事件参数-界面名称
        /// </summary>
        public const string EVENT_CLOSE_VIEW = nameof(EVENT_CLOSE_VIEW);

        /// <summary>
        /// 根视图销毁事件，事件参数-界面名称
        /// </summary>
        public const string EVENT_DISPOSE_VIEW = nameof(EVENT_DISPOSE_VIEW);

        private static bool _initialized;
        private static readonly Dictionary<string, BaseView> _closedViews = new(16);
        private static readonly List<LazyDisposeInfo> _disposeQueue = new(16);
        private static readonly Dictionary<string, Action<bool>> _loadingViews = new(16);
        private static readonly Dictionary<string, BaseView> _openedViews = new(16);
        
        private static uint _liveTimeAfterClosed = 30;
        private const float AllowDisposeErrorTime = 0.1f; //在0.1f秒的误差内都认为可以进行释放

        internal static IUIPrefabLoader Loader { get; private set; }

        private static IUILayerMgr LayerMgr { get; set; }

        public static RGraphs Renderer { get; private set; }

        public static RedPointMgr RedPointMgr { get; private set; }
        
        
        /// <summary>
        /// 初始化UIMgr(包括红点系统)
        /// </summary>
        /// <param name="redPointKeyEnumType">RedPointKey枚举类型</param>
        /// <param name="loader">加载UI预制体</param>
        /// <param name="layerMgr">层级管理接口，如果为Null则使用默认的管理器</param>
        /// <param name="liveTimeAfterClosed">当界面被关闭后仍可存活的时间（秒，仅对根界面生效，默认为30秒，小于0则立即销毁）</param>
        public static void Initialize(Type redPointKeyEnumType, IUIPrefabLoader loader, IUILayerMgr layerMgr = null,
                                      uint liveTimeAfterClosed = 30)
        {
            if (_initialized)
            {
                LogUtil.Warn("UIMgr已经初始化");
                return;
            }

            if (redPointKeyEnumType == null)
            {
                LogUtil.Exception(new Exception("UIMgr初始化失败，UIMgr初始化参数redPointKeyEnumType不能为null"));
                return;
            }

            Initialize(loader, layerMgr, liveTimeAfterClosed);
            RedPointMgr = new RedPointMgr(redPointKeyEnumType);
        }

        /// <summary>
        /// 初始化UIMgr
        /// </summary>
        /// <param name="loader">加载UI预制体</param>
        /// <param name="layerMgr">层级管理接口，如果为Null则使用默认的管理器</param>
        /// <param name="liveTimeAfterClosed">当界面被关闭后仍可存活的时间（秒，仅对根界面生效，默认为30秒）</param>
        public static void Initialize(IUIPrefabLoader loader, IUILayerMgr layerMgr, uint liveTimeAfterClosed = 30)
        {
            if (_initialized)
            {
                LogUtil.Warn("UIMgr已经初始化");
                return;
            }

            if (loader == null)
            {
                LogUtil.Exception(new Exception("UIMgr初始化参数UIPrefabLoader不能为null"));
                return;
            }

            _initialized = true;
            Loader = loader;
            LayerMgr = layerMgr;

            Object.DontDestroyOnLoad(LayerMgr.Root);

            _liveTimeAfterClosed = liveTimeAfterClosed;
            Renderer = new RGraphs();

            TimerMgr.Create().OfCallback(LazyDispose).OfCount(-1).OfInterval(1).Build();
        }

        private static void LazyDispose()
        {
            if(_disposeQueue.Count <= 0) return;
            float currentTime = Time.realtimeSinceStartup;
            int disposeEndIndex = -1;
            for (int i = 0; i < _disposeQueue.Count; i++)
            {
                LazyDisposeInfo lazy = _disposeQueue[i];
                if (Mathf.Abs(currentTime - lazy.closedTimeTick - lazy.liveTime) <= AllowDisposeErrorTime)
                {
                    if (_closedViews.Remove(lazy.viewName, out BaseView closedView) && closedView)
                    {
                        DisposeUI(closedView);
                        EventMgr.Dispatch(EVENT_DISPOSE_VIEW, lazy.viewName);
                    }
                    disposeEndIndex = i;
                }
                else
                    break; //_disposeQueue一定是按closedTimeTick升序排序的
            }
            if (disposeEndIndex >= 0)
                _disposeQueue.RemoveRange(0, disposeEndIndex + 1);
        }

        private static void DisposeUI(BaseUI ui)
        {
            Object.Destroy(ui.gameObject);
        }
        
        public static T GetView<T>(string viewName) where T : BaseView
        {
            if (_openedViews.TryGetValue(viewName, out BaseView view) && view is T typedView) return typedView;
            return null;
        }

        public static bool IsOpen<T>() where T : BaseView
        {
            return IsOpen(typeof(T).Name);
        }

        public static bool IsOpen(string viewName)
        {
            return _openedViews.ContainsKey(viewName);
        }

        public static bool IsClosed<T>() where T : BaseView
        {
            return !IsOpen<T>();
        }

        public static bool IsClosed(string viewName)
        {
            return !IsOpen(viewName);
        }

        /// <summary>
        /// 打开指定类型（界面名称为类型名称）
        /// </summary>
        /// <param name="callback">界面打开完成后回调，参数true-界面打开成功，false-界面打开失败</param>
        /// <typeparam name="T">界面类型（如果GameObject身上没有添加此组件则会自动添加此组件）</typeparam>
        public static void Open<T>(Action<bool> callback = null) where T : BaseView
        {
            Open<T>(callback, Array.Empty<object>());
        }

        /// <summary>
        /// 打开指定类型（界面名称为类型名称）
        /// </summary>
        /// <param name="callback">界面打开完成后回调，参数true-界面打开成功，false-界面打开失败</param>
        /// <param name="args">界面传递参数（如果界面在加载还没有完成时中又被执行了一次Open，此时此参数无效）</param>
        /// <typeparam name="T">界面类型（如果GameObject身上没有添加此组件则会自动添加此组件）</typeparam>
        public static void Open<T>(Action<bool> callback = null, params object[] args) where T : BaseView
        {
            Open(typeof(T).Name, callback, args);
        }

        /// <summary>
        /// 打开指定名称的界面
        /// <para>这种方式必须保证预制体身上已经挂载了BaseView的脚本</para>
        /// </summary>
        /// <param name="viewName">界面名称</param>
        /// <param name="callback">界面打开完成后回调，参数true-界面打开成功，false-界面打开失败</param>
        public static void Open(string viewName, Action<bool> callback = null)
        {
            Open(viewName, callback, Array.Empty<object>());
        }

        /// <summary>
        /// 打开指定名称的界面
        /// <para>这种方式必须保证预制体身上已经挂载了BaseView的脚本</para>
        /// </summary>
        /// <param name="viewName">界面名称</param>
        /// <param name="callback">界面打开完成后回调，参数true-界面打开成功，false-界面打开失败</param>
        /// <param name="args">界面传递参数（如果界面在加载还没有完成时中又被执行了一次Open，此时此参数无效）</param>
        public static void Open(string viewName, Action<bool> callback = null, params object[] args)
        {
            if (_loadingViews.TryGetValue(viewName, out Action<bool> callbacks))
            {
                if (callback != null)
                    _loadingViews[viewName] += callback;
                return;
            }

            if (_openedViews.TryGetValue(viewName, out BaseView openedView) && openedView)
            {
                openedView.OnOpenInternal(args);
                callback?.Invoke(true);
                return;
            }

            if (_closedViews.Remove(viewName, out BaseView closedView) && closedView)
            {
                _openedViews.Add(viewName, closedView);
                _disposeQueue.Remove(viewName);

                closedView.OnOpenInternal(args);
                callback?.Invoke(true);
                return;
            }

            _openedViews[viewName] = null; //这一步是防止加载未完成时重复加载
            _loadingViews[viewName] = callback;
            Loader.LoadUIObjectAsync(viewName, loadedUIObj =>
            {
                //界面没有加载完成就关闭了界面
                if (_closedViews.Remove(viewName))
                {
                    if (_loadingViews.Remove(viewName, out Action<bool> callbacks))
                        callbacks?.Invoke(false);
                    return;
                }

                if (!loadedUIObj)
                {
                    if (_loadingViews.Remove(viewName, out Action<bool> callbacks))
                        callbacks?.Invoke(false);
                    _openedViews.Remove(viewName);
                    LogUtil.Exception(new Exception($"加载UI对象失败，界面类型：{viewName}"));
                    return;
                }

                bool isPrefab = !loadedUIObj.scene.IsValid();
                GameObject viewObj = loadedUIObj;
                if (isPrefab)
                {
                    BaseView viewOnPrefab = loadedUIObj.GetComponent<BaseView>();
                    if (!viewOnPrefab)
                    { 
                        LogUtil.Exception(new Exception($"UI预制体身上没有挂载任何BaseView脚本，无法正常完成初始化流程，界面类型：{viewName}"));
                        if (_loadingViews.Remove(viewName, out callbacks)) callbacks?.Invoke(false);
                        return;
                    }
                    viewObj = CoreUtils.RectTransformClone(loadedUIObj, LayerMgr.GetLayerRoot(viewOnPrefab.Layer).transform);
                    CoreUtils.KeepTheSameWithPrefab(loadedUIObj.transform as RectTransform, viewObj.transform as RectTransform);
                }

                BaseView newView = viewObj.GetComponent<BaseView>();
                Transform layer = LayerMgr.GetLayerRoot(newView.Layer).transform;
                if (viewObj.transform.parent != layer)
                    viewObj.transform.SetParent(layer);
                
                _openedViews[viewName] = newView;
                viewObj.name = viewName;
                newView.OnCreateInternal();
                newView.OnOpenInternal(args);
                if (_loadingViews.Remove(viewName, out callbacks)) callbacks?.Invoke(true);
            });
        }

        public static void Close<T>() where T : BaseView
        {
            Close(typeof(T).Name);
        }

        public static void Close(string viewName)
        {
            if (_openedViews.Remove(viewName, out BaseView view) && view)
            {
                _closedViews[viewName] = view;
                float liveTimeAfterClose = Mathf.Min(_liveTimeAfterClosed, view.LiveTimeAfterClosed);
                if (liveTimeAfterClose > AllowDisposeErrorTime)
                    _disposeQueue.Add(new LazyDisposeInfo(viewName, liveTimeAfterClose));
                view.OnCloseInternal();
                EventMgr.Dispatch(EVENT_CLOSE_VIEW, viewName);
                if (liveTimeAfterClose <= AllowDisposeErrorTime)
                {
                    Object.Destroy(view.gameObject);
                    EventMgr.Dispatch(EVENT_DISPOSE_VIEW, viewName);
                }
            }
        }

        /// <summary>
        /// 创建UI
        /// </summary>
        /// <param name="uiName">UI预制体名称</param>
        /// <param name="callback">创建完成后的回调，参数1-true-创建成功 false-创建失败  参数2-创建好的UI对象 </param>
        /// <param name="parent">父物体</param>
        /// <typeparam name="T"></typeparam>
        public static void CreateUI<T>(string uiName, Action<bool, T> callback, GameObject parent = null) where T : BaseUI
        {
            Loader.LoadUIObjectAsync(uiName, loadedUIObj =>
            {
                if (!loadedUIObj)
                {
                    callback?.Invoke(false, null);
                    return;
                }
                bool isPrefab = !loadedUIObj.scene.IsValid();

                GameObject uiObj = loadedUIObj;
                
                if (isPrefab)
                    uiObj = CoreUtils.RectTransformClone(loadedUIObj, null);
                
                if (parent)
                    uiObj.transform.SetParent(parent.transform);

                if (isPrefab)
                    CoreUtils.KeepTheSameWithPrefab(loadedUIObj.transform as RectTransform, uiObj.transform as RectTransform);
                
                if (!uiObj.TryGetComponent(out T ui))
                    ui = uiObj.AddComponent<T>();
                
                ui.OnCreateInternal();
                callback?.Invoke(true, ui);
            });
        }

        /// <summary>
        /// 判断一个界面是否是根视图
        /// </summary>
        /// <param name="view"></param>
        /// <returns></returns>
        public static bool IsRootView(BaseView view)
        {
            return (_openedViews.TryGetValue(view.Name, out BaseView openedView) && openedView == view) ||
                   (_closedViews.TryGetValue(view.Name, out BaseView closedView) && closedView == view);
        }
        
        /// <summary>
        /// 创建UI
        /// </summary>
        /// <param name="callback">创建完成后的回调，参数1-true-创建成功 false-创建失败  参数2-创建好的UI对象 </param>
        /// <param name="parent">父物体</param>
        /// <typeparam name="T"></typeparam>
        public static void CreateUI<T>(Action<bool, T> callback, GameObject parent = null) where T : BaseUI
        {
            CreateUI(typeof(T).Name, callback, parent);
        }

        /// <summary>
        /// 销毁UI(无法通过此函数销毁View对象)
        /// </summary>
        /// <param name="ui">要销毁的UI(根界面无法通过此函数销毁)</param>
        public static void DestroyUI(BaseUI ui)
        {
            if (!ui || ui.Disposed || (ui is BaseView view && IsRootView(view))) return;
            DisposeUI(ui);
        }

        /// <summary>
        /// 向指定界面发送消息
        /// </summary>
        /// <param name="viewName">根视图名称</param>
        /// <param name="sender">发送者</param>
        /// <param name="message">消息</param>
        /// <param name="sendIfClosed">true-即使界面处于关闭状态仍然发送消息，默认为true</param>
        /// <returns>true-发送成功被正确接受到 false-失败</returns>
        public static bool SendMessage(string viewName, object sender, object message, bool sendIfClosed = true)
        {
            if (_openedViews.TryGetValue(viewName, out BaseView openedView) && openedView)
            {
                openedView.OnReceiveMessageInternal(sender, message);
                return true;
            }
            else if (sendIfClosed && _closedViews.TryGetValue(viewName, out BaseView closedView) && closedView)
            {
                closedView.OnReceiveMessageInternal(sender, message);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 向指定界面发送消息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="message">消息</param>
        /// <param name="sendIfClosed">true-即使界面处于关闭状态仍然发送消息，默认为true</param>
        /// <returns>true-发送成功被正确接受到 false-失败</returns>
        public static bool SendMessage<T>(object sender, object message, bool sendIfClosed = true) where T : BaseView
        {
            return SendMessage(typeof(T).Name, sender, message, sendIfClosed);
        }

        /// <summary>
        /// 向所有根界面广播消息
        /// </summary>
        /// <param name="sender">发送者</param>
        /// <param name="message">消息</param>
        /// <param name="includeClosedViews">是否包含被关闭的界面，默认为false</param>
        public static void BroadcastMessage(object sender, object message, bool includeClosedViews = false)
        {
            using InternalTempCollection<Dictionary<string, BaseView>, KeyValuePair<string, BaseView>> openedViews = new(_openedViews);
            foreach (var kv in openedViews)
            {
                if (kv.Value)
                    kv.Value.OnReceiveMessageInternal(sender, message);
            }

            if (!includeClosedViews) return;

            using InternalTempCollection<Dictionary<string, BaseView>, KeyValuePair<string, BaseView>> closedViews = new(_closedViews);
            foreach (var kv in closedViews)
            {
                if (kv.Value)
                    kv.Value.OnReceiveMessageInternal(sender, message);
            }
        }

        /// <summary>
        /// 从所有根视图节点开始，刷新所有的挂载有BaseUI的组件
        /// </summary>
        /// <param name="force">true-强制刷新</param>
        /// <param name="includeClosedView">true-包含已经关闭的界面（默认为false）</param>
        public static void RefreshAllUI(bool force = false, bool includeClosedView = false)
        {
            foreach (BaseView view in _openedViews.Values)
            {
                view.RefreshUIInternal(force);
            }
            if (!includeClosedView) return;
            foreach (BaseView closedView in _closedViews.Values)
            {
                closedView.RefreshUIInternal(force);
            }
        }
    }
}