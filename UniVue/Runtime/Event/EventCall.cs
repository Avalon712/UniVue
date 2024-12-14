using UniVue.Common;

namespace UniVue.Event
{
    public sealed class EventCall
    {
        /// <summary>
        /// 方法的参数列表信息
        /// </summary>
        /// <remarks>这个参数顺序和方法的参数顺序完全一致</remarks>
        public Argument[] Arguments { get; }

        /// <summary>
        /// 触发回调的事件名称
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// 触发范围
        /// </summary>
        public string[] Views { get; }

        /// <summary>
        /// 方法名称
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// 方法所在的类的全类名
        /// </summary>
        public string TypeFullName { get; }

        /// <summary>
        /// 是否为协程方法
        /// </summary>
        public bool IsCoroutine { get; }

        public EventCall(Argument[] arguments, string eventName, string[] views, string methodName, string typeFullName, bool isCoroutine)
        {
            Arguments = arguments;
            EventName = eventName;
            Views = views;
            MethodName = methodName;
            TypeFullName = typeFullName;
            IsCoroutine = isCoroutine;
        }

        public bool TrySetInovkeArgumentValues(EventUI eventUI)
        {
            if (Arguments == null) return true;
            ArgumentUI[] methodArgValues = eventUI.Arguments;
            Argument[] methodArgs = Arguments;
            int count = 0;
            //方法参数不为null同时存在参数UI的情况
            if (methodArgValues != null && methodArgs != null)
            {
                for (int j = 0; j < methodArgs.Length; j++)
                {
                    Argument methodArg = methodArgs[j];
                    for (int i = 0; i < methodArgValues.Length; i++)
                    {
                        bool flag = false;
                        switch (methodArg.type)
                        {
                            case SupportableArgumentType.Custom:
                                if (Vue.Event.TryGetCustomArgumentMapper(methodArg.typeFullName, out var mapper))
                                {
                                    flag = true;
                                    methodArg.value = mapper.GetValue(methodArgValues);
                                }
                                else
                                {
                                    ThrowUtil.ThrowWarn($"事件回调方法{MethodName}中尚未为类型为{methodArg.typeFullName}的自定义参数类型注册参数映射器");
                                }
                                break;
                            case SupportableArgumentType.EventUI:
                                methodArg.value = eventUI;
                                flag = true;
                                break;
                            case SupportableArgumentType.EventCall:
                                methodArg.value = this;
                                flag = true;
                                break;
                            default:
                                if (methodArgValues[i].TryGetArgumentValue(methodArg, out object value))
                                {
                                    methodArg.value = value;
                                    flag = true;
                                }
                                break;
                        }
                        if (flag)
                        {
                            count++;
                            break;
                        }
                    }
                }
            }
            //方法参数不为null但是不存在参数UI的情况
            else if (methodArgValues == null && methodArgs != null)
            {
                for (int j = 0; j < methodArgs.Length; j++)
                {
                    Argument methodArg = methodArgs[j];
                    switch (methodArg.type)
                    {
                        case SupportableArgumentType.Custom:
                            if (Vue.Event.TryGetCustomArgumentMapper(methodArg.typeFullName, out var mapper))
                            {
                                count++;
                                methodArg.value = mapper.GetValue(methodArgValues);
                            }
                            else
                            {
                                ThrowUtil.ThrowWarn($"事件回调方法{MethodName}中尚未为类型为{methodArg.typeFullName}的自定义参数类型注册参数映射器");
                            }
                            break;
                        case SupportableArgumentType.EventUI:
                            count++;
                            methodArg.value = eventUI;
                            break;
                        case SupportableArgumentType.EventCall:
                            count++;
                            methodArg.value = this;
                            break;
                    }
                }
            }
            return count == Arguments.Length;
        }
    }

    public sealed class Argument
    {
        public readonly string typeFullName;
        public readonly SupportableArgumentType type;
        public readonly string name;
        public object value { get; internal set; }

        public Argument(string typeFullName, SupportableArgumentType type, string name)
        {
            this.typeFullName = typeFullName;
            this.type = type;
            this.name = name;
            value = null;
        }
    }
}
