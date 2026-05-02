using System;
using System.Collections.Generic;

namespace UniVue.Internal
{
    internal sealed class InternalEqualityComparer<T>
    {
        private static readonly Dictionary<Type, object> _equalityComparers = new(16);

        public static EqualityComparer<T> Comparer
        {
            get
            {
                Type type = typeof(T);
                if (!_equalityComparers.TryGetValue(type, out object comparerObj))
                {
                    comparerObj = EqualityComparer<T>.Default;
                    _equalityComparers[type] = comparerObj;
                }

                return (EqualityComparer<T>)comparerObj;
            }
        }
    }
}