using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace UniVue.Internal
{
    internal sealed class InternalObjectPool<T> where T : class, new()
    {
        private static readonly ConcurrentDictionary<Type, object> _pools = new();
        private readonly Func<T> _createFunc;
        private readonly Action<T> _disposeFunc;
        private readonly HashSet<T> _items = new(4, InternalReferenceComparer<T>.Shared);

        private uint _maxCapacity = uint.MaxValue;

        private InternalObjectPool() { }

        public InternalObjectPool(Func<T> createFunc, Action<T> disposeFunc)
        {
            _createFunc = createFunc;
            _disposeFunc = disposeFunc;
        }

        public int Count => _items.Count;

        /// <summary>
        /// 最大容量，超过该容量的对象将不再被缓存，而是直接丢弃（默认为uint.MaxValue，即不限制容量）
        /// </summary>
        public uint MaxCapacity
        {
            get => _maxCapacity;
            set
            {
                _maxCapacity = value;
                while (Count > value)
                    TryRemove(out _);
            }
        }

        public static InternalObjectPool<T> Shared
        {
            get
            {
                Type type = typeof(T);
                if (!_pools.TryGetValue(type, out object poolObj))
                {
                    poolObj = new InternalObjectPool<T>();
                    _pools[type] = poolObj;
                }

                return (InternalObjectPool<T>)poolObj;
            }
        }

        private bool TryRemove(out T item)
        {
            item = null;
            foreach (T r in _items)
            {
                item = r;
                break;
            }

            ;
            return item != null && _items.Remove(item);
        }

        public T Rent()
        {
            if (!TryRemove(out T item))
                return _createFunc != null ? _createFunc.Invoke() : new T();
            return item;
        }

        public void Return(ref T item)
        {
            if (item == null) return;
            _disposeFunc?.Invoke(item);
            if (Count >= MaxCapacity || !_items.Add(item)) return;
            item = null;
        }
    }
}