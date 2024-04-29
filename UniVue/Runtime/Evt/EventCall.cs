using System;
using System.Reflection;
using UniVue.Evt.Evts;
using UniVue.Utils;

namespace UniVue.Evt
{
    public sealed class EventCall
    {
        private MethodInfo _call;

        public IEventRegister Register { get; private set; }

        /// <summary>
        /// 绑定的事件名称
        /// </summary>
        public string EventName { get; private set; }

        /// <summary>
        /// 当前触发的事件
        /// </summary>
        private UIEvent _triggerEvt;

        /// <summary>
        /// 方法执行参数
        /// </summary>
        private object[] _parameters;

        public EventCall(string eventName,MethodInfo call,IEventRegister register)
        {
            EventName = eventName; _call = call;_triggerEvt = null; Register = register;
        }

        internal void Call(UIEvent triggerEvt)
        {
            _triggerEvt = triggerEvt;
            SetParameterValues();
            _call.Invoke(Register, _parameters);
            _triggerEvt = null;
        }

        public void Unregister()
        {
            if (_parameters != null)
            {
                for (int i = 0; i < _parameters.Length; i++)
                {
                    _parameters[i] = null;
                }
                _parameters = null;
            }
            _call = null;
            Register = null;
            EventName = null;
        }


        public EventArg[] GetCurrentEventArgs()
        {
            if (_triggerEvt == null)
            {
#if UNITY_EDITOR
                LogUtil.Warning("当前没有事件正在被触发执行!无法获取到事件参数!");
#endif
                return null;
            }
            return _triggerEvt.EventArgs;
        }

        /// <summary>
        /// 对方法参数进行赋值
        /// </summary>
        private void SetParameterValues()
        {
            ParameterInfo[] parameters = _call.GetParameters();
            if (parameters != null && parameters.Length > 0)
            {
                EventArg[] args = _triggerEvt.EventArgs;
                _parameters = _parameters == null ? new object[parameters.Length] : _parameters;

                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo parameter = parameters[i];
                    string argName = parameter.Name;
                    if (args != null)
                    {
                        _parameters[i] = null;

                        for (int j = 0; j < args.Length; j++)
                        {
                            //EventCall类型
                            if (parameter.ParameterType == typeof(EventCall))
                            {
                                _parameters[i] = this;
                                continue;
                            }

                            //UIEvent类型
                            if (parameter.ParameterType == typeof(UIEvent))
                            {
                                _parameters[i] = _triggerEvt;
                                continue;
                            }

                            //EventArg[]类型
                            if (parameter.ParameterType == typeof(EventArg[]))
                            {
                                _parameters[i] = _triggerEvt.EventArgs;
                                continue;
                            }

                            //自定义的类型数据
                            if (ReflectionUtil.IsCustomType(parameter.ParameterType))
                            {
                                //重用之前创建的对象实例
                                if (_parameters[i] != null)
                                {
                                    EntityMapper.SetValues(_parameters[i], args);
                                }
                                else
                                {
                                    _parameters[i] = EntityMapper.Map(parameter.ParameterType, args);
                                }
                            }

                            if (args[j].ArgumentName == argName)
                            {
                                //如果是枚举类型
                                if (parameter.ParameterType.IsEnum)
                                {
                                    object value = args[i].GetArgumentValue();
                                    if (value.GetType() == typeof(int))
                                    {
                                        _parameters[i] = Enum.ToObject(parameter.ParameterType, value);
                                    }
                                    else
                                    {
#if UNITY_EDITOR
                                        LogUtil.Warning($"方法[{_call.Name}]: 参数名为{argName}的类型为{parameter.ParameterType}，与UI返回的事件参数类型{value.GetType()}不是一个int类型，无法将一个非int类型的值转为枚举类型!");
#endif
                                    }
                                }
                                else //如果是int\string\float\bool类型
                                {
                                    object value = args[i].GetArgumentValue();
                                    if (value.GetType() != parameter.ParameterType)
                                    {
#if UNITY_EDITOR
                                        LogUtil.Warning($"方法[{_call.Name}]: 参数名为{argName}的类型为{parameter.ParameterType}，与UI返回的事件参数类型{value.GetType()}不一致，无法正确进行赋值!");
#endif
                                    }
                                    else
                                    {
                                        _parameters[i] = value;
                                    }
                                }

                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
