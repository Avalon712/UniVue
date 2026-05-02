using System;
using System.Collections.Generic;

namespace UniVue.Coroutine
{
    public sealed class CoroutineYieldHandleContext
    {
        private readonly Dictionary<Type, YieldHandler> _handlers;
        private Dictionary<Type, YieldHandler> _cacheHandlers;

        public CoroutineYieldHandleContext(List<YieldHandler> handlers)
        {
            _handlers = new Dictionary<Type, YieldHandler>();
            for (int i = 0; i < handlers.Count; i++) _handlers.TryAdd(handlers[i].YieldType, handlers[i]);
        }

        public static CoroutineYieldHandleContext Default { get; } = new(NewInternalHandlers);

        public static List<YieldHandler> NewInternalHandlers => new()
        {
            new YieldAsyncOperation(),
            new YieldCoroutineID(),
            new YieldEnumerator(),
            new YieldPredicate(),
            new YieldTask(),
            new YieldWaitForEndOfFrame(),
            new YieldWaitForFixedUpdate(),
            new YieldWaitForSeconds(),
            new YieldWaitForSecondsRealtime(),
            new YieldWaitUntil(),
            new YieldWaitWhile()
        };

        /// <summary>
        /// 获取指定yield类型的处理器
        /// </summary>
        /// <remarks>优先找具体类型的YieldHandler，如果没有再查找其父类类型的处理器</remarks>
        /// <param name="yieldType">yield的类型</param>
        /// <returns></returns>
        public YieldHandler GetHandler(Type yieldType)
        {
            //优先找具体类型
            if (_handlers.TryGetValue(yieldType, out YieldHandler handler)) return handler;

            //找父类类型
            if (_cacheHandlers != null && _cacheHandlers.TryGetValue(yieldType, out YieldHandler cache)) return cache;

            _cacheHandlers ??= new Dictionary<Type, YieldHandler>();

            foreach (Type type in _handlers.Keys)
            {
                if (type.IsAssignableFrom(yieldType))
                {
                    handler = _handlers[type];
                    _cacheHandlers.Add(yieldType, handler);
                    return handler;
                }
            }

            _cacheHandlers.Add(yieldType, null);
            return null;
        }

        /// <summary>
        /// 注册处理器到默认的Yield处理器上下文中
        /// </summary>
        /// <param name="handler">YieldHandler</param>
        public static void RegisterHandlerToDefaultContext(YieldHandler handler)
        {
            Dictionary<Type, YieldHandler> handlers = Default._handlers;
            handlers.TryAdd(handler.YieldType, handler);

            Dictionary<Type, YieldHandler> cacheHandlers = Default._cacheHandlers;
            if (cacheHandlers != null) cacheHandlers.Remove(handler.YieldType);
        }

        /// <summary>
        /// 从默认的Yield处理器上下文中注销指定yield类型的YieldHandler
        /// </summary>
        /// <typeparam name="T">yield return的类型</typeparam>
        public static void UnregisterHandlerFromDefaultContext<T>()
        {
            Dictionary<Type, YieldHandler> handlers = Default._handlers;
            Type yieldType = typeof(T);
            handlers.Remove(yieldType);

            Dictionary<Type, YieldHandler> cacheHandlers = Default._cacheHandlers;
            if (cacheHandlers != null) cacheHandlers.TryAdd(yieldType, null);
        }
    }
}