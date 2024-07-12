using System;
using System.Reflection;
using UniVue.Evt.Attr;
using UniVue.Evt.Evts;
using UniVue.Utils;

namespace UniVue.Evt
{
    public sealed class EventCall
    {
        private MethodInfo _call;

        public IEventRegister Register { get; private set; }


        public EventCallAttribute CallInfo { get; private set; }

        /// <summary>
        /// 当前触发的事件
        /// </summary>
        private UIEvent _triggerEvt;

        /// <summary>
        /// 方法执行参数
        /// </summary>
        private object[] _parameters;

        public EventCall(EventCallAttribute callInfo, MethodInfo call, IEventRegister register)
        {
            CallInfo = callInfo; _call = call; _triggerEvt = null; Register = register;
            ParameterInfo[] parameters = _call.GetParameters();
            if (parameters != null && parameters.Length > 0)
            {
                _parameters = new object[parameters.Length];
            }
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
            CallInfo = null;
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
            EventArg[] args = _triggerEvt.EventArgs;

            if (parameters != null && parameters.Length > 0 && args != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    ParameterInfo parameter = parameters[i];
                    string argName = parameter.Name;

                    SupportableArgType argType = ReflectionUtil.GetSupportableArgType(parameter.ParameterType);

                    if (argType == SupportableArgType.None) { continue; }

                    if ((int)argType < 7)
                    {
                        SetArgMatchValue(argName, parameter.ParameterType, i, argType, args);
                    }
                    else
                    {
                        switch (argType)
                        {
                            case SupportableArgType.Custom:
                                IEntityMapper mapper = Vue.Event.GetEntityMapper(parameter.ParameterType);
                                if (mapper != null)
                                {
                                    if (_parameters[i] == null)
                                        _parameters[i] = mapper.CreateEntity();
                                    mapper.SetValues(_parameters[i], args);
                                }
                                else
                                {
                                    if (_parameters[i] == null)
                                        _parameters[i] = EntityMapper.CreateEntity(parameter.ParameterType);
                                    EntityMapper.SetValues(_parameters[i], args);
                                }
                                break;

                            case SupportableArgType.UIEvent:
                                _parameters[i] = _triggerEvt;
                                break;

                            case SupportableArgType.EventArg:
                                _parameters[i] = _triggerEvt.EventArgs;
                                break;

                            case SupportableArgType.EventCall:
                                _parameters[i] = this;
                                break;
                        }
                    }

                }
            }
        }

        private void SetArgMatchValue(string argName, Type parameterType, int i, SupportableArgType argType, EventArg[] args)
        {
            //除开以上类型，以下这些类型将进行参数名与类型都匹配成功才能设置
            for (int j = 0; j < args.Length; j++)
            {
                if (args[j].ArgumentName == argName)
                {
                    object value = args[j].GetArgumentValue();
                    Type valueType = value.GetType();

                    //int转enum
                    if (argType == SupportableArgType.Enum && valueType == typeof(int))
                    {
                        try
                        {
                            _parameters[i] = Enum.ToObject(parameterType, value);
                            return;
                        }
                        catch (Exception e)
                        {
#if UNITY_EDITOR
                            LogUtil.Warning($"无法将{valueType.Name}转为枚举类型{parameterType.Name}, 异常原因:{e.Message}");
#endif
                        }
                    }

                    //string转enum
                    if (argType == SupportableArgType.Enum && valueType == typeof(string))
                    {
                        if (Enum.TryParse(parameterType, value as string, out _parameters[i]))
                        {
                            return;
                        }
                    }

                    //string转int、float类型
                    if (argType == SupportableArgType.Int && valueType == typeof(string))
                    {
                        if (int.TryParse(value as string, out int result))
                        {
                            _parameters[i] = result;
                            return;
                        }
                    }

                    //string转int、float类型
                    if (argType == SupportableArgType.Float && valueType == typeof(string))
                    {
                        if (float.TryParse(value as string, out float result))
                        {
                            _parameters[i] = result;
                            return;
                        }
                    }

                    //int、float转string
                    if (argType == SupportableArgType.String && (valueType == typeof(int) || valueType == typeof(float)))
                    {
                        _parameters[i] = value.ToString();
                        return;
                    }

                    //float转int
                    if (argType == SupportableArgType.Int && valueType == typeof(float))
                    {
                        _parameters[i] = (int)(float)value;
                        return;
                    }

                    //int转float
                    if (argType == SupportableArgType.Float && valueType == typeof(int))
                    {
                        _parameters[i] = (float)(int)value;
                        return;
                    }

                    if (valueType == parameterType)
                    {
                        _parameters[i] = value;
                    }
#if UNITY_EDITOR
                    else
                    {
                        LogUtil.Warning($"方法[{_call.Name}]: 参数名为{argName}的类型为{parameterType}，与UI返回的事件参数类型{value.GetType()}不一致，无法正确进行赋值!");
                    }
#endif
                    return;
                }
            }
        }


    }


}
