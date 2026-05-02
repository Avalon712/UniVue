using System;

namespace UniVue.UI
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DontGenUICodeAttribute : Attribute
    {
        public UIGenCode Code { get; set; }
    }

    public enum UIGenCode
    {
        /// <summary>
        /// 整个类都不会有代码生成
        /// </summary>
        Class,

        /// <summary>
        /// 当此类被作为其他类的属性时，不生成此属性
        /// </summary>
        Property
    }
}