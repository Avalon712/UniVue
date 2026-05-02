using System;
using System.Collections;
using System.Collections.Generic;

namespace UniVue.Common
{
    /// <summary>
    /// </summary>
    /// <typeparam name="TRequireType">期望的类型</typeparam>
    /// <typeparam name="TCollectionItemType">集合类型</typeparam>
    public struct IndexableCollectionIterator<TRequireType, TCollectionItemType>
        : IEnumerator<TRequireType>,
          IEnumerable<TRequireType>,
          IEquatable<IndexableCollectionIterator<TRequireType, TCollectionItemType>>
        where TRequireType : TCollectionItemType
    {
        private TCollectionItemType[] _array;
        private List<TCollectionItemType> _list;
        private int _index;

        private int Count
        {
            get
            {
                if (_list == null && _array == null) return 0;
                return _list == null ? _array.Length : _list.Count;
            }
        }

        public TRequireType Current { get; private set; }

        object IEnumerator.Current => Current;

        private TCollectionItemType this[int index]
        {
            get
            {
                if (_array == null && _list == null) return default;
                return _list == null ? _array[index] : _list[index];
            }
        }

        public IndexableCollectionIterator(TCollectionItemType[] array)
        {
            _index = 0;
            _array = array;
            _list = null;
            Current = default;
        }

        public IndexableCollectionIterator(List<TCollectionItemType> list)
        {
            _index = 0;
            _array = null;
            _list = list;
            Current = default;
        }

        public bool MoveNext()
        {
            int count = Count;
            bool hasNext = false;
            for (; _index < count; _index++)
            {
                TCollectionItemType item = this[_index];
                if (item != null && item is TRequireType requireItem)
                {
                    Current = requireItem;
                    _index++;
                    hasNext = true;
                    break;
                }
            }

            return hasNext;
        }

        public void Reset()
        {
            _index = 0;
            Current = default;
        }

        public void Dispose()
        {
            Current = default;
            _array = null;
            _list = null;
        }

        IEnumerator<TRequireType> IEnumerable<TRequireType>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public bool Equals(IndexableCollectionIterator<TRequireType, TCollectionItemType> other)
        {
            return Equals(_array, other._array) && Equals(_list, other._list) && _index == other._index;
        }

        public override bool Equals(object obj)
        {
            return obj is IndexableCollectionIterator<TRequireType, TCollectionItemType> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_array, _list, _index);
        }
    }
}