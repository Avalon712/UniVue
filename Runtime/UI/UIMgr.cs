using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Common;
using UniVue.Timer;
using UniVue.Utils;
using Object = UnityEngine.Object;

namespace UniVue.UI
{
    public static class UIMgr
    {
        private static bool _initialized;
        private static readonly Dictionary<string, BaseView> _closedViews = new(16);
        private static readonly List<string> _disposeQueue = new(16);
        private static readonly Dictionary<string, Action<bool>> _loadingViews = new(16);
        private static readonly Dictionary<string, BaseView> _openedViews = new(16);

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
        /// <param name="lazyDisposeInterval">每隔指定秒数执行一次对被关闭的界面的资源释放(一次释放一个界面)</param>
        public static void Initialize(Type redPointKeyEnumType, IUIPrefabLoader loader, IUILayerMgr layerMgr = null,
                                      uint lazyDisposeInterval = 30)
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

            Initialize(loader, layerMgr, lazyDisposeInterval);
            RedPointMgr = new RedPointMgr(redPointKeyEnumType);
        }

        /// <summary>
        /// 初始化UIMgr
        /// </summary>
        /// <param name="loader">加载UI预制体</param>
        /// <param name="layerMgr">层级管理接口，如果为Null则使用默认的管理器</param>
        /// <param name="lazyDisposeInterval">每隔指定秒数执行一次对被关闭的界面的资源释放(一次释放一个界面)</param>
        public static void Initialize(IUIPrefabLoader loader, IUILayerMgr layerMgr, uint lazyDisposeInterval = 30)
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

            //隐藏层级

            Transform rootLayer = LayerMgr.Root.transform;
            LayerMgr.HideLayer.transform.SetParent(rootLayer.GetChild(rootLayer.childCount - 1));

            Object.DontDestroyOnLoad(LayerMgr.Root);

            //每隔一定时间释放一次关闭的界面
            float seconds = lazyDisposeInterval;
            TimerMgr.Create()
                    .OfCount(-1)
                    .OfInterval(seconds)
                    .OfExecuteCondition(() => _initialized && LayerMgr.HideLayer)
                    .OfCallback(() =>
                     {
                         if (_disposeQueue.Count > 0)
                         {
                             string viewName = _disposeQueue[0];
                             _disposeQueue.RemoveAt(0);
                             if (_closedViews.Remove(viewName, out BaseView view) && view != null)
                             {
                                 GameObject ui = view.UI;
                                 view.OnDisposeInternal();
                                 Object.Destroy(ui);
                             }
                         }
                     })
                    .Build();

            Renderer = new RGraphs();
        }


        public static T GetView<T>(string viewName) where T : BaseView
        {
            if (_openedViews.TryGetValue(viewName, out BaseView view) && view is T typedView) return typedView;
            return null;
        }

        public static bool IsOpen<T>() where T : BaseView
        {
            return _openedViews.ContainsKey(typeof(T).Name);
        }

        public static bool IsClosed<T>() where T : BaseView
        {
            return !IsOpen<T>();
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
            Type viewType = typeof(T);
            string viewName = viewType.Name;

            if (_loadingViews.TryGetValue(viewName, out Action<bool> callbacks))
            {
                if (callback != null)
                    _loadingViews[viewName] += callback;
                return;
            }

            if (_openedViews.ContainsKey(viewName))
            {
                callback?.Invoke(true);
                return;
            }

            if (_closedViews.Remove(viewName, out BaseView view) && view != null)
            {
                _openedViews.Add(viewName, view);
                _disposeQueue.Remove(viewName);

                view.UI.transform.SetParent(LayerMgr.GetLayerRoot(view.Layer).transform);
                view.OnOpenInternal(args);
                callback?.Invoke(true);
                return;
            }

            _openedViews[viewName] = null; //这一步是防止加载未完成时重复加载
            _loadingViews[viewName] = callback;
            Loader.LoadUIPrefabAsync(viewType, viewPrefab =>
            {
                //界面没有加载完成就关闭了界面
                if (_closedViews.Remove(viewName))
                {
                    if (_loadingViews.Remove(viewName, out Action<bool> callbacks))
                        callbacks?.Invoke(false);
                    return;
                }

                if (viewPrefab == null)
                {
                    if (_loadingViews.Remove(viewName, out Action<bool> callbacks))
                        callbacks?.Invoke(false);
                    _openedViews.Remove(viewName);
                    LogUtil.Exception(new Exception($"加载UI预制体失败，界面类型：{viewName}"));
                    return;
                }

                GameObject viewObj = GameObjectUtils.RectTransformClone(viewPrefab, LayerMgr.HideLayer.transform);
                BaseView newView = viewObj.GetComponent<T>();
                if (!newView)
                    newView = viewObj.AddComponent<T>();

                newView.transform.SetParent(LayerMgr.GetLayerRoot(newView.Layer).transform);
                GameObjectUtils.KeepTheSameWithPrefab(viewPrefab.transform as RectTransform,
                                                      viewObj.transform as RectTransform);

                _openedViews[viewName] = newView;
                viewObj.name = viewName;
                newView.OnCreateInternal(viewObj);
                newView.OnOpenInternal(args);
                if (_loadingViews.Remove(viewName, out callbacks)) callbacks?.Invoke(true);
            });
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

            if (_openedViews.ContainsKey(viewName))
            {
                callback?.Invoke(true);
                return;
            }

            if (_closedViews.Remove(viewName, out BaseView view) && view != null)
            {
                _openedViews.Add(viewName, view);
                _disposeQueue.Remove(viewName);

                view.UI.transform.SetParent(LayerMgr.GetLayerRoot(view.Layer).transform);
                view.OnOpenInternal(args);
                callback?.Invoke(true);
                return;
            }

            _openedViews[viewName] = null; //这一步是防止加载未完成时重复加载
            _loadingViews[viewName] = callback;
            Loader.LoadUIPrefabAsync(viewName, viewPrefab =>
            {
                //界面没有加载完成就关闭了界面
                if (_closedViews.Remove(viewName))
                {
                    if (_loadingViews.Remove(viewName, out Action<bool> callbacks))
                        callbacks?.Invoke(false);
                    return;
                }

                if (viewPrefab == null)
                {
                    if (_loadingViews.Remove(viewName, out Action<bool> callbacks))
                        callbacks?.Invoke(false);
                    _openedViews.Remove(viewName);
                    LogUtil.Exception(new Exception($"加载UI预制体失败，界面类型：{viewName}"));
                    return;
                }

                BaseView viewOnPrefab = viewPrefab.GetComponent<BaseView>();
                ExceptionUtils.ThrowIfNull(viewOnPrefab, "UI预制体身上没有挂载任何BaseView脚本，无法正常完成初始化流程");

                if (!viewOnPrefab)
                {
                    if (_loadingViews.Remove(viewName, out callbacks)) callbacks?.Invoke(false);
                    return;
                }

                GameObject viewObj = GameObjectUtils.RectTransformClone(viewPrefab, LayerMgr.HideLayer.transform);
                BaseView newView = viewObj.GetComponent<BaseView>();

                newView.transform.SetParent(LayerMgr.GetLayerRoot(newView.Layer).transform);
                GameObjectUtils.KeepTheSameWithPrefab(viewPrefab.transform as RectTransform,
                                                      viewObj.transform as RectTransform);

                _openedViews[viewName] = newView;
                viewObj.name = viewName;
                newView.OnCreateInternal(viewObj);
                newView.OnOpenInternal(args);
                if (_loadingViews.Remove(viewName, out callbacks)) callbacks?.Invoke(true);
            });
        }

        public static void Close(string viewName)
        {
            if (_openedViews.Remove(viewName, out BaseView view))
            {
                _closedViews[viewName] = view;
                if (view != null)
                {
                    _disposeQueue.Add(viewName);
                    view.OnCloseInternal();
                    view.UI.transform.SetParent(LayerMgr.HideLayer.transform);
                }
            }
        }

        /// <summary>
        /// 创建UI
        /// </summary>
        /// <param name="callback">创建完成后的回调，参数1-true-创建成功 false-创建失败  参数2-创建好的UI对象 </param>
        /// <typeparam name="T"></typeparam>
        public static void CreateUI<T>(Action<bool, T> callback) where T : BaseUI
        {
            Loader.LoadUIPrefabAsync(typeof(T), uiPrefab =>
            {
                if (!uiPrefab)
                {
                    callback?.Invoke(false, null);
                    return;
                }

                GameObject uiObj = GameObjectUtils.RectTransformClone(uiPrefab, null);
                if (!uiObj.TryGetComponent(out T ui))
                    ui = uiObj.AddComponent<T>();
                callback?.Invoke(true, ui);
            });
        }

        /// <summary>
        /// 创建UI
        /// <remarks>注意这种方式必须保证预制体身上有一个已经挂载的BaseUI脚本</remarks>
        /// </summary>
        /// <param name="uiName"></param>
        /// <param name="callback">创建完成后的回调，参数1-true-创建成功 false-创建失败  参数2-创建好的UI对象 </param>
        public static void CreateUI(string uiName, Action<bool, BaseUI> callback)
        {
            if (string.IsNullOrEmpty(uiName))
            {
                callback?.Invoke(false, null);
                return;
            }

            Loader.LoadUIPrefabAsync(uiName, uiPrefab =>
            {
                if (!uiPrefab || !uiPrefab.GetComponent<BaseUI>())
                {
                    callback?.Invoke(false, null);
                    return;
                }

                GameObject uiObj = GameObjectUtils.RectTransformClone(uiPrefab, null);
                callback?.Invoke(true, uiObj.GetComponent<BaseUI>());
            });
        }

        /// <summary>
        /// 销毁UI
        /// </summary>
        /// <param name="ui">要销毁的UI</param>
        public static void DestroyUI(BaseUI ui)
        {
            if (!ui) return;
            GameObject uiObj = ui.UI;
            ui.OnDisposeInternal();
            Object.Destroy(uiObj);
        }

        public static void Close<T>() where T : BaseView
        {
            Close(typeof(T).Name);
        }
    }
}