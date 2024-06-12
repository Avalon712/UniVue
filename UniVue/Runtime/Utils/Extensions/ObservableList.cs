using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

#if UNITY_EDITOR
using System.Diagnostics;
using System.Reflection;
#endif

namespace UniVue.Utils
{

    public sealed class ObservableList<T> : IEnumerable<T>, IObservableList
    {
        private string _debugInfo;          //抛出异常时的调试信息
        private bool _immutable;            //当前是否为不可变集合
        private readonly List<T> _list;     //内部维护的List集合
        private int _currId;                //标识当前在那个观察者的回调函数

        public ObservableList() { _list = new(); }

        public ObservableList(int capacity) { _list = new(capacity); }

        public ObservableList(IEnumerable<T> collection) { _list = new(collection); }

        /// <summary>
        /// 当集合内容发生改变时调用（包括元素的增加、删除、替换、顺序改变）
        /// </summary>
        private List<ValueTuple<int, Action<NotificationMode>>> _changes = new(1);

        /// <summary>
        /// 当有新数据添加时回调
        /// </summary>
        private List<ValueTuple<int, Action<object>>> _adds = new(1);

        /// <summary>
        /// 当有元素被移除时回调（arg0: 被移除的元素 arg1: 被移除元素的索引； ）
        /// </summary>
        private List<ValueTuple<int, Action<object, int>>> _removes = new(1);

        /// <summary>
        /// 当元素被替换时回调。arg0是元素的索引；arg1是替换前的元素；arg2是替换后的元素
        /// </summary>
        private List<ValueTuple<int, Action<int, object, object>>> _replaces = new(1);

        public int Capacity
        {
            get => _list.Capacity;
            set => _list.Capacity = value;
        }

        public int Count => _list.Count;

        public int CurrentObserverId => _currId;

        private void Changed(NotificationMode mode)
        {
            for (int i = 0; i < _changes.Count; i++)
            {
                _currId = _changes[i].Item1;
                _changes[i].Item2.Invoke(mode);
            }
            _currId = int.MinValue;
        }

        private void Removed(int index, T reomved)
        {
            for (int i = 0; i < _removes.Count; i++)
            {
                _currId = _removes[i].Item1;
                _removes[i].Item2.Invoke(reomved, index);
            }
            _currId = int.MinValue;
            Changed(NotificationMode.Remove);
        }

        private void Added(T added)
        {
            for (int i = 0; i < _adds.Count; i++)
            {
                _currId = _adds[i].Item1;
                _adds[i].Item2.Invoke(added);
            }
            _currId = int.MinValue;
            Changed(NotificationMode.Add);
        }

        private void Replaced(int index, T r1, T r2)
        {
            for (int i = 0; i < _replaces.Count; i++)
            {
                _currId = _replaces[i].Item1;
                _replaces[i].Item2.Invoke(index, r1, r2);
            }
            _currId = int.MinValue;
            Changed(NotificationMode.Replace);
        }

        public T this[int index]
        {
            get => _list[index];

            set
            {
                T old = _list[index];
                _list[index] = value;
                Replaced(index, old, value);
            }
        }

        /// <summary>
        /// 设置为不可变|可变的集合
        /// </summary>
        public bool Immutable
        {
            get => _immutable;
            set
            {
                _immutable = value;
                SetDebugInfo();
            }
        }

        private void CheckImmutable()
        {
            if (_immutable)
                throw new InvalidOperationException($"不可对不可变集合进行内容修改！{_debugInfo}，因此禁止对集合内容进行改变（禁止的操作包括：元素顺序改变、增加、删除、插入）。");
        }

        public void Add(T item)
        {
            CheckImmutable();
            _list.Add(item);
            Added(item);
        }


        public void AddRange(IEnumerable<T> collection)
        {
            CheckImmutable();
            int startIndex = Count - 1;
            _list.AddRange(collection);
            if (_adds != null)
            {
                for (int i = startIndex; i < _list.Count; i++)
                {
                    Added(_list[i]);
                }
            }
        }

        public ReadOnlyCollection<T> AsReadOnly() => _list.AsReadOnly();


        public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
            => _list.BinarySearch(index, count, item, comparer);

        public int BinarySearch(T item)
            => BinarySearch(0, Count, item, null);

        public int BinarySearch(T item, IComparer<T> comparer)
            => BinarySearch(0, Count, item, comparer);

        public void Clear()
        {
            CheckImmutable();
            if (_removes != null)
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    Removed(i, _list[i]);
                }
            }
            _list.Clear();
        }


        public bool Contains(T item) => _list.Contains(item);



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

        public List<T> FindAll(Predicate<T> match) => _list.FindAll(match);

        public int FindIndex(Predicate<T> match) => _list.FindIndex(match);

        public int FindIndex(int startIndex, Predicate<T> match) => _list.FindIndex(startIndex, match);

        public int FindIndex(int startIndex, int count, Predicate<T> match)
            => _list.FindIndex(startIndex, count, match);

        public T FindLast(Predicate<T> match) => _list.FindLast(match);

        public int FindLastIndex(Predicate<T> match) => _list.FindLastIndex(match);

        public int FindLastIndex(int startIndex, Predicate<T> match)
            => FindLastIndex(startIndex, startIndex + 1, match);

        public int FindLastIndex(int startIndex, int count, Predicate<T> match)
            => _list.FindLastIndex(startIndex, count, match);

        public void ForEach(Action<T> action) => _list.ForEach(action);


        public List<T>.Enumerator GetEnumerator() => _list.GetEnumerator();

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => _list.GetEnumerator();


        public List<T> GetRange(int index, int count) => _list.GetRange(index, count);

        public int IndexOf(T item) => _list.IndexOf(item);


        public int IndexOf(T item, int index) => _list.IndexOf(item, index);


        public int IndexOf(T item, int index, int count) => _list.IndexOf(item, index, count);


        public void Insert(int index, T item)
        {
            CheckImmutable();
            _list.Insert(index, item);
            Added(item);
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            using (IEnumerator<T> en = collection.GetEnumerator())
            {
                while (en.MoveNext())
                {
                    Insert(index++, en.Current);
                }
            }
        }

        public int LastIndexOf(T item) => _list.LastIndexOf(item);

        public int LastIndexOf(T item, int index) => _list.LastIndexOf(item, index);

        public int LastIndexOf(T item, int index, int count) => _list.LastIndexOf(item, index, count);

        public bool Remove(T item)
        {
            CheckImmutable();
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
            CheckImmutable();
            int count = 0;
            for (int i = 0; i < _list.Count; i++)
            {
                if (match(_list[i]))
                {
                    T removed = _list[i];
                    _list.RemoveAt(i);
                    Removed(i, removed);
                }
            }
            return count;
        }

        public void RemoveAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                CheckImmutable();
                T removed = _list[index];
                _list.RemoveAt(index);
                Removed(index, removed);
            }
        }

        public void RemoveRange(int index, int count)
        {
            if (count > 0 && index >= 0 && index < Count)
            {
                CheckImmutable();
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
            CheckImmutable();
            _list.Reverse(index, count);
            Changed(NotificationMode.Sort);
        }


        public void Sort()
            => Sort(0, Count, null);

        public void Sort(IComparer<T> comparer)
            => Sort(0, Count, comparer);


        public void Sort(int index, int count, IComparer<T> comparer)
        {
            CheckImmutable();
            _list.Sort(index, count, comparer);
            Changed(NotificationMode.Sort);
        }

        public void Sort(Comparison<T> comparison)
        {
            CheckImmutable();
            _list.Sort(comparison);
            Changed(NotificationMode.Sort);
        }


        public T[] ToArray() => _list.ToArray();


        public void TrimExcess() => _list.TrimExcess();

        public bool TrueForAll(Predicate<T> match) => _list.TrueForAll(match);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public V Get<V>(int index)
        {
            CheckTypeMatch<V>();
            return (V)(object)_list[index];
        }

        public void Set<V>(V item, int index)
        {
            CheckTypeMatch<V>();
            this[index] = (T)(object)item;
        }

        public void Add<V>(V item)
        {
            CheckTypeMatch<V>();
            Add((T)(object)item);
        }

        public bool Remove<V>(V item)
        {
            CheckTypeMatch<V>();
            return Remove((T)(object)item);
        }

        public bool Contains<V>(V item)
        {
            CheckTypeMatch<V>();
            return Contains((T)(object)item);
        }

        public void Sort<V>(IComparer<V> comparer)
        {
            CheckTypeMatch<V>();
            if (comparer != null)
            {
                Sort((i1, i2) => comparer.Compare((V)(object)i1, (V)(object)i2));
            }
        }

        public void AddListener_OnChanged(int observerId, Action<NotificationMode> onChanged)
        {
            _changes.Add((observerId, onChanged));
        }

        public void AddListener_OnRemoved<V>(int observerId, Action<V, int> onRemoved)
        {
            CheckTypeMatch<V>();
            _removes.Add((observerId, (item, index) => onRemoved((V)item, index)));
        }

        public void AddListener_OnAdded<V>(int observerId, Action<V> onAdded)
        {
            CheckTypeMatch<V>();
            _adds.Add((observerId, item => onAdded((V)item)));
        }

        public void AddListener_OnReplaced<V>(int observerId, Action<int, V, V> onReplaced)
        {
            CheckTypeMatch<V>();
            _replaces.Add((observerId, (index, r1, r2) => onReplaced(index, (V)r1, (V)r2)));
        }

        public void ClearObservers()
        {
            _changes.Clear();
            _removes.Clear();
            _adds.Clear();
            _replaces.Clear();
        }

        public void RemoveListeners(int observerId)
        {
            RemoveListener_OnAdded(observerId);
            RemoveListener_OnChanged(observerId);
            RemoveListener_OnReplaced(observerId);
            RemoveListener_OnRemoved(observerId);
        }

        public void RemoveListener_OnChanged(int observerId)
        {
            for (int i = 0; i < _changes.Count; i++)
            {
                if (_changes[i].Item1 == observerId) { _changes.RemoveAt(i--); }
            }
        }

        public void RemoveListener_OnRemoved(int observerId)
        {
            for (int i = 0; i < _removes.Count; i++)
            {
                if (_removes[i].Item1 == observerId) { _removes.RemoveAt(i--); }
            }
        }

        public void RemoveListener_OnAdded(int observerId)
        {
            for (int i = 0; i < _adds.Count; i++)
            {
                if (_adds[i].Item1 == observerId) { _adds.RemoveAt(i--); }
            }
        }

        public void RemoveListener_OnReplaced(int observerId)
        {
            for (int i = 0; i < _replaces.Count; i++)
            {
                if (_replaces[i].Item1 == observerId) { _replaces.RemoveAt(i--); }
            }
        }

        private void SetDebugInfo()
        {
#if UNITY_EDITOR
            if (_immutable)
            {
                StackTrace stackTrace = new StackTrace();
                StackFrame stackFrame = stackTrace.GetFrame(2);
                MethodBase methodBase = stackFrame.GetMethod();
                string className = methodBase.DeclaringType.FullName;
                string methodName = methodBase.Name;
                _debugInfo = $"你在类[{className}]中的[{methodName}]方法将ObservableList<{typeof(T).Name}>集合设置为了一个不可变集合";
            }
            else
            {
                _debugInfo = null;
            }
#endif
        }

        private void CheckTypeMatch<V>()
        {
            if (!typeof(V).IsAssignableFrom(typeof(T)))
                throw new Exception($"类型{typeof(V).FullName}无法转换为类型{typeof(T).FullName}");
        }

        public int IndexOf<V>(V item)
        {
            CheckTypeMatch<V>();
            return IndexOf((T)(object)item);
        }
    }
}
