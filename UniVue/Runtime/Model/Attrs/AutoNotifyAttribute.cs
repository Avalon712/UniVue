using System;

namespace UniVue.Model
{
    /// <summary>
    /// 使用C#的源生成器自动生成源代码
    /// </summary>
    /// <remarks>此特性只有在引入UniVue的源生成器后才会生效，UniVue源生成器<see href="">UniVue.SourceGenerator</see></remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class AutoNotifyAttribute : Attribute
    {
        public AutoNotifyAttribute() { }

        public AutoNotifyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; set; }
    }
}
