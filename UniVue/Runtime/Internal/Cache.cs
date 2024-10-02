using System;

namespace UniVue.Internal
{
    internal readonly struct Cache
    {
        /// <summary>
        /// 缓存对象
        /// </summary>
        public readonly object obj;

        public readonly InternalType type;

        public Cache(InternalType type, object obj)
        {
            this.type = type;
            this.obj = obj;
        }

        public override bool Equals(object obj)
        {
            return obj is Cache cache &&
                   type == cache.type &&
                   cache.obj == this.obj;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(obj, type);
        }

        public override string ToString()
        {
            return type.ToString();
        }
    }

    internal enum InternalType
    {
        List_String,
        List_ModelUI,
        List_PropertyUI,
        List_ModelRuleResult,
        List_EventRuleResult,
    }
}
