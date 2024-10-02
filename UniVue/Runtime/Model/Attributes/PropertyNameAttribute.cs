
using System;

namespace UniVue.Model
{
    /// <summary>
    /// 为字段定义属性名称
    /// <para>此特性只有在导入<see href="https://github.com/Avalon712/UniVue-SourceGenerator">UniVue源生成器</see>后才会生效</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class PropertyNameAttribute : Attribute
    {
        public PropertyNameAttribute(string name) { }
    }
}
