
using System;

namespace UniVue.Model
{
    /// <summary>
    /// 当前属性更改时也通知其它属性
    /// <para>此特性只有在导入<see href="https://github.com/Avalon712/UniVue-SourceGenerator">UniVue源生成器</see>后才会生效</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public sealed class AlsoNotifyAttribute : Attribute
    {
        public AlsoNotifyAttribute(string propertyName) { }
    }
}
