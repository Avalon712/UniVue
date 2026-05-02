using System;
using System.Collections.Generic;

namespace UniVue.Common
{
    public enum HeapType
    {
        /// <summary>
        /// 小顶堆
        /// </summary>
        MinHeap,

        /// <summary>
        /// 大顶堆
        /// </summary>
        MaxHeap
    }

    public sealed class Heap<T> where T : IComparable<T>
    {
        private readonly List<T> _items;

        public Heap(HeapType heapType, int capacity)
        {
            Type = heapType;
            _items = new List<T>(capacity);
        }

        public Heap(HeapType heapType, IEnumerable<T> collection)
        {
            Type = heapType;
            _items = new List<T>(collection);
            BuildHeap();
        }

        public HeapType Type { get; }
        public int Count => _items.Count;

        public void Add(T item)
        {
            _items.Add(item);
            ShiftUp(_items.Count - 1);
        }

        public T Remove()
        {
            if (_items.Count == 0) throw new InvalidOperationException("Heap is empty.");

            T top = _items[0];
            int lastIndex = _items.Count - 1;
            _items[0] = _items[lastIndex];
            _items.RemoveAt(lastIndex);

            if (_items.Count > 0) ShiftDown(0);

            return top;
        }

        public void Clear()
        {
            _items.Clear();
        }

        public T Peek()
        {
            if (_items.Count == 0) throw new InvalidOperationException("Heap is empty.");

            return _items[0];
        }

        private void BuildHeap()
        {
            for (int i = Parent(_items.Count - 1); i >= 0; i--) ShiftDown(i);
        }

        private void ShiftUp(int index)
        {
            while (index > 0)
            {
                int parent = Parent(index);
                if (!HasHigherPriority(index, parent)) break;

                Swap(index, parent);
                index = parent;
            }
        }

        private void ShiftDown(int index)
        {
            while (true)
            {
                int left = Left(index);
                int right = left + 1;
                int best = index;

                if (left < _items.Count && HasHigherPriority(left, best)) best = left;

                if (right < _items.Count && HasHigherPriority(right, best)) best = right;

                if (best == index) break;

                Swap(index, best);
                index = best;
            }
        }

        private bool HasHigherPriority(int lhs, int rhs)
        {
            int compare = _items[lhs].CompareTo(_items[rhs]);
            return Type == HeapType.MaxHeap ? compare > 0 : compare < 0;
        }

        private void Swap(int i, int j)
        {
            (_items[i], _items[j]) = (_items[j], _items[i]);
        }

        private static int Parent(int index)
        {
            return (index - 1) / 2;
        }

        private static int Left(int index)
        {
            return index * 2 + 1;
        }
    }
}