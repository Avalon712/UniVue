using UnityEngine;
using UniVue.Common;

namespace UniVue.Event
{
    public abstract class EventUI
    {
        /// <summary>
        /// UI类型
        /// </summary>
        public UIType UIType { get; }

        /// <summary>
        /// 当前UIEvent所处的视图
        /// </summary>
        public string ViewName { get; private set; }

        /// <summary>
        /// 事件名称
        /// </summary>
        public string EventName { get; private set; }

        /// <summary>
        /// 事件参数
        /// </summary>
        public ArgumentUI[] Arguments { get; private set; }

        public EventUI(UIType type, string viewName, string eventName, ArgumentUI[] eventArgs)
        {
            UIType = type;
            ViewName = viewName;
            EventName = eventName;
            Arguments = eventArgs;
        }

        /// <summary>
        /// 获取触发当前事件的UI组件
        /// </summary>
        /// <typeparam name="T">UI组件类型</typeparam>
        /// <returns>组件类型</returns>
        public abstract T GetUI<T>() where T : Component;

        /// <summary>
        /// 为当前事件指定参数设置参数值
        /// </summary>
        /// <param name="argName">参数名称</param>
        /// <param name="argValue">参数值</param>
        public void SetEventArg(string argName, in ArgumentValue argValue)
        {
            for (int i = 0; i < Arguments.Length; i++)
            {
                if (argName == Arguments[i].ArgumentName)
                {
                    Arguments[i].SetArgumentValue(argValue);
                }
            }
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        public virtual void Unregister()
        {
            Arguments = null;
        }

        /// <summary>
        /// 触发此事件
        /// </summary>
        public void Execute()
        {
            Vue.Event.ExecuteEvent(this);
        }
    }

    public readonly ref struct ArgumentValue
    {
        public readonly SupportableArgumentType type;
        public readonly object value;

        public ArgumentValue(SupportableArgumentType type, object value)
        {
            this.type = type;
            this.value = value;
        }

        public bool TryGetArgumentValue(SupportableArgumentType wishType, out object value)
        {
            value = null;
            switch (type)
            {
                case SupportableArgumentType.Int:
                    switch (wishType)
                    {
                        case SupportableArgumentType.Int:
                            value = this.value;
                            return true;
                        case SupportableArgumentType.Float:
                            value = (float)(int)this.value;
                            return true;
                        case SupportableArgumentType.String:
                            value = this.value.ToString();
                            return true;
                        case SupportableArgumentType.Bool:
                            value = ((int)value) > 0;
                            return true;
                    }
                    break;
                case SupportableArgumentType.Float:
                    switch (wishType)
                    {
                        case SupportableArgumentType.Int:
                            value = (int)(float)this.value;
                            return true;
                        case SupportableArgumentType.Float:
                            value = this.value;
                            return true;
                        case SupportableArgumentType.String:
                            value = this.value.ToString();
                            return true;
                        case SupportableArgumentType.Bool:
                            value = ((float)value) > 0;
                            return true;
                    }
                    break;
                case SupportableArgumentType.String:
                    switch (wishType)
                    {
                        case SupportableArgumentType.Int:
                            if (int.TryParse(this.value as string, out int vi))
                            {
                                value = vi;
                                return true;
                            }
                            return false;
                        case SupportableArgumentType.Float:
                            if (float.TryParse(this.value as string, out float vf))
                            {
                                value = vf;
                                return true;
                            }
                            return false;
                        case SupportableArgumentType.String:
                            value = this.value;
                            return true;
                        case SupportableArgumentType.Bool:
                            if (bool.TryParse(this.value as string, out bool vb))
                            {
                                value = vb;
                                return true;
                            }
                            return false;
                    }
                    break;
                case SupportableArgumentType.Enum:
                    switch (wishType)
                    {
                        case SupportableArgumentType.Int:
                            value = (int)this.value;
                            return true;
                        case SupportableArgumentType.String:
                            value = this.value.ToString();
                            return true;
                        default: return false;
                    }
                case SupportableArgumentType.Bool:
                    switch (wishType)
                    {
                        case SupportableArgumentType.Int:
                            value = (bool)this.value ? 1 : 0;
                            return true;
                        case SupportableArgumentType.Float:
                            value = (bool)this.value ? 1f : 0f;
                            return true;
                        case SupportableArgumentType.String:
                            value = this.value.ToString();
                            return true;
                        case SupportableArgumentType.Bool:
                            value = (bool)this.value;
                            return true;
                    }
                    break;
                case SupportableArgumentType.Sprite:
                    if (wishType == SupportableArgumentType.Sprite)
                    {
                        value = this.value;
                        return true;
                    }
                    break;
            }
            return false;
        }
    }
}
