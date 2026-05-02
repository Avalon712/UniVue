using System;

namespace UniVue.UI
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LazyInitUIAttribute : Attribute
    {
        public LazyInitUIAttribute(string path)
        {
            Path = path;
        }

        public string Path { get; }
    }
}