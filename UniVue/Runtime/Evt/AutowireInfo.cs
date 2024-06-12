using System;
using UnityEngine;
using UniVue.Evt.Attr;

namespace UniVue.Evt
{
    public sealed class AutowireInfo
    {
        public EventCallAutowireAttribute EventCallInfo { get; private set; }

        public Type type { get; private set; }

        public AutowireInfo(Type type, EventCallAutowireAttribute eventCallInfo)
        {
            this.type = type; EventCallInfo = eventCallInfo;
        }

        internal object AutowireEventCall()
        {
            //必须先判断MonoBehaviour
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                return new GameObject(type.Name, type);
            }
            else if (typeof(IEventRegister).IsAssignableFrom(type))
            {
                return Activator.CreateInstance(type);
            }

            return null;
        }
    }
}
