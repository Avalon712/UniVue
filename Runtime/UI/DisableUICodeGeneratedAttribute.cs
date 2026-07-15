using System;

namespace UniVue.UI
{
    /// <summary>
    /// 添加此特性后，此类不会进行UI代码的导出
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class DisableUICodeGeneratedAttribute : Attribute
    {
        
    }
}