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
                    EventCall call = new(attribute.EventName, method,register);
                    _calls.Add(call);
                    tuple.Dispose();
                }
            }
        }

        public void AddUIEvent(UIEvent uIEvent) 
        {
            _events.Add(uIEvent);
        }

        /// <summary>
        /// 触发某件事件
        /// </summary>
        public void ExecuteEvent(UIEvent trigger)
        {
            List<EventCall> calls = _calls;
            for (int i = 0; i < calls.Count; i++)
            {
                if (calls[i].EventName == trigger.EventName)
                {
                    _currentCall = calls[i];
                    calls[i].Call(trigger);
                }
            }

            _currentCall = null;
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
                if (object.ReferenceEquals(_calls[i].Register, register) && _calls[i].EventName==eventName)
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
        public void UnregisterUIEvents()
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

    }
}
