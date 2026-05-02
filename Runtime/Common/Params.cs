using System;
using System.Collections.Generic;
using UniVue.Internal;
using UniVue.Utils;

namespace UniVue.Common
{
    /// <summary>
    /// 避免params T[]的数组对象开销，最多只能支持10个参数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly ref struct Params<T>
    {
        private readonly T _v0;
        private readonly T _v1;
        private readonly T _v2;
        private readonly T _v3;
        private readonly T _v4;
        private readonly T _v5;
        private readonly T _v6;
        private readonly T _v7;
        private readonly T _v8;
        private readonly T _v9;

        public int Length { get; }

        public Params(int length, in T v0, in T v1 = default, in T v2 = default, in T v3 = default, in T v4 = default,
                      in T v5 = default,
                      in T v6 = default, in T v7 = default, in T v8 = default, in T v9 = default)
        {
            _v0 = v0;
            _v1 = v1;
            _v2 = v2;
            _v3 = v3;
            _v4 = v4;
            _v5 = v5;
            _v6 = v6;
            _v7 = v7;
            _v8 = v8;
            _v9 = v9;
            Length = length;
        }

        public T this[int index]
        {
            get
            {
                ExceptionUtils.ThrowIfTrue(index < 0 || index >= Length,
                                           $"Index Out Of Bounds, index = {index}, length = {Length}");
                return index switch
                {
                    0 => _v0,
                    1 => _v1,
                    2 => _v2,
                    3 => _v3,
                    4 => _v4,
                    5 => _v5,
                    6 => _v6,
                    7 => _v7,
                    8 => _v8,
                    9 => _v9,
                    _ => default
                };
            }
        }

        public T[] ToArray()
        {
            T[] array = new T[Length];
            for (int i = 0; i < Length; i++) array[i] = this[i];
            return array;
        }

        public List<T> ToList()
        {
            List<T> list = new(Length);
            foreach (T param in this) list.Add(param);
            return list;
        }

        public int IndexOf(T item)
        {
            for (int i = 0; i < Length; i++)
            {
                if (InternalEqualityComparer<T>.Comparer.Equals(this[i], item))
                    return i;
            }

            return -1;
        }

        public Enumerator<T> GetEnumerator()
        {
            return new Enumerator<T>(Length, this);
        }

        /// <summary>
        /// 传递引用类型时，将从最后一个参数开始判断元素数量，如果最后一个参数不为null，则数量为10，倒数第二个不为null则为9，依次类推
        /// </summary>
        /// <returns></returns>
        public static Params<TClass> _<TClass>(TClass v0, TClass v1 = null, TClass v2 = null, TClass v3 = null,
                                               TClass v4 = null, TClass v5 = null, TClass v6 = null,
                                               TClass v7 = null, TClass v8 = null, TClass v9 = null)
            where TClass : class
        {
            int length = 0;
            if (v9 != null) length = 10;
            else if (v8 != null) length = 9;
            else if (v7 != null) length = 8;
            else if (v6 != null) length = 7;
            else if (v5 != null) length = 6;
            else if (v4 != null) length = 5;
            else if (v3 != null) length = 4;
            else if (v2 != null) length = 3;
            else if (v1 != null) length = 2;
            else if (v0 != null) length = 1;
            else length = 0;
            return new Params<TClass>(length, v0, v1, v2, v3, v4, v5, v6, v7, v8, v9);
        }

        /// <summary>
        /// 传递结构体参数时必须指定长度
        /// </summary>
        /// <returns></returns>
        public static Params<TStruct> _<TStruct>(int length, in TStruct v0, in TStruct v1 = default,
                                                 in TStruct v2 = default,
                                                 in TStruct v3 = default,
                                                 in TStruct v4 = default, in TStruct v5 = default,
                                                 in TStruct v6 = default,
                                                 in TStruct v7 = default, in TStruct v8 = default,
                                                 in TStruct v9 = default)
            where TStruct : struct
        {
            return new Params<TStruct>(Math.Clamp(length, 0, 10), v0, v1, v2, v3, v4, v5, v6, v7, v8, v9);
        }


        /// <summary>
        /// 传递引用类型时，将从最后一个参数开始判断元素数量，如果最后一个参数不为null，则数量为10，倒数第二个不为null则为9，依次类推
        /// </summary>
        /// <returns></returns>
        public static Params<object> _(object v1, object v2 = null, object v3 = null, object v4 = null,
                                       object v5 = null, object v6 = null, object v7 = null, object v8 = null,
                                       object v9 = null)
        {
            return _<object>(v1, v2, v3, v4, v5, v6, v7, v8, v9);
        }

        public struct Enumerator<TElement>
        {
            private readonly TElement _v0;
            private readonly TElement _v1;
            private readonly TElement _v2;
            private readonly TElement _v3;
            private readonly TElement _v4;
            private readonly TElement _v5;
            private readonly TElement _v6;
            private readonly TElement _v7;
            private readonly TElement _v8;
            private readonly TElement _v9;
            private int _index;
            private readonly int _length;

            public Enumerator(int length, in Params<TElement> @params)
            {
                _v0 = @params._v0;
                _v1 = @params._v1;
                _v2 = @params._v2;
                _v3 = @params._v3;
                _v4 = @params._v4;
                _v5 = @params._v5;
                _v6 = @params._v6;
                _v7 = @params._v7;
                _v8 = @params._v8;
                _v9 = @params._v9;
                Current = default;
                _length = length;
                _index = -1;
            }

            public bool MoveNext()
            {
                Current = ++_index switch
                {
                    0 => _v0,
                    1 => _v1,
                    2 => _v2,
                    3 => _v3,
                    4 => _v4,
                    5 => _v5,
                    6 => _v6,
                    7 => _v7,
                    8 => _v8,
                    9 => _v9,
                    _ => default
                };
                return _index < _length;
            }

            public void Reset()
            {
                _index = -1;
            }

            public TElement Current { get; private set; }
        }
    }
}