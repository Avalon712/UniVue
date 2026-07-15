using System;

namespace UniVue.Model
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class NoNotifyAttribute : Attribute
    {
        
    }
}