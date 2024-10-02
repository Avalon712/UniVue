
using System;

namespace UniVue.Model
{
    /// <summary>
    /// 指定不要为注解此特性的字段生成通知属性
    /// <para>此特性只有在导入<see href="https://github.com/Avalon712/UniVue-SourceGenerator">UniVue源生成器</see>后才会生效</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DontNotifyAttribute : Attribute
    {

    }
}
