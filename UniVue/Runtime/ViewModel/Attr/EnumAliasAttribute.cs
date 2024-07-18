using System;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 使用其它名字显示该枚举值（用于将枚举值显示到UI上）
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class EnumAliasAttribute : Attribute
    {
        public string Languag { get; private set; }

        public string Alias { get; private set; }

        public EnumAliasAttribute(string alias)
        {
            Alias = alias;
        }

        public EnumAliasAttribute(string languag, string alias)
        {
            Languag = languag;
            Alias = alias;
        }
    }
}
