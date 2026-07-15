using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UniVue.Common;
using VC;
using UniVue.Event;
using UniVue.Internal;
using UniVue.Model;
using UniVue.Timer;
using UniVue.Utils;

namespace UniVue.UI
{
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseUI : UIModel
    {
        private RGraph _graph;
        private float _timer;
        private List<ulong> _coroutines;
        private List<ulong> _timers;
        private List<BaseUI> _children;
        
        //保证OnCreate一定最先被执行
        private bool _creationInvoked;
        private bool _needInvokeOnActivateAfterOnCreate; 
        private bool _needInvokeOnInactiveAfterOnCreate;

        public bool Disposed { get; private set; }

        /// <summary>
        /// UI对象
        /// </summary>
        public GameObject UI { get; private set; }

        public RectTransform Layout { get; private set; }

        /// <summary>
        /// UI名称
        /// <remarks>此名称为创建时的名称，且不会再变</remarks>
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 所有子UI
        /// <remarks>每次调用都是重新获取不会缓存结果</remarks>
        /// </summary>
        public IReadOnlyList<BaseUI> Children
        {
            get
            {
                CheckDisposed();
                if (_children == null)
                    _children = InternalObjectPool<List<BaseUI>>.Shared.Rent();
                _children.Clear();
                foreach (Transform child in transform)
                {
                    if (child.TryGetComponent(out BaseUI ui))
                    {
                        _children.Add(ui);
                    }
                }
                return _children;
            }
        }

        /// <summary>
        /// 父UI
        /// <remarks>每次调用都是重新获取不会缓存结果</remarks>
        /// </summary>
        public BaseUI Parent
        {
            get
            {
                CheckDisposed();
                Transform parent = transform.parent;
                BaseUI parentUI = null;
                while (parent)
                {
                    if (parent.TryGetComponent(out parentUI))
                        break;
                    parent = parent.parent;
                }
                return parentUI;
            }
        }

        internal void OnCreateInternal()
        {
            static void TraverseChildren(Transform transform)
            {
                foreach (Transform child in transform)
                {
                    if (child.TryGetComponent(out BaseUI ui))
                        ui.OnCreateInternal();
                    else
                        TraverseChildren(child);
                }
            }
            
            if (Disposed) return;
            
            TraverseChildren(transform);
            Name = gameObject.name;
            UI = gameObject;
            Layout = UI.GetComponent<RectTransform>();
            
            OnCreate();
            _creationInvoked = true;
            if (_needInvokeOnActivateAfterOnCreate)
                SetUIActive(true);
            else if (_needInvokeOnInactiveAfterOnCreate)
                SetUIActive(false);
        }

        private void SetUIActive(bool active)
        {
            UIMgr.Renderer.SetEnable(_graph, active);
            if (active)
            {
                OnActive();
                RefreshUI();
            }
            else
            {
                OnInactive();
            }
        }
        
        #region Unity回调
        
        private void OnEnable()
        {
            //OnEnable调用情况：不管何种情况gameObject.activeSelf=true && enabled=true都是前提
            //在Awake之后自然调用
            //其祖上节点处于失活状态，当其离开失活区域时
            //祖上节点从失活状态变为激活状态

            if (!_creationInvoked)
            {
                _needInvokeOnActivateAfterOnCreate = true;
                _needInvokeOnInactiveAfterOnCreate = false;
                return;
            }
            SetUIActive(true);
        }

        private void OnDisable()
        {
            //OnDisable调用情况：
            //gameObject.activeSelf = false
            //enabled = false
            //从激活状态进入失活区域
            
            if (!_creationInvoked)
            {
                _needInvokeOnActivateAfterOnCreate = false;
                _needInvokeOnInactiveAfterOnCreate = true;
                return;
            }
            SetUIActive(false);
        }

        private void OnDestroy()
        {
            OnDispose();
            UIMgr.Renderer.Remove(ref _graph);
            KillAllCoroutines();
            KillAllTimers();
            EventMgr.Off(this);
            _children?.Clear();
            InternalObjectPool<List<ulong>>.Shared.Return(ref _coroutines);
            InternalObjectPool<List<ulong>>.Shared.Return(ref _timers);
            InternalObjectPool<List<BaseUI>>.Shared.Return(ref _children);
            Disposed = true;
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void CheckDisposed()
        {
            ExceptionUtils.ThrowIfTrue(Disposed, Disposed ? $"{GetType().Name} is disposed." : string.Empty);
        }

        private bool TryAllocateCollections(ref List<ulong> collection)
        {
            if (collection == null)
            {
                collection = InternalObjectPool<List<ulong>>.Shared.Rent();
                return true;
            }
            return false;
        }

        #region 辅助函数

        /// <summary>
        /// 当前UI是否可以被看见
        /// </summary>
        /// <returns></returns>
        protected bool IsVisible()
        {
            bool visible = false;
            if (gameObject.activeSelf)
            {
                Transform parent = transform.parent;
                while (parent)
                {
                    if(parent.gameObject.activeSelf)
                        parent = parent.parent;
                    else
                        break;
                }
                visible = !parent;
            }
            return visible;
        }
        
        /// <summary>
        /// 刷新UI（绑定的所有渲染函数都会被执行一次，当渲染状态从Disable变为Enable时会自动调用一次此函数）
        /// <remarks>绝大多数情况下你都无需手动调用此函数。此函数不是精准式的更新，只要有脏标记则会将此UI绑定的所有渲染函数都执行一遍</remarks>
        /// </summary>
        /// <param name="force">是否强制刷新，即不管有无脏标记都重新渲染一遍（默认情况只有在当前UI的渲染状态处于Disable期间有渲染状态的变化时才会真正调用）</param>
        protected void RefreshUI(bool force = false)
        {
            if (_graph.g == null) return;
            CheckDisposed();
            UIMgr.Renderer.RenderIfDirtyOrForce(_graph, force, true);
        }

        /// <summary>
        /// 获取当前UI上绑定的所有Model
        /// </summary>
        /// <returns></returns>
        public IEnumerable<BaseModel> GetModels()
        {
            if (_graph.g == null) yield break;
            foreach (RKey key in _graph.g.next.Keys)
            {
                BaseModel model = key.As<BaseModel>();
                if (model != null) yield return model;
            }
        }

        /// <summary>
        /// 获取当前UI上绑定的指定类型的Model，如果有多个满足条件的Model，则返回第一个找到的Model，如果没有满足条件的Model，则返回Null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetModel<T>() where T : BaseModel
        {
            if (_graph.g == null) return null;
            CheckDisposed();
            foreach (BaseModel model in GetModels())
            {
                if (model is T modelT)
                    return modelT;
            }

            return null;
        }

        /// <summary>
        /// 判断当前UI是否存在绑定的指定模型
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool IsBind(BaseModel model)
        {
            if (_graph.g == null) return false;
            CheckDisposed();
            return _graph.g.next.ContainsKey(model);
        }

        /// <summary>
        /// 替换之前绑定的Model
        /// </summary>
        /// <param name="oldModel">之前绑定的旧数据</param>
        /// <param name="newModel">未绑定过的新数据</param>
        /// <typeparam name="T"></typeparam>
        protected void Rebind<T>(T oldModel, T newModel) where T : BaseModel
        {
            if (_graph.g == null || oldModel == newModel) return;
            CheckDisposed();
            ExceptionUtils.ThrowIfArgNull(oldModel, nameof(oldModel));
            ExceptionUtils.ThrowIfArgNull(newModel, nameof(newModel));
            ExceptionUtils.ThrowIfFalse(IsBind(oldModel), "当前UI不存在绑定的模型(oldModel)");
            ExceptionUtils.ThrowIfTrue(IsBind(newModel), "当前UI已经绑定了模型(newModel)");
            UIMgr.Renderer.Rebind(_graph, oldModel, newModel, enabled && gameObject.activeSelf);
            UIMgr.Renderer.TrySetDirty(_graph);
        }

        /// <summary>
        /// 绑定渲染函数
        /// </summary>
        /// <param name="model">绑定的模型</param>
        /// <param name="renderFunc">渲染函数</param>
        /// <returns>绑定的渲染函数</returns>
        protected Action Bind(BaseModel model, Action renderFunc)
        {
            CheckDisposed();
            ExceptionUtils.ThrowIfArgNull(model, nameof(model));
            ExceptionUtils.ThrowIfArgNull(renderFunc, nameof(renderFunc));
            UIMgr.Renderer.AddNode(ref _graph, model, renderFunc);
            return renderFunc;
        }

        /// <summary>
        /// 绑定渲染函数
        /// </summary>
        /// <param name="model">绑定的模型</param>
        /// <param name="renderFunc">渲染函数</param>
        /// <param name="propertyNames">指定Model上要监听的属性</param>
        /// <returns>绑定的渲染函数</returns>
        [InternalParamsGCOptimization]
        protected Action Bind(BaseModel model, Action renderFunc, params string[] propertyNames)
        {
            CheckDisposed();
            ExceptionUtils.ThrowIfArgNull(model, nameof(model));
            ExceptionUtils.ThrowIfArgNull(renderFunc, nameof(renderFunc));
            if (propertyNames == null || propertyNames.Length <= 0)
                UIMgr.Renderer.AddNode(ref _graph, model, renderFunc);
            else
                UIMgr.Renderer.AddNode(ref _graph, model, renderFunc, propertyNames);
            return renderFunc;
        }

        /// <summary>
        /// 绑定渲染函数
        /// </summary>
        /// <param name="model">绑定的模型</param>
        /// <param name="renderFunc">渲染函数</param>
        /// <param name="propertyNames">指定Model上要监听的属性</param>
        /// <returns>绑定的渲染函数</returns>
        protected Action Bind(BaseModel model, Action renderFunc, in Params<string> propertyNames)
        {
            CheckDisposed();
            ExceptionUtils.ThrowIfArgNull(model, nameof(model));
            ExceptionUtils.ThrowIfArgNull(renderFunc, nameof(renderFunc));
            if (propertyNames.Length <= 0)
                UIMgr.Renderer.AddNode(ref _graph, model, renderFunc);
            else
                UIMgr.Renderer.AddNode(ref _graph, model, renderFunc, propertyNames);
            return renderFunc;
        }

        /// <summary>
        /// 绑定指定事件触发的渲染函数
        /// </summary>
        /// <param name="eventKey"></param>
        /// <param name="renderFunc"></param>
        /// <returns>绑定的渲染函数</returns>
        protected Action Bind(in EventKey eventKey, Action renderFunc)
        {
            CheckDisposed();
            ExceptionUtils.ThrowIfTrue(eventKey.Type == EventKeyType.NotEventKey, "无效的EventKey");
            ExceptionUtils.ThrowIfArgNull(renderFunc, nameof(renderFunc));
            UIMgr.Renderer.AddNode(ref _graph, eventKey, renderFunc);
            return renderFunc;
        }

        /// <summary>
        /// 绑定指定事件触发的渲染函数
        /// </summary>
        /// <param name="eventKeys"></param>
        /// <param name="renderFunc"></param>
        /// <returns>绑定的渲染函数</returns>
        [InternalParamsGCOptimization]
        protected Action Bind(Action renderFunc, params EventKey[] eventKeys)
        {
            CheckDisposed();
            ExceptionUtils.ThrowIfTrue(eventKeys == null || eventKeys.Length <= 0, "无效的EventKey");
            ExceptionUtils.ThrowIfArgNull(renderFunc, nameof(renderFunc));
            if (eventKeys == null || eventKeys.Length <= 0) return renderFunc;
            foreach (EventKey eventKey in eventKeys)
            {
                UIMgr.Renderer.AddNode(ref _graph, eventKey, renderFunc);
            }

            return renderFunc;
        }

        /// <summary>
        /// 绑定指定事件触发的渲染函数
        /// </summary>
        /// <param name="eventKeys"></param>
        /// <param name="renderFunc"></param>
        /// <returns>绑定的渲染函数</returns>
        protected Action Bind(Action renderFunc, in Params<EventKey> eventKeys)
        {
            CheckDisposed();
            ExceptionUtils.ThrowIfTrue(eventKeys.Length <= 0, "无效的EventKey");
            ExceptionUtils.ThrowIfArgNull(renderFunc, nameof(renderFunc));
            foreach (EventKey eventKey in eventKeys)
            {
                UIMgr.Renderer.AddNode(ref _graph, eventKey, renderFunc);
            }

            return renderFunc;
        }

        /// <summary>
        /// 注销与Model相关的渲染函数
        /// </summary>
        /// <param name="model"></param>
        protected void Unbind(BaseModel model)
        {
            if (_graph.g == null) return;
            CheckDisposed();
            ExceptionUtils.ThrowIfArgNull(model, nameof(model));
            UIMgr.Renderer.Clear(ref _graph, model);
        }

        /// <summary>
        /// 注销由EventKey触发的渲染函数
        /// </summary>
        /// <param name="eventKeys"></param>
        [InternalParamsGCOptimization]
        protected void Unbind(params EventKey[] eventKeys)
        {
            if (_graph.g == null || eventKeys.Length <= 0) return;
            CheckDisposed();
            foreach (EventKey eventKey in eventKeys)
            {
                if (eventKey.Type == EventKeyType.NotEventKey) continue;
                UIMgr.Renderer.Clear(ref _graph, eventKey);
            }
        }

        /// <summary>
        /// 注销由EventKey触发的渲染函数
        /// </summary>
        /// <param name="eventKeys"></param>
        protected void Unbind(in Params<EventKey> eventKeys)
        {
            if (_graph.g == null || eventKeys.Length <= 0) return;
            CheckDisposed();
            foreach (EventKey eventKey in eventKeys)
            {
                if (eventKey.Type == EventKeyType.NotEventKey) continue;
                UIMgr.Renderer.Clear(ref _graph, eventKey);
            }
        }

        /// <summary>
        /// 注销与Model相关的渲染函数，如果propertyNames参数为空，则注销与该Model相关的所有渲染函数，否则只注销与指定属性相关的渲染函数
        /// </summary>
        /// <param name="model"></param>
        /// <param name="propertyNames"></param>
        [InternalParamsGCOptimization]
        protected void Unbind(BaseModel model, params string[] propertyNames)
        {
            if (_graph.g == null) return;
            CheckDisposed();
            ExceptionUtils.ThrowIfArgNull(model, nameof(model));
            if (propertyNames == null || propertyNames.Length == 0)
                UIMgr.Renderer.Clear(ref _graph, model);
            else
                UIMgr.Renderer.Clear(ref _graph, model, propertyNames);
        }

        /// <summary>
        /// 注销与Model相关的渲染函数，如果propertyNames参数为空，则注销与该Model相关的所有渲染函数，否则只注销与指定属性相关的渲染函数（None GC Alloc）
        /// </summary>
        /// <param name="model"></param>
        /// <param name="propertyNames"></param>
        protected void Unbind(BaseModel model, in Params<string> propertyNames)
        {
            if (_graph.g == null) return;
            CheckDisposed();
            ExceptionUtils.ThrowIfArgNull(model, nameof(model));
            if (propertyNames.Length <= 0)
                UIMgr.Renderer.Clear(ref _graph, model);
            else
                UIMgr.Renderer.Clear(ref _graph, model, propertyNames);
        }

        /// <summary>
        /// 注销所有的渲染函数
        /// </summary>
        protected void UnbindAll()
        {
            if (_graph.g == null) return;
            CheckDisposed();
            UIMgr.Renderer.Remove(ref _graph);
        }

        /// <summary>
        /// 监听事件（目标对象为当前BaseUI的实例对象，会在Dispose时自动清空所有当前实例对象的所有监听）
        /// </summary>
        /// <param name="eventKey">事件唯一Key</param>
        /// <param name="callback">事件回调（不支持多播委托）</param>
        protected void On(in EventKey eventKey, Action callback)
        {
            CheckDisposed();
            EventMgr.On(eventKey, this, callback);
        }

        /// <summary>
        /// 监听事件（目标对象为当前BaseUI的实例对象，会在Dispose时自动清空所有当前实例对象的所有监听）
        /// </summary>
        /// <param name="eventKey">事件唯一key</param>
        /// <param name="callback">事件回调（不支持多播委托）</param>
        /// <typeparam name="T"></typeparam>
        protected void On<T>(in EventKey eventKey, Action<T> callback)
        {
            CheckDisposed();
            EventMgr.On(eventKey, this, callback);
        }

        /// <summary>
        /// 注销事件监听
        /// </summary>
        /// <param name="eventKey">事件唯一Key</param>
        /// <param name="callback">事件回调</param>
        protected void Off(in EventKey eventKey, Action callback)
        {
            CheckDisposed();
            EventMgr.Off(eventKey, callback);
        }

        /// <summary>
        /// 注销事件监听
        /// </summary>
        /// <param name="eventKey">事件唯一Key</param>
        /// <param name="callback">事件回调</param>
        /// <typeparam name="T">事件参数类型</typeparam>
        protected void Off<T>(in EventKey eventKey, Action<T> callback)
        {
            CheckDisposed();
            EventMgr.Off(eventKey, callback);
        }

        /// <summary>
        /// 添加定时器任务
        /// </summary>
        /// <param name="delay">延时指定时间后开始执行</param>
        /// <param name="interval">每隔指定时间后执行一次</param>
        /// <param name="repeat">执行次数，小于0表示无限执行</param>
        /// <param name="callback">回调函数</param>
        /// <param name="executeCondition">每次执行时需要满足的条件，一旦不满足条件将自动销毁当前定时器</param>
        /// <param name="cancelCondition">定时任务取消条件</param>
        /// <returns>定时器唯一id</returns>
        protected ulong AddTimer(float delay, float interval, int repeat, Action callback, Func<bool> executeCondition = null,
                                 Func<bool> cancelCondition = null)
        {
            CheckDisposed();
            TryAllocateCollections(ref _timers);
            ulong timerId = TimerMgr.Create().OfDelay(delay).OfInterval(interval).OfCount(repeat).OfCallback(callback)
                                    .OfExecuteCondition(executeCondition).OfCancelCondition(cancelCondition).Build();
            _timers.Add(timerId);
            return timerId;
        }

        /// <summary>
        /// 开启协程（注意不是Unity的协程）
        /// </summary>
        protected CoroutineID RunCoroutine(IEnumerator routine)
        {
            CheckDisposed();
            TryAllocateCollections(ref _coroutines);
            CoroutineID coroutineID = VCoroutine.Run(routine);
            _coroutines.Add(coroutineID.id);
            return coroutineID;
        }

        /// <summary>
        /// 暂停协程运行（注意不会销毁协程，协程内的状态会被保留）（注意不是Unity的协程）
        /// </summary>
        protected void StopCoroutine(in CoroutineID coroutineId)
        {
            CheckDisposed();
            VCoroutine.Stop(coroutineId);
        }

        /// <summary>
        /// 恢复协程运行（注意不是Unity的协程）
        /// </summary>
        protected void ResumeCoroutine(in CoroutineID coroutineId)
        {
            CheckDisposed();
            VCoroutine.Resume(coroutineId);
        }

        /// <summary>
        /// 销毁协程（注意不是Unity的协程）
        /// </summary>
        protected void KillCoroutine(in CoroutineID coroutineId)
        {
            CheckDisposed();
            VCoroutine.Kill(coroutineId);
        }

        /// <summary>
        /// 如果协程访问了当前界面的UI资源，防止界面被销毁后协程继续访问UI资源导致异常，可以通过ManageCoroutine将协程交由当前界面管理，在界面销毁时会自动销毁所有被管理的协程（注意不是Unity的协程）
        /// </summary>
        /// <param name="coroutineId">协程id</param>
        protected void ManageCoroutine(in CoroutineID coroutineId)
        {
            CheckDisposed();
            TryAllocateCollections(ref _coroutines);
            if (!_coroutines.Contains(coroutineId.id)) _coroutines.Add(coroutineId.id);
        }

        private void KillAllCoroutines()
        {
            if (_coroutines == null) return;
            CheckDisposed();
            foreach (ulong coroutine in _coroutines) VCoroutine.Kill(coroutine);
            _coroutines.Clear();
        }

        private void KillAllTimers()
        {
            if (_timers == null) return;
            CheckDisposed();
            foreach (ulong timer in _timers) TimerMgr.Kill(timer);
            _timers.Clear();
        }

        /// <summary>
        /// 按路径查找GameObject（包含activeSelf=false的GameObject）
        /// </summary>
        /// <param name="path">从当前UI根节点开始的路径，如：UIRoot/txtText，UIRoot</param>
        protected GameObject FindByPath(string path)
        {
            CheckDisposed();
            if (string.IsNullOrEmpty(path)) return null;
            return CoreUtils.FindByPath(path, UI);
        }

        #endregion

        #region 生命周期

        /// <summary>
        /// UI被创建成功时回调（不受Unity生命周期的管理）
        /// </summary>
        protected virtual void OnCreate() { }

        /// <summary>
        /// 被激活时回调(受Unity的生命周期控制)
        /// </summary>
        protected virtual void OnActive() { }

        /// <summary>
        /// 失活时回调(受Unity的生命周期控制)
        /// </summary>
        protected virtual void OnInactive() { }

        /// <summary>
        /// UI被销毁时回调(受Unity的生命周期控制)
        /// </summary>
        protected virtual void OnDispose() { }

        #endregion
    }
}