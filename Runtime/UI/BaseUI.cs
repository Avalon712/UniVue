using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UniVue.Common;
using UniVue.Coroutine;
using UniVue.Event;
using UniVue.Internal;
using UniVue.Model;
using UniVue.Timer;
using UniVue.Utils;

namespace UniVue.UI
{
    /// <summary>
    /// 不建议使用MonoBehaviour的任何函数，框架内部自有一套生命周期管理函数，继承自MonoBehaviour只是方便绑定UI预制体
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public abstract class BaseUI : MonoBehaviour
    {
        private List<ulong> _coroutines;
        private bool _enable;
        private RGraph _graph;
        private float _timer;
        private List<ulong> _timers;
        private CoroutineID _updateCoroutine;

        /// <summary>
        /// 是否启用回调OnUpdate()，默认false
        /// </summary>
        protected bool enableUpdate;

        /// <summary>
        /// 是否启用回调OnUpdatePerSecond()，默认false
        /// </summary>
        protected bool enableUpdatePerSecond;

        public bool Disposed { get; private set; }

        public GameObject UI { get; private set; }

        protected bool Enable
        {
            get => !Disposed && _enable;
            set
            {
                if (Disposed || _enable == value) return;
                _enable = value;
                if (value)
                    CoroutineMgr.Resume(_updateCoroutine);
                else
                    CoroutineMgr.Stop(_updateCoroutine);
                if (value)
                    RefreshUI();
                UIMgr.Renderer.SetEnable(_graph, value);
            }
        }

        internal void OnCreateInternal(GameObject ui)
        {
            if (Disposed) return;
            _coroutines = InternalObjectPool<List<ulong>>.Shared.Rent();
            _timers = InternalObjectPool<List<ulong>>.Shared.Rent();
            UI = ui;
            OnCreate();
            if (enableUpdate | enableUpdatePerSecond)
                _updateCoroutine = CoroutineMgr.Run(UpdateInternal());

            _enable = ui.gameObject.activeSelf;
        }

        internal void OnDisposeInternal()
        {
            if (Disposed) return;
            UIMgr.Renderer.Remove(ref _graph);
            KillAllCoroutines();
            KillAllTimers();
            OnDispose();
            EventMgr.Off(this);
            UI = null;
            Disposed = true;
            CoroutineMgr.Kill(_updateCoroutine);
            _updateCoroutine = 0;
            InternalObjectPool<List<ulong>>.Shared.Return(ref _coroutines);
            InternalObjectPool<List<ulong>>.Shared.Return(ref _timers);
        }

        [Conditional("UNITY_EDITOR")]
        protected void CheckDisposedAndInitialized()
        {
            ExceptionUtils.ThrowIfTrue(Disposed, Disposed ? $"{GetType().Name} is disposed." : string.Empty);
            ExceptionUtils.ThrowIfNull(_timers, "初始化尚未完成");
            ExceptionUtils.ThrowIfNull(_coroutines, "初始化尚未完成");
        }

        private IEnumerator UpdateInternal()
        {
            while (true)
            {
                if (!_enable)
                    yield return new WaitUntil(() => _enable);

                yield return null;

                float deltaTime = Time.deltaTime;
                if (enableUpdate)
                    OnUpdate(deltaTime);

                if (enableUpdatePerSecond)
                {
                    _timer += deltaTime;
                    if (_timer >= 1f)
                    {
                        _timer -= 1f;
                        OnUpdatePerSecond();
                    }
                }
            }
        }

#region 辅助函数

        /// <summary>
        /// 刷新UI（绑定的所有渲染函数都会被执行一次，当渲染状态从Disable变为Enable时会自动调用一次此函数）
        /// <remarks>绝大多数情况下你都无需手动调用此函数。此函数不是精准式的更新，只要有脏标记则会将此UI绑定的所有渲染函数都执行一遍</remarks>
        /// </summary>
        /// <param name="force">是否强制刷新，即不管有无脏标记都重新渲染一遍（默认情况只有在当前UI的渲染状态处于Disable期间有渲染状态的变化时才会真正调用）</param>
        protected void RefreshUI(bool force = false)
        {
            if (_graph.g == null) return;
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
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
        public bool ContainsModel(BaseModel model)
        {
            CheckDisposedAndInitialized();
            if (_graph.g == null) return false;
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
            CheckDisposedAndInitialized();
            ExceptionUtils.ThrowIfArgNull(oldModel, nameof(oldModel));
            ExceptionUtils.ThrowIfArgNull(newModel, nameof(newModel));
            ExceptionUtils.ThrowIfFalse(ContainsModel(oldModel), "当前UI不存在绑定的模型(oldModel)");
            ExceptionUtils.ThrowIfTrue(ContainsModel(newModel), "当前UI已经绑定了模型(newModel)");
            UIMgr.Renderer.Rebind(_graph, oldModel, newModel, Enable);
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
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
            UIMgr.Renderer.Remove(ref _graph);
        }

        /// <summary>
        /// 监听事件（目标对象为当前BaseUI的实例对象，会在Dispose时自动清空所有当前实例对象的所有监听）
        /// </summary>
        /// <param name="eventKey">事件唯一Key</param>
        /// <param name="callback">事件回调（不支持多播委托）</param>
        protected void On(in EventKey eventKey, Action callback)
        {
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
            EventMgr.On(eventKey, this, callback);
        }

        /// <summary>
        /// 注销事件监听
        /// </summary>
        /// <param name="eventKey">事件唯一Key</param>
        /// <param name="callback">事件回调</param>
        protected void Off(in EventKey eventKey, Action callback)
        {
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
        protected ulong AddTimer(float delay, float interval, int repeat, Action callback, Func<bool> executeCondition,
                                 Func<bool> cancelCondition = null)
        {
            CheckDisposedAndInitialized();
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
            CheckDisposedAndInitialized();
            CoroutineID coroutineID = CoroutineMgr.Run(routine);
            _coroutines.Add(coroutineID.id);
            return coroutineID;
        }

        /// <summary>
        /// 暂停协程运行（注意不会销毁协程，协程内的状态会被保留）（注意不是Unity的协程）
        /// </summary>
        protected void StopCoroutine(in CoroutineID coroutineId)
        {
            CheckDisposedAndInitialized();
            CoroutineMgr.Stop(coroutineId);
        }

        /// <summary>
        /// 恢复协程运行（注意不是Unity的协程）
        /// </summary>
        protected void ResumeCoroutine(in CoroutineID coroutineId)
        {
            CheckDisposedAndInitialized();
            CoroutineMgr.Resume(coroutineId);
        }

        /// <summary>
        /// 销毁协程（注意不是Unity的协程）
        /// </summary>
        protected void KillCoroutine(in CoroutineID coroutineId)
        {
            CheckDisposedAndInitialized();
            CoroutineMgr.Kill(coroutineId);
            _coroutines.Remove(coroutineId.id);
        }

        /// <summary>
        /// 如果协程访问了当前界面的UI资源，防止界面被销毁后协程继续访问UI资源导致异常，可以通过ManageCoroutine将协程交由当前界面管理，在界面销毁时会自动销毁所有被管理的协程（注意不是Unity的协程）
        /// </summary>
        /// <param name="coroutineId">协程id</param>
        protected void ManageCoroutine(in CoroutineID coroutineId)
        {
            CheckDisposedAndInitialized();
            if (!_coroutines.Contains(coroutineId.id)) _coroutines.Add(coroutineId.id);
        }

        protected void KillAllCoroutines()
        {
            CheckDisposedAndInitialized();
            foreach (ulong coroutine in _coroutines) CoroutineMgr.Kill(coroutine);
            _coroutines.Clear();
        }

        protected void KillAllTimers()
        {
            CheckDisposedAndInitialized();
            foreach (ulong timer in _timers) TimerMgr.Kill(timer);
            _timers.Clear();
        }

        /// <summary>
        /// 按路径查找GameObject（包含activeSelf=false的GameObject）
        /// </summary>
        /// <param name="path">从当前UI根节点开始的路径，如：UIRoot/txtText，UIRoot</param>
        protected GameObject FindByPath(string path)
        {
            CheckDisposedAndInitialized();
            if (string.IsNullOrEmpty(path)) return null;
            return GameObjectUtils.FindByPath(path, UI);
        }

#endregion

#region 生命周期

        /// <summary>
        /// UI被创建成功时回调
        /// </summary>
        protected virtual void OnCreate() { }

        /// <summary>
        /// 每帧回调，需设置<see cref="enableUpdate" />为true
        /// </summary>
        /// <param name="deltaTime">帧生成时间，秒</param>
        protected virtual void OnUpdate(in float deltaTime) { }

        /// <summary>
        /// 每秒调用，需设置<see cref="enableUpdatePerSecond" />为true
        /// </summary>
        protected virtual void OnUpdatePerSecond() { }

        /// <summary>
        /// UI被销毁时回调
        /// </summary>
        protected virtual void OnDispose() { }

#endregion
    }
}