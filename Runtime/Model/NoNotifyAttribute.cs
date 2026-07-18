#region

using System;

#endregion

namespace UniVue.Model
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class NoNotifyAttribute : Attribute
    {
    }
}