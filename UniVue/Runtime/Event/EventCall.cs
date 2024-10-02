using UniVue.Common;

namespace UniVue.Event
{
    public sealed class EventCall
    {
        /// <summary>
        /// 方法的参数列表信息
        /// </summary>
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

        public EventCall(Argument[] arguments,
                         string eventName,
                         string[] views,
                         string methodName,
                         string typeFullName,
                         bool isCoroutine)
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
            ArgumentUI[] args = eventUI.Arguments;
            Argument[] arguments = Arguments;
            int count = 0;
            if (args != null && arguments != null)
            {
                for (int j = 0; j < arguments.Length; j++)
                {
                    Argument argument = arguments[j];
                    for (int i = 0; i < args.Length; i++)
                    {
                        bool flag = false;
                        switch (argument.type)
                        {
                            case SupportableArgumentType.Custom:
                                if (Vue.Event.TryGetCustomArgumentMapper(argument.typeFullName, out var mapper))
                                {
                                    flag = true;
                                    argument.value = mapper.GetValue(args);
                                }
                                else
                                {
                                    ThrowUtil.ThrowWarn($"事件回调方法{MethodName}中尚未为类型为{argument.typeFullName}的自定义参数类型注册参数映射器");
                                }
                                break;
                            case SupportableArgumentType.EventUI:
                                argument.value = eventUI;
                                flag = true;
                                break;
                            case SupportableArgumentType.EventCall:
                                argument.value = this;
                                flag = true;
                                break;
                            default:
                                if (args[i].TryGetArgumentValue(argument, out object value))
                                {
                                    argument.value = value;
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

        /// <summary>
        /// 不安全的设置参数值，不会进行类型的安全检查
        /// </summary>
        public void UnsafeSetValue(object value)
        {
            this.value = value;
        }
    }
}
