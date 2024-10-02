using System;
using UniVue.i18n;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 使用其它名字显示该枚举值（用于将枚举值显示到UI上）
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class EnumAliasAttribute : Attribute
    {
        public Language language { get; }

        public string Alias { get; }

        public EnumAliasAttribute(string alias) : this(Language.None, alias) { }

        public EnumAliasAttribute(Language langTag, string alias)
        {
            language = langTag;
            Alias = alias;
        }
    }
}
