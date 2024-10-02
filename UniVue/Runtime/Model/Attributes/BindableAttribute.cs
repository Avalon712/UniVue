
using System;

namespace UniVue.Model
{
    /// <summary>
    /// 注解此特性的类将自动实现IBindableModel接口
    /// <para>此特性只有在导入<see href="https://github.com/Avalon712/UniVue-SourceGenerator">UniVue源生成器</see>后才会生效</para>
    /// </summary>
    /// <remarks>此特性只能注解在一个分部类<see langword="partial class"/>和分部结构体<see langword="partial struct"/>上，同时这个类或结构体不能是一个内部类或内部结构体，否则将不会其作用</remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class BindableAttribute : Attribute
    {
        public bool OnPropertyChanged { get; set; }

        public BindableAttribute() { }
    }

}
