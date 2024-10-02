using System;

namespace UniVue.Extensions
{
    [Flags]
    public enum OperablePermission
    {
        Read = 1,
        Add = 2,
        Insert = 4,
        Remove = 8,
        Clear = 16,
        Replace = 32,
        Sort = 64,
        Reverse = 128,
        All = 255,
    }
}
