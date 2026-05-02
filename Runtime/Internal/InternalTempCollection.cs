using System;
using System.Collections;
using System.Collections.Generic;

namespace UniVue.Internal
{
    /// <summary>
    /// 一定配合using语句使用
    /// </summary>
    /// <typeparam name="TCollection"></typeparam>
    /// <typeparam name="TItem"></typeparam>
    internal struct InternalTempCollection<TCollection, TItem> : IDisposable,
                                                                 IEquatable<InternalTempCollection<TCollection, TItem>>,
                                                                 IEnumerable<TItem>
        where TCollection : class, ICollection<TItem>, new()
    {
        private TCollection _collection;

        public TCollection Collection
        {
            get
            {
                _collection ??= InternalObjectPool<TCollection>.Shared.Rent();
                return _collection;
            }
        }

        private bool _disposed;

        public InternalTempCollection(ICollection<TItem> collection)
        {
            _disposed = false;
            _collection = InternalObjectPool<TCollection>.Shared.Rent();
            _collection.Clear();
            if (collection != null)
            {
                foreach (TItem item in collection)
                    _collection.Add(item);
            }
        }

        public InternalTempCollection(IEnumerable<TItem> collection)
        {
            _disposed = false;
            _collection = InternalObjectPool<TCollection>.Shared.Rent();
            _collection.Clear();
            if (collection != null)
            {
                foreach (TItem item in collection)
                    _collection.Add(item);
            }
        }

        public static implicit operator TCollection(InternalTempCollection<TCollection, TItem> collection)
        {
            return collection.Collection;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Collection.Clear();
            InternalObjectPool<TCollection>.Shared.Return(ref _collection);
        }

        public bool Equals(InternalTempCollection<TCollection, TItem> other)
        {
            return false;
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return Collection.GetHashCode();
        }
    }
}