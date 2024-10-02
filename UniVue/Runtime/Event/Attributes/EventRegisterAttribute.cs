using System;

namespace UniVue.Event
{
    /// <summary>
    /// 为注解此特性的类生成事件注册
    /// </summary>
    /// <remarks>此特性只有在导入<see href="https://github.com/Avalon712/UniVue-SourceGenerator">UniVue源生成器</see>后才会生效</remarks>
    [AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class EventRegisterAttribute : Attribute
    {

    }
}
