using System;
using UnityEngine;
using UniVue.Evt.Evts;
using UniVue.Evt;

namespace UniVue.Utils
{
    public sealed class EventUtil
    {
        private EventUtil() { }

        public static SupportableArgType GetSupportableArgType(Type type)
        {
            if (type == typeof(string)) { return SupportableArgType.String; }
            if (type == typeof(int)) { return SupportableArgType.Int; }
            if (type == typeof(float)) { return SupportableArgType.Float; }
            if (type == typeof(bool)) { return SupportableArgType.Bool; }
            if (type.IsEnum) { return SupportableArgType.Enum; }
            if (type == typeof(Sprite)) { return SupportableArgType.Sprite; }
            if (type == typeof(UIEvent)) { return SupportableArgType.UIEvent; }
            if (type == typeof(EventCall)) { return SupportableArgType.EventCall; }
            if (type == typeof(EventArg[])) { return SupportableArgType.EventArg; }
            if (ReflectionUtil.IsCustomType(type)) { return SupportableArgType.Custom; } //必须防止最后一个进行判断
            return SupportableArgType.None;
        }
    }

    /// <summary>
    /// EventCall支持的方法参数类型
    /// </summary>
    public enum SupportableArgType
    {
        /// <summary>
        /// 不被支持的类型
        /// </summary>
        None,
        Int,
        Float,
        String,
        Enum,
        Bool,
        Sprite,
        Custom,
        UIEvent,
        EventArg,
        EventCall
    }
}
