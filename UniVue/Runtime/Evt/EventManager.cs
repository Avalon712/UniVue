using System;
using System.Collections.Generic;
using System.Reflection;
using UniVue.Evt.Attr;
using UniVue.Evt.Evts;
using UniVue.Utils;

namespace UniVue.Evt
{
    public sealed class EventManager
    {
        private List<UIEvent> _events;
        private List<EventCall> _calls;
        private List<AutowireInfo> _autowires;

        internal EventManager() { _events = new(18);_calls = new(18); }

        /// <summary>
        /// 当前执行的事件回调
        /// </summary>
        private EventCall _currentCall; 

        /// <summary>
        /// 注册事件注册器
        /// </summary>
        /// <typeparam name="T">实现IEventRegister接口类型</typeparam>
        /// <param name="register">实现IEventRegister接口对象</param>
        public void Signup<T>(T register) where T : IEventRegister
        {
            CustomTuple<MethodInfo, EventCallAttribute> tuple = new();
            using (var r = ReflectionUtil.GetEventCallMethods(register, tuple).GetEnumerator())
            {
                while (r.MoveNext())
                {
                    MethodInfo method = tuple.Item1;
                    EventCallAttribute attribute = tuple.Item2;
                    EventCall call = new(attribute, method, register);
                    _calls.Add(call);
                    tuple.Dispose();
                }
            }
        }

        /// <summary>
        /// 自动装配EventCall
        /// </summary>
        internal void ConfigAutowireEventCalls(string[] scanAssemblies)
        {
            //只执行一次
            if(_autowires == null)
            {
                _autowires = new List<AutowireInfo>();
                using (var it = ReflectionUtil.GetAutowireInfos(scanAssemblies).GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        _autowires.Add(it.Current);
                    }
                }
            }
        }

        /// <summary>
        /// 对不支持反射的时候提高的API
        /// </summary>
        internal void AddAutowireInfos(Type[] types)
        {
            if(types != null)
            {
                for (int i = 0; i < types.Length; i++)
                {
                    var attribute = types[i].GetCustomAttribute<EventCallAutowireAttribute>();
                    if (attribute != null)
                    {
                        if (types[i].GetInterface("IEventRegister") != null)
                        {
                            var autowire = new AutowireInfo(types[i], attribute);
                            if(_autowires == null) { _autowires = new(); }
                            _autowires.Add(autowire);
                        }
#if UNITY_EDITOR
                        else
                        {
                            LogUtil.Warning("EventCallAutowireAttribute特性只能注解在实现了接口IEventRegister的类上!");
                        }
#endif
                    }
                }
            }
        }

        /// <summary>
        /// 当场景加载完成时调用
        /// </summary>
        public void AutowireAndUnloadEventCalls(string sceneName)
        {
            if(_autowires == null) { return; }

            for (int i = 0; i < _autowires.Count; i++)
            {
                AutowireInfo info = _autowires[i];
                if (info.EventCallInfo.Scenes != null)
                {
                    string[] scenes = info.EventCallInfo.Scenes;
                    bool unload = true; //是否要卸载当前类型的所有EventCall

                    for (int k = 0; k < scenes.Length; k++)
                    {
                        if (scenes[k] == sceneName && (!info.EventCallInfo.singleton || !ContainsType(info.type)))
                        {
                            object register = info.AutowireEventCall();
#if UNITY_EDITOR
                            LogUtil.Info($"已自动装配类型为{register.GetType()}的注册器到场景{sceneName}中!");
#endif
                            unload = false;
                            break;
                        }
                    }

                    if (unload)
                    {
                        for (int k = 0; k < _calls.Count; k++)
                        {
                            if (_calls[k].Register.GetType() == info.type)
                            {
#if UNITY_EDITOR
                                string[] views = _calls[k].CallInfo.Views;
                                string s = views != null ? string.Join(", ", views) : string.Empty;
                                LogUtil.Info($"已自动卸载类型为{_calls[k].Register.GetType()}的注册器注册的EventCall{{EventName={_calls[k].CallInfo.EventName},Views=[{s}]}}!");
#endif
                                ListUtil.TrailDelete(_calls, k--);
                            }
                        }
                    }
                }
            }
        }

        internal void AddUIEvent(UIEvent uIEvent) 
        {
            _events.Add(uIEvent);
        }

        /// <summary>
        /// 为事件参数赋值
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="viewName">哪个视图下的事件</param>
        /// <param name="values">key=参数名,value=参数值</param>
        public void SetEventArgs(string eventName,string viewName,Dictionary<string,object> values)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                UIEvent @event = _events[i];
                if(@event.EventName == eventName && @event.ViewName == viewName)
                {
                    @event.SetEventArgs(values);
                }
            }
        }

        /// <summary>
        /// 获取指定类型的注册器
        /// 注：如果有多种类型的，只会返回第一个查找到的
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetRegister<T>() where T : IEventRegister
        {
            for (int i = 0; i < _calls.Count; i++)
            {
                if (_calls[i].Register.GetType() == typeof(T)) { return (T)_calls[i].Register; }
            }
            return default;
        }

        /// <summary>
        /// 获取指定类型的所有注册器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>IEnumerable<T></returns>
        public IEnumerable<T> GetRegisters<T>() where T : IEventRegister
        {
            for (int i = 0; i < _calls.Count; i++)
            {
                if (_calls[i].Register.GetType() == typeof(T)) { yield return (T)_calls[i].Register; }
            }
        }

        /// <summary>
        /// 触发某件事件
        /// </summary>
        public void ExecuteEvent(UIEvent trigger)
        {
            List<EventCall> calls = _calls;
            for (int i = 0; i < calls.Count; i++)
            {
                if (calls[i].CallInfo.EventName == trigger.EventName)
                {
                    string[] views = calls[i].CallInfo.Views;
                    bool exe = views == null;
                    if (!exe)
                    {
                        exe = false;
                        for (int j = 0; j < views.Length; j++)
                        {
                            if (views[j] == trigger.ViewName) { exe = true;break; }
                        }
                    }

                    if (exe)
                    {
                        _currentCall = calls[i];
                        calls[i].Call(trigger);
                    }
                }
            }

            _currentCall = null;
        }

        /// <summary>
        /// 主动触发事件
        /// </summary>
        /// <param name="eventName">触发事件的名称</param>
        /// <param name="viewNames">指定哪些视图下的此事件进行触发，如果为null，则所有定义了此事件的视图都将进行触发</param>
        public void TriggerUIEvent(string eventName, params string[] viewNames)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                if (_events[i].EventName == eventName)
                {
                    if (viewNames == null) { _events[i].Trigger(); }
                    else
                    {
                        for (int j = 0; j < viewNames.Length; j++)
                        {
                            if (_events[i].ViewName == viewNames[j])
                            {
                                _events[i].Trigger();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 注销某个事件注册器
        /// </summary>
        /// <typeparam name="T">实现IEventRegister接口类型</typeparam>
        /// <param name="register">实现IEventRegister接口对象</param>
        public void Signout<T>(T register) where T : IEventRegister
        {
            for (int i = 0; i < _calls.Count; i++)
            {
                if(object.ReferenceEquals(_calls[i].Register,register))
                {
                    _calls[i].Unregister();
                    ListUtil.TrailDelete(_calls, i--);
                }
            }
        }

        /// <summary>
        /// 注销所有的事件注册器
        /// </summary>
        public void SignoutAll()
        {
            for (int i = 0; i < _calls.Count; i++)
            {
                _calls[i].Unregister();
            }
            _calls.Clear();
        }

        /// <summary>
        /// 注销指定注册器的指定事件
        /// </summary>
        /// <typeparam name="T">实现IEventRegister接口类型</typeparam>
        /// <param name="register">实现IEventRegister接口类型的对象</param>
        /// <param name="eventName">要注销的事件名称</param>
        public void UnregiserEventCall<T>(T register,string eventName)
        {
            for (int i = 0; i < _calls.Count; i++)
            {
                if (object.ReferenceEquals(_calls[i].Register, register) && _calls[i].CallInfo.EventName==eventName)
                {
                    _calls[i].Unregister();
                    ListUtil.TrailDelete(_calls, i--);
                }
            }
        }

        /// <summary>
        /// 注销所有UI事件
        /// 注：在场景切换时调用
        /// </summary>
        public void UnregisterAllUIEvents()
        {
            for (int i = 0; i < _events.Count; i++)
            {
                _events[i].Unregister();
            }
            _events.Clear();
        }

        /// <summary>
        /// 卸载指定事件名称的UIEvent
        /// </summary>
        /// <param name="eventName">要卸载的事件名称</param>
        public void UnregisterUIEvent(string eventName)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                if (_events[i].EventName == eventName)
                {
                    _events[i].Unregister();
                    ListUtil.TrailDelete(_events, i--);
                }
            }
        }

        /// <summary>
        /// 卸载指定视图的所有UIEvent
        /// </summary>
        /// <param name="viewName">要卸载UIEvent的视图名称</param>
        public void UnregisterUIEvents(string viewName)
        {
            for (int i = 0; i < _events.Count; i++)
            {
                if (_events[i].ViewName == viewName) 
                {
                    _events[i].Unregister();
                    ListUtil.TrailDelete(_events, i--);
                }
            }
        }

        public EventArg[] GetCurrentEventArgs()
        {
            if(_currentCall == null)
            {
#if UNITY_EDITOR
                LogUtil.Warning("当前没有事件正在被触发执行!无法获取到事件参数!");
#endif
                return null;
            }
            return _currentCall.GetCurrentEventArgs();
        }

        /// <summary>
        /// 判断当前所有的EventCall中是否存在此类型的注册器
        /// </summary>
        public bool ContainsType(Type type) 
        {
            if (!typeof(IEventRegister).IsAssignableFrom(type)) { return false; }

            for (int i = 0; i < _calls.Count; i++)
            {
                if (_calls[i].Register.GetType() == type) { return true; }
            }

            return false;
        }
    }
}
