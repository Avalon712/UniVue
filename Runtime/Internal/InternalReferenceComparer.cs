#region

using System.Collections.Generic;
using System.Runtime.CompilerServices;

#endregion

namespace UniVue.Internal
{
    internal sealed class InternalReferenceComparer<T> : IEqualityComparer<T>
    {
        private InternalReferenceComparer()
        {
        }

        public static InternalReferenceComparer<T> Shared { get; } = new();

        public bool Equals(T x, T y)
        {
            return ReferenceEquals(x, y);
        }

        public int GetHashCode(T obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}