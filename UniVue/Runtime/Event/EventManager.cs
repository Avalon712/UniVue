using System;
using System.Collections.Generic;
using UniVue.Common;

namespace UniVue.Event
{
    public sealed class EventManager
    {
        private readonly List<EventUI> _events;
        private readonly List<Caller> _callers;
        //key=typeFullName value=该注册器的所有事件调用模板
        private readonly Dictionary<string, List<EventCall>> _table;
        private readonly Dictionary<string, ICustomArgumentMapper> _mappers;

        private readonly struct Caller
        {
            public readonly string typeId;
            public readonly IEventRegister register;

            public Caller(string typeId, IEventRegister register)
            {
                this.typeId = typeId;
                this.register = register;
            }

            public override bool Equals(object obj)
            {
                return obj is Caller caller &&
                       typeId == caller.typeId &&
                       this.register == caller.register;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(typeId, register);
            }
        }

        internal EventManager()
        {
            _mappers = new Dictionary<string, ICustomArgumentMapper>();
            _callers = new List<Caller>(10);
            _events = new List<EventUI>(20);
            _table = new Dictionary<string, List<EventCall>>(10);
        }

        internal List<EventUI> Events => _events;

        /// <summary>
        /// 当回调了某个事件后调用此函数
        /// </summary>
        /// <remarks>arg0-当前调用的事件调用 arg1-当前方法执行结果</remarks>
        public event Action<EventCall, object> OnInvoked;

        /// <summary>
        /// 注册事件注册器
        /// </summary>
        /// <param name="register">实现IEventRegister接口对象</param>
        public void Register(IEventRegister register)
        {
            string typeId = register.GetType().FullName;
            Caller r = new Caller(typeId, register);
            if (!_callers.Contains(r))
            {
                if (!_table.ContainsKey(typeId))
                {
                    List<EventCall> calls = new List<EventCall>();
                    register.OnRegisterEventCall(calls);
                    _table.Add(typeId, calls);
                }
                _callers.Add(r);
            }
        }

        /// <summary>
        /// 注销某个事件注册器
        /// </summary>
        /// <typeparam name="T">实现IEventRegister接口类型</typeparam>
        /// <param name="register">实现IEventRegister接口对象</param>
        public void Unregister(IEventRegister register)
        {
            for (int i = 0; i < _callers.Count; i++)
            {
                if (ReferenceEquals(_callers[i].register, register))
                {
                    _callers.RemoveAt(i);
                    break;
                }
            }
        }

        public void AddUIEvent(EventUI eventUI)
        {
            _events.Add(eventUI);
        }

        /// <summary>
        /// 为事件参数赋值
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="viewName">哪个视图下的事件</param>
        /// <param name="argName">参数名</param>
        /// <param name="argValue">参数值</param>
        public void SetEventArg(string eventName, string viewName, string argName, in ArgumentValue argValue)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                EventUI @event = _events[i];
                if (@event.EventName == eventName && @event.ViewName == viewName)
                {
                    @event.SetEventArg(argName, argValue);
                }
            }
        }


        /// <summary>
        /// 为事件参数赋值
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="argName">参数名</param>
        /// <param name="argValue">参数值</param>
        public void SetEventArg(string eventName, string argName, in ArgumentValue argValue)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                EventUI @event = _events[i];
                if (@event.EventName == eventName)
                {
                    @event.SetEventArg(argName, argValue);
                }
            }
        }

        /// <summary>
        /// 执行指定的UI事件
        /// </summary>
        public void ExecuteEvent(EventUI @event)
        {
            for (int i = 0; i < _callers.Count; i++)
            {
                Caller caller = _callers[i];
                if (_table.TryGetValue(caller.typeId, out List<EventCall> calls))
                {
                    for (int j = 0; j < calls.Count; j++)
                    {
                        EventCall call = calls[j];
                        if (call.EventName == @event.EventName && call.TrySetInovkeArgumentValues(@event))
                        {
                            object result = caller.register.Invoke(call);
                            OnInvoked?.Invoke(call, result);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 执行指定事件名称以及触发范围的事件
        /// </summary>
        /// <param name="eventName">触发事件的名称</param>
        /// <param name="viewNames">指定哪些视图下的此事件进行触发，如果为null，则所有定义了此事件的视图都将进行触发</param>
        public void ExecuteEvent(string eventName, params string[] viewNames)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                if (_events[i].EventName == eventName)
                {
                    if (viewNames == null) { ExecuteEvent(_events[i]); }
                    else
                    {
                        for (int j = 0; j < viewNames.Length; j++)
                        {
                            if (_events[i].ViewName == viewNames[j])
                            {
                                ExecuteEvent(_events[i]);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 注销所有UI事件
        /// 注：在场景切换时调用
        /// </summary>
        internal void UnloadResources()
        {
            for (int i = 0; i < _events.Count; i++)
            {
                _events[i].Unregister();
            }
            _events.Clear();
        }

        /// <summary>
        /// 获取所有的注册器
        /// </summary>
        /// <returns>内部所有注册的注册器List<IEventRegister></returns>
        public List<IEventRegister> GetAllRegisters()
        {
            return _callers.ConvertAll(c => c.register);
        }

        /// <summary>
        /// 注册自定义的事件参数映射器
        /// </summary>
        /// <param name="typeFullName">参数类型的全类型名称</param>
        /// <param name="argument">参数映射器</param>
        public void RegisterCustomArgumentMapper(string typeFullName, ICustomArgumentMapper argument)
        {
            if (!_mappers.TryAdd(typeFullName, argument))
            {
                ThrowUtil.ThrowWarn($"重复为类型{typeFullName}注册参数映射器!");
            }
        }

        internal bool TryGetCustomArgumentMapper(string typeFullName, out ICustomArgumentMapper mapper)
        {
            return _mappers.TryGetValue(typeFullName, out mapper);
        }
    }
}
