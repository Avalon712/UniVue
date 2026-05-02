using System;
using System.Collections.Generic;
using System.Diagnostics;
using UniVue.Common;
using UniVue.Internal;
using UniVue.Utils;

namespace UniVue.Event
{
    public static class EventMgr
    {
        private static readonly Dictionary<EventKey, HashSet<EventCallback>> _callbacks = new(128);
        private static readonly List<EventKey> _dispatchPaths = new();

        /// <summary>
        /// 当前触发的事件
        /// </summary>
        public static EventKey CurrentTriggeredEvent { get; private set; }

        /// <summary>
        /// 任意事件被触发时都会回调此函数，此函数总是在最后被执行
        /// <remarks>派发Dispatch的事件EventKey必须要有至少一个合法的监听回调时OnEvent才会被执行</remarks>
        /// </summary>
        public static event Action<EventKey> OnEvent;

        /// <summary>
        /// 当触发死循环链路时回调，参数为事件触发链路
        /// </summary>
        public static event Action<IReadOnlyList<EventKey>> OnDeadLoop;

        private static HashSet<EventCallback> GetCallbacks(in EventKey eventKey, bool createIfNotExist = true)
        {
            if (!_callbacks.TryGetValue(eventKey, out HashSet<EventCallback> callbacks) && createIfNotExist)
            {
                callbacks = InternalObjectPool<HashSet<EventCallback>>.Shared.Rent();
                callbacks.Clear();
                _callbacks[eventKey] = callbacks;
            }

            return callbacks;
        }

        [Conditional("UNITY_EDITOR")]
        private static void CheckMutiDelegate(Delegate callback)
        {
            if (callback != null && callback.GetInvocationList().Length > 1)
                throw new ArgumentException("Multicast delegates are not supported as event callbacks.");
        }

        [Conditional("UNITY_EDITOR")]
        private static void PrintLog(in EventKey eventKey, EventCallback callback)
        {
            LogUtil.Info($"Event Dispatched: {eventKey}, Callback successfully, Registered At: {callback.trackFrame}");
        }

        private static bool CheckDeadLoop(in EventKey eventKey)
        {
            if (_dispatchPaths.Contains(eventKey))
            {
                _dispatchPaths.Add(eventKey);
                LogUtil.Warn($"检查到死循环链路[{string.Join(" => ", _dispatchPaths)}]，事件执行已被强制中断！");
                OnDeadLoop?.Invoke(_dispatchPaths);
                _dispatchPaths.RemoveAt(_dispatchPaths.Count - 1);
                return true;
            }

            _dispatchPaths.Add(eventKey);
            return false;
        }

        /// <summary>
        /// 监听事件（默认的目标对象为委托的Target对象）
        /// </summary>
        /// <param name="eventKey">事件唯一Key</param>
        /// <param name="callback">事件回调（不支持多播委托）</param>
        public static void On(in EventKey eventKey, Action callback)
        {
            ExceptionUtils.ThrowIfArgNull(callback, nameof(callback));
            CheckMutiDelegate(callback);
            GetCallbacks(eventKey).Add(new EventCallback(callback, callback.Target));
        }

        /// <summary>
        /// 监听事件（默认的观察对象为委托的Target对象）
        /// </summary>
        /// <param name="eventKey">事件唯一key</param>
        /// <param name="callback">事件回调（不支持多播委托）</param>
        /// <typeparam name="T"></typeparam>
        public static void On<T>(in EventKey eventKey, Action<T> callback)
        {
            ExceptionUtils.ThrowIfArgNull(callback, nameof(callback));
            CheckMutiDelegate(callback);
            GetCallbacks(eventKey).Add(new EventCallback(typeof(T), callback, callback.Target));
        }

        /// <summary>
        /// 监听事件
        /// </summary>
        /// <param name="eventKey">事件唯一Key</param>
        /// <param name="target">目标对象</param>
        /// <param name="callback">事件回调（不支持多播委托）</param>
        public static void On(in EventKey eventKey, object target, Action callback)
        {
            ExceptionUtils.ThrowIfArgNull(target, nameof(target));
            ExceptionUtils.ThrowIfArgNull(callback, nameof(callback));
            CheckMutiDelegate(callback);
            GetCallbacks(eventKey).Add(new EventCallback(callback, target));
        }

        /// <summary>
        /// 监听事件
        /// </summary>
        /// <param name="eventKey">事件唯一key</param>
        /// <param name="target">目标对象</param>
        /// <param name="callback">事件回调（不支持多播委托）</param>
        /// <typeparam name="T"></typeparam>
        public static void On<T>(in EventKey eventKey, object target, Action<T> callback)
        {
            ExceptionUtils.ThrowIfArgNull(target, nameof(target));
            ExceptionUtils.ThrowIfArgNull(callback, nameof(callback));
            CheckMutiDelegate(callback);
            GetCallbacks(eventKey).Add(new EventCallback(typeof(T), callback, target));
        }

        /// <summary>
        /// 注销事件监听
        /// </summary>
        /// <param name="eventKey">事件唯一Key</param>
        /// <param name="callback">事件回调</param>
        public static void Off(in EventKey eventKey, Action callback)
        {
            ExceptionUtils.ThrowIfArgNull(callback, nameof(callback));
            CheckMutiDelegate(callback);
            HashSet<EventCallback> callbacks = GetCallbacks(eventKey, false);
            if (callbacks != null)
            {
                using InternalTempCollection<List<EventCallback>, EventCallback> tempCollection = new(callbacks);
                foreach (EventCallback caller in tempCollection.Collection)
                {
                    if (caller.Is(callback))
                        callbacks.Remove(caller);
                }

                if (callbacks.Count <= 0)
                {
                    _callbacks.Remove(eventKey);
                    InternalObjectPool<HashSet<EventCallback>>.Shared.Return(ref callbacks);
                }
            }
        }

        /// <summary>
        /// 注销事件监听
        /// </summary>
        /// <param name="eventKey">事件唯一Key</param>
        /// <param name="callback">事件回调</param>
        /// <typeparam name="T">事件参数类型</typeparam>
        public static void Off<T>(in EventKey eventKey, Action<T> callback)
        {
            ExceptionUtils.ThrowIfArgNull(callback, nameof(callback));
            CheckMutiDelegate(callback);
            HashSet<EventCallback> callbacks = GetCallbacks(eventKey, false);
            if (callbacks != null)
            {
                using InternalTempCollection<List<EventCallback>, EventCallback> tempCollection = new(callbacks);
                foreach (EventCallback caller in tempCollection.Collection)
                {
                    if (caller.Is(callback))
                        callbacks.Remove(caller);
                }

                if (callbacks.Count <= 0)
                {
                    _callbacks.Remove(eventKey);
                    InternalObjectPool<HashSet<EventCallback>>.Shared.Return(ref callbacks);
                }
            }
        }

        /// <summary>
        /// 注销事件Key下的所有监听
        /// </summary>
        /// <param name="eventKey"></param>
        public static void Off(in EventKey eventKey)
        {
            HashSet<EventCallback> callbacks = GetCallbacks(eventKey, false);
            if (callbacks != null)
            {
                _callbacks.Remove(eventKey);
                callbacks.Clear();
                InternalObjectPool<HashSet<EventCallback>>.Shared.Return(ref callbacks);
            }
        }

        private static void OffTarget(HashSet<EventCallback> callbacks, object target)
        {
            if (callbacks == null) return;
            using InternalTempCollection<List<EventCallback>, EventCallback> tempCollection = new(callbacks);
            foreach (EventCallback callback in tempCollection.Collection)
            {
                if (callback.Target == target)
                    callbacks.Remove(callback);
            }
        }

        /// <summary>
        /// 注销目标对象对指定事件Key下的所有监听
        /// </summary>
        /// <param name="eventKey"></param>
        /// <param name="target">目标对象</param>
        public static void Off(in EventKey eventKey, object target)
        {
            HashSet<EventCallback> callbacks = GetCallbacks(eventKey, false);
            if (callbacks != null)
            {
                OffTarget(callbacks, target);
                if (callbacks.Count <= 0)
                {
                    _callbacks.Remove(eventKey);
                    InternalObjectPool<HashSet<EventCallback>>.Shared.Return(ref callbacks);
                }
            }
        }

        /// <summary>
        /// 注销目标对象的所有事件监听
        /// </summary>
        /// <param name="target">目标对象，如果传递为null，则所有静态回调函数都会被注销</param>
        /// <param name="allowTargetIsNull">是否允许target为null对象，默认为false</param>
        public static void Off(object target, bool allowTargetIsNull = false)
        {
            if (target == null && !allowTargetIsNull)
                return;

            List<EventKey> eventKeys = new();
            foreach (KeyValuePair<EventKey, HashSet<EventCallback>> kv in _callbacks)
            {
                OffTarget(kv.Value, target);
                if (kv.Value.Count <= 0)
                {
                    eventKeys.Add(kv.Key);
                    HashSet<EventCallback> temp = kv.Value;
                    InternalObjectPool<HashSet<EventCallback>>.Shared.Return(ref temp);
                }
            }

            foreach (EventKey eventKey in eventKeys) _callbacks.Remove(eventKey);
        }

        /// <summary>
        /// 无参事件分发（带参数的监听不会被调用）
        /// </summary>
        /// <param name="eventKey"></param>
        /// <param name="printLog">是否输出事件调用日志 默认不输出</param>
        public static void Dispatch(in EventKey eventKey, bool printLog = false)
        {
            if (eventKey.Type == EventKeyType.NotEventKey) return;
            HashSet<EventCallback> callbacks = GetCallbacks(eventKey, false);

            if (callbacks != null)
            {
                if (CheckDeadLoop(eventKey))
                    return;

                CurrentTriggeredEvent = eventKey;

                using InternalTempCollection<List<EventCallback>, EventCallback> tempCollection = new(callbacks);
                foreach (EventCallback callback in tempCollection.Collection)
                {
                    if (callbacks.Contains(callback) && callback.Invoke() && printLog)
                        PrintLog(eventKey, callback);
                }

                OnEvent?.Invoke(eventKey);
                _dispatchPaths.Clear();
                CurrentTriggeredEvent = new EventKey();
            }
        }


        /// <summary>
        /// 带参事件分发（不带参数的事件监听也能被调用）
        /// </summary>
        /// <param name="eventKey"></param>
        /// <param name="eventArg">事件参数</param>
        /// <param name="printLog">是否输出事件调用日志 默认不输出</param>
        /// <typeparam name="T">事件参数类型</typeparam>
        public static void Dispatch<T>(in EventKey eventKey, T eventArg, bool printLog = false)
        {
            if (eventKey.Type == EventKeyType.NotEventKey) return;
            HashSet<EventCallback> callbacks = GetCallbacks(eventKey, false);
            if (callbacks != null)
            {
                if (CheckDeadLoop(eventKey))
                    return;

                CurrentTriggeredEvent = eventKey;

                using InternalTempCollection<List<EventCallback>, EventCallback> tempCollection = new(callbacks);
                foreach (EventCallback callback in tempCollection.Collection)
                {
                    if (callbacks.Contains(callback) && callback.Invoke(eventArg) && printLog)
                        PrintLog(eventKey, callback);
                }

                OnEvent?.Invoke(eventKey);
                _dispatchPaths.Clear();
                CurrentTriggeredEvent = new EventKey();
            }
        }
    }
}