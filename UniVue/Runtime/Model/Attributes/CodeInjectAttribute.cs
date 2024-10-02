
using System;

namespace UniVue.Model
{
    /// <summary>
    /// 在属性方法的指定位置注入指定代码
    /// <para>此特性只有在导入<see href="https://github.com/Avalon712/UniVue-SourceGenerator">UniVue源生成器</see>后才会生效</para>
    /// </summary>
    /// <remarks>可以注入多行代码，每行代码需要使用代码结束符-英文分号';'</remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class CodeInjectAttribute : Attribute
    {
        public CodeInjectAttribute(InjectType type, string code) { }
    }


    public enum InjectType
    {
        Get,
        Set_BeforeChanged,
        Set_AfterChanged,
    }
}
