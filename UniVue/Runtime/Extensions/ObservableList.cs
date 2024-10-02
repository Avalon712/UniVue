using System;
using System.Collections;
using System.Collections.Generic;
using UniVue.Common;

namespace UniVue.Extensions
{
    public sealed class ObservableList<T> : IEnumerable<T>, IObservableList
    {
        private int _secretKey;
        private bool _discard;

        private readonly List<T> _list;     //内部维护的List集合

        public event Action<ChangedMode, int> OnChanged;

        public ObservableList() { _list = new(); }

        public ObservableList(int capacity) { _list = new(capacity); }

        public ObservableList(IEnumerable<T> collection) { _list = new(collection); }

        public ObservableList(List<T> list, bool copy = false)
        {
            if (copy)
                _list = list.GetRange(0, list.Count);
            else
                _list = list;
        }

        public static implicit operator ObservableList<T>(List<T> list)
        {
            return new ObservableList<T>(list);
        }

        public static explicit operator List<T>(ObservableList<T> list)
        {
            return list.GetRange(0, list.Count);
        }

        public int Capacity
        {
            get => _list.Capacity;
            set => _list.Capacity = value;
        }

        public int Count => _list.Count;


        public T this[int index]
        {
            get
            {
                CheckPermission(OperablePermission.Read);
                return _list[index];
            }

            set
            {
                CheckPermission(OperablePermission.Replace);
                _list[index] = value;
                OnChanged?.Invoke(ChangedMode.Replace, index);
            }
        }

        public OperablePermission Permission { get; private set; } = OperablePermission.All;

        object IObservableList.this[int index] => this[index];

        private void CheckPermission(OperablePermission permission)
        {
            if (!HavePermission(permission))
            {
                ThrowUtil.ThrowExceptionIfTrue(true, $"当前没有对集合List拥有{CommonUtil.ToString(permission)}操作的权限，当前集合已经被使用一个密钥{_secretKey}进行了权限锁定[{CommonUtil.ToString(Permission)}]，只有符合此权限的操作才被允许,你也可以通过DiscardPermission()丢弃此权限锁定。");
            }
        }

        public void Add(T item)
        {
            CheckPermission(OperablePermission.Add);
            _list.Add(item);
            OnChanged?.Invoke(ChangedMode.Add, Count - 1);
        }


        public void AddRange(IEnumerable<T> collection)
        {
            CheckPermission(OperablePermission.Add);
            int startIndex = Count;
            _list.AddRange(collection);
            for (int i = startIndex; i < _list.Count; i++)
            {
                OnChanged?.Invoke(ChangedMode.Add, i);
            }
        }

        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
            => _list.BinarySearch(index, count, item, comparer);

        public int BinarySearch(T item)
            => BinarySearch(0, Count, item, null);

        public int BinarySearch(T item, IComparer<T> comparer)
            => BinarySearch(0, Count, item, comparer);

        public void Clear()
        {
            CheckPermission(OperablePermission.Clear);
            _list.Clear();
            OnChanged?.Invoke(ChangedMode.Clear, -1);
        }


        public bool Contains(T item)
        {
            CheckPermission(OperablePermission.Read);
            return _list.Contains(item);
        }

        public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
            => _list.ConvertAll(converter);


        public void CopyTo(T[] array) => _list.CopyTo(array);


        public void CopyTo(int index, T[] array, int arrayIndex, int count)
            => _list.CopyTo(index, array, arrayIndex, count);

        public void CopyTo(T[] array, int arrayIndex)
            => _list.CopyTo(array, arrayIndex);

        public bool Exists(Predicate<T> match)
            => FindIndex(match) != -1;

        public T Find(Predicate<T> match) => _list.Find(match);

        public List<T> FindAll(Predicate<T> match)
        {
            CheckPermission(OperablePermission.Read);
            return _list.FindAll(match);
        }

        public int FindIndex(Predicate<T> match)
        {
            CheckPermission(OperablePermission.Read);
            return _list.FindIndex(match);
        }

        public int FindIndex(int startIndex, Predicate<T> match)
        {
            CheckPermission(OperablePermission.Read);
            return _list.FindIndex(startIndex, match);
        }

        public int FindIndex(int startIndex, int count, Predicate<T> match)
        {
            CheckPermission(OperablePermission.Read);
            return _list.FindIndex(startIndex, count, match); ;
        }

        public T FindLast(Predicate<T> match)
        {
            CheckPermission(OperablePermission.Read);
            return _list.FindLast(match);
        }

        public int FindLastIndex(Predicate<T> match)
        {
            CheckPermission(OperablePermission.Read);
            return _list.FindLastIndex(match);
        }

        public int FindLastIndex(int startIndex, Predicate<T> match)
            => FindLastIndex(startIndex, startIndex + 1, match);

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
        {
            CheckPermission(OperablePermission.Read);
            return _list.FindLastIndex(startIndex, count, match);
        }

        public void ForEach(Action<T> action)
        {
            CheckPermission(OperablePermission.Read);
            _list.ForEach(action);
        }

        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _list.GetEnumerator();


        public List<T> GetRange(int index, int count) => _list.GetRange(index, count);

        public int IndexOf(T item)
        {
            CheckPermission(OperablePermission.Read);
            return _list.IndexOf(item);
        }

        public int IndexOf(T item, int index)
        {
            CheckPermission(OperablePermission.Read);
            return _list.IndexOf(item, index);
        }

        public int IndexOf(T item, int index, int count)
        {
            CheckPermission(OperablePermission.Read);
            return _list.IndexOf(item, index, count);
        }

        public void Insert(int index, T item)
        {
            CheckPermission(OperablePermission.Insert);
            _list.Insert(index, item);
            OnChanged?.Invoke(ChangedMode.Add, index);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            CheckPermission(OperablePermission.Insert);
            using (IEnumerator<T> en = collection.GetEnumerator())
            {
                while (en.MoveNext())
                {
                    Insert(index++, en.Current);
                }
            }
        }

        public int LastIndexOf(T item)
        {
            CheckPermission(OperablePermission.Read);
            return _list.LastIndexOf(item);
        }

        public int LastIndexOf(T item, int index)
        {
            CheckPermission(OperablePermission.Read);
            return _list.LastIndexOf(item, index);
        }

        public int LastIndexOf(T item, int index, int count)
        {
            CheckPermission(OperablePermission.Read);
            return _list.LastIndexOf(item, index, count);
        }

        public bool Remove(T item)
        {
            CheckPermission(OperablePermission.Remove);
            int index = IndexOf(item);
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }
            return false;
        }


        public int RemoveAll(Predicate<T> match)
        {
            CheckPermission(OperablePermission.Remove);
            int count = 0;
            for (int i = 0; i < _list.Count; i++)
            {
                if (match.Invoke(_list[i]))
                {
                    _list.RemoveAt(i);
                    OnChanged?.Invoke(ChangedMode.Remove, i);
                }
            }
            return count;
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                CheckPermission(OperablePermission.Remove);
                T removed = _list[index];
                _list.RemoveAt(index);
                OnChanged?.Invoke(ChangedMode.Remove, index);
            }
        }

        public void RemoveRange(int index, int count)
        {
            if (count > 0 && index >= 0 && index < Count)
            {
                CheckPermission(OperablePermission.Remove);
                for (int i = 0; i < count; i++)
                {
                    RemoveAt(i + index);
                }
            }
        }

        public void Reverse()
            => Reverse(0, Count);

        public void Reverse(int index, int count)
        {
            CheckPermission(OperablePermission.Reverse);
            _list.Reverse(index, count);
            OnChanged?.Invoke(ChangedMode.Sort, -1);
        }


        public void Sort()
            => Sort(0, Count, null);

        public void Sort(IComparer<T> comparer)
            => Sort(0, Count, comparer);


        public void Sort(int index, int count, IComparer<T> comparer)
        {
            CheckPermission(OperablePermission.Sort);
            _list.Sort(index, count, comparer);
            OnChanged?.Invoke(ChangedMode.Sort, -1);
        }

        public void Sort(Comparison<T> comparison)
        {
            CheckPermission(OperablePermission.Sort);
            _list.Sort(comparison);
            OnChanged?.Invoke(ChangedMode.Sort, -1);
        }


        public T[] ToArray() => _list.ToArray();


        public void TrimExcess() => _list.TrimExcess();

        public bool TrueForAll(Predicate<T> match)
        {
            CheckPermission(OperablePermission.Read);
            return _list.TrueForAll(match);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public object Get(int index)
        {
            return this[index];
        }

        public void Set(object item, int index)
        {
            this[index] = (T)item;
        }

        public void Add(object item)
        {
            Add((T)item);
        }

        public bool Remove(object item)
        {
            return Remove((T)item);
        }

        public bool Contains(object item)
        {
            return Contains((T)item);
        }

        public int IndexOf(object item)
        {
            return IndexOf((T)item);
        }

        public void SetPermission(int secretKey, OperablePermission permission)
        {
            if (_discard || _secretKey == secretKey)
            {
                _secretKey = secretKey;
                Permission = permission;
                _discard = false;
            }
        }


        public bool HavePermission(OperablePermission permission)
        {
            return (Permission & permission) == permission;
        }

        public void DiscardPermission(int secretKey)
        {
            if (!_discard && _secretKey == secretKey)
            {
                _discard = true;
                Permission = OperablePermission.All;
            }
            else
            {
                ThrowUtil.ThrowWarn($"你的密钥{secretKey}与锁定权限的密钥{_secretKey}不匹配，因此丢弃权限锁定是无效的。");
            }
        }
    }
}
