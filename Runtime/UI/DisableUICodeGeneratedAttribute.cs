#region

using System;

#endregion

namespace UniVue.UI
{
    /// <summary>
    /// 添加此特性后，此类不会进行UI代码的导出
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DisableUICodeGeneratedAttribute : Attribute
    {
    }
}