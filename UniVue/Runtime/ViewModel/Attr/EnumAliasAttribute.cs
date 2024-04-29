using System;

namespace UniVue.ViewModel.Attr
{
    /// <summary>
    /// 使用其它名字显示该枚举值（用于将枚举值显示到UI上）
    /// </summary>
    [AttributeUsage(AttributeTargets.Field,AllowMultiple = false)]
    public sealed class EnumAliasAttribute : Attribute
    {
        public string Alias { get; private set; }

        public EnumAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }
}
