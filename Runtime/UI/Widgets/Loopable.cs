using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Utils;

namespace UniVue.UI.Widgets
{
    public enum ScrollDirection
    {
        /// <summary>
        /// 垂直
        /// </summary>
        Vertical,

        /// <summary>
        /// 水平
        /// </summary>
        Horizontal
    }

    [DontGenUICode(Code = UIGenCode.Property)]
    public abstract class LoopItem : BaseComponent { }

    [RequireComponent(typeof(ScrollRect))]
    public abstract class Loopable : BaseComponent
    {
        [SerializeField]
        protected ScrollDirection scrollDir;

        [SerializeField]
        protected Vector2 gap;

        [SerializeField]
        protected Vector2Int grid;

        private LinkedList<RectTransform> _children;

        private int _dataCount;

        /// <summary>
        /// 数据头指针，指向的是第一个Item的位置所渲染的数据的索引
        /// </summary>
        protected int _head;

        /// <summary>
        /// 采用脏标记模式解决快速拖动时留白渲染跟不上的问题
        /// </summary>
        private bool _isDirty;

        private Vector3[] _itemCorners;
        protected ScrollRect _scrollRect;

        /// <summary>
        /// 数据尾指针，指向的是最后一个Item的位置所渲染的数据的索引
        /// </summary>
        protected int _tail;

        private Vector3[] _viewportCorners;

        public ScrollRect Scroller
        {
            get
            {
                if (!_scrollRect) _scrollRect = GetComponent<ScrollRect>();
                return _scrollRect;
            }
        }

        public Scrollbar Scrollbar => scrollDir == ScrollDirection.Horizontal
            ? Scroller.horizontalScrollbar
            : Scroller.verticalScrollbar;

        protected int FirstIndex => 0;

        protected abstract int LastIndex { get; }

        protected int ChildCount { get; private set; }

        protected LinkedListNode<RectTransform> FirstChild => _children.First;

        protected LinkedListNode<RectTransform> LastChild => _children.Last;

        /// <summary>
        /// 显示的数据的数量
        /// </summary>
        public int Count
        {
            get => _dataCount;
            set
            {
                _dataCount = value;
                Refresh(true);
            }
        }

        protected Action<int, LoopItem> OnItemRender { get; private set; }

        protected Vector2 Distance { get; private set; }

        protected sealed override void OnCreate()
        {
            enableUpdate = true;
            _scrollRect = GetComponent<ScrollRect>();


            //防止BaseComponent的Show回调
            RectTransform content = _scrollRect.content;
            ChildCount = content.childCount;
            int childCount = ChildCount;
            _children = new LinkedList<RectTransform>();
            for (int i = 0; i < childCount; i++)
            {
                Transform child = content.GetChild(i);
                child.gameObject.SetActive(false);
                _children.AddLast(child as RectTransform);
            }

            Distance = FirstChild.Value.sizeDelta * (gap / Vector2.Max(gap, Vector2.one)) + gap;

            //绑定滚动事件
            _itemCorners = new Vector3[4];
            _viewportCorners = new Vector3[4];
            _scrollRect.onValueChanged.AddListener(OnScroll);
            Scrollbar?.onValueChanged.AddListener(OnScroll);
            Scroller.vertical = scrollDir == ScrollDirection.Vertical;
            Scroller.horizontal = scrollDir == ScrollDirection.Horizontal;
            Count = 0;
        }

        protected override void OnUpdate(in float deltaTime)
        {
            if (!_isDirty) return;
            _scrollRect.viewport.GetWorldCorners(_viewportCorners);
            int maxUpdateSteps = Mathf.Max(grid.x * grid.y, 1);

            if (scrollDir == ScrollDirection.Vertical)
            {
                GetChild(FirstIndex).Value.GetWorldCorners(_itemCorners);
                if (_itemCorners[0].y > _viewportCorners[1].y)
                {
                    while (_isDirty && maxUpdateSteps-- > 0)
                    {
                        GetChild(FirstIndex).Value.GetWorldCorners(_itemCorners);
                        _isDirty = _itemCorners[0].y > _viewportCorners[1].y &&
                                   OnMoveItem(Direction.Up, _viewportCorners, _itemCorners);
                    }
                }
                else
                {
                    GetChild(LastIndex).Value.GetWorldCorners(_itemCorners);
                    if (_itemCorners[0].y < _viewportCorners[0].y)
                    {
                        while (_isDirty && maxUpdateSteps-- > 0)
                        {
                            GetChild(LastIndex).Value.GetWorldCorners(_itemCorners);
                            _isDirty = _itemCorners[0].y < _viewportCorners[0].y &&
                                       OnMoveItem(Direction.Down, _viewportCorners, _itemCorners);
                        }
                    }
                    else
                    {
                        _isDirty = false;
                    }
                }
            }
            else
            {
                GetChild(FirstIndex).Value.GetWorldCorners(_itemCorners);
                if (_itemCorners[3].x < _viewportCorners[1].x)
                {
                    while (_isDirty && maxUpdateSteps-- > 0)
                    {
                        GetChild(FirstIndex).Value.GetWorldCorners(_itemCorners);
                        _isDirty = _itemCorners[3].x < _viewportCorners[1].x &&
                                   OnMoveItem(Direction.Left, _viewportCorners, _itemCorners);
                    }
                }
                else
                {
                    GetChild(LastIndex).Value.GetWorldCorners(_itemCorners);
                    if (_itemCorners[3].x > _viewportCorners[3].x)
                    {
                        while (_isDirty && maxUpdateSteps-- > 0)
                        {
                            GetChild(LastIndex).Value.GetWorldCorners(_itemCorners);
                            _isDirty = _itemCorners[3].x > _viewportCorners[3].x &&
                                       OnMoveItem(Direction.Right, _viewportCorners, _itemCorners);
                        }
                    }
                    else
                    {
                        _isDirty = false;
                    }
                }
            }
        }

        protected sealed override void OnDispose()
        {
            _scrollRect.onValueChanged.RemoveListener(OnScroll);
            Scrollbar?.onValueChanged.RemoveListener(OnScroll);
            OnItemRender = null;
        }

        private void OnScroll(float value)
        {
            _isDirty = true;
        }

        private void OnScroll(Vector2 _)
        {
            _isDirty = true;
        }

        /// <summary>
        /// 只刷新可以被看见的区域的数据
        /// </summary>
        protected void RefreshViewArea()
        {
            int count = Count;
            _tail = _head;

            foreach (RectTransform child in _children)
            {
                LoopItem item = child.GetComponent<LoopItem>();
                if (_tail < count)
                    OnItemRender?.Invoke(_tail++, item);
                else
                    item.Hide();
            }

            --_tail;
        }

        public void BindItemRender<T>(Action<int, T> itemRender) where T : LoopItem
        {
            ExceptionUtils.ThrowIfArgNull(itemRender, nameof(itemRender));
            ExceptionUtils.ThrowIfTrue(OnItemRender != null, "不能重复绑定");
            OnItemRender = (index, item) =>
            {
                if (item is T itemT)
                {
                    item.Show();
                    itemRender.Invoke(index, itemT);
                }
            };
        }

        /// <summary>
        /// 强制刷新
        /// </summary>
        protected void ForceRefresh()
        {
            _head = 0;
            _scrollRect.normalizedPosition = scrollDir == ScrollDirection.Vertical ? Vector2.up : Vector2.zero;
            ResetItemPos(Vector3.zero);
            RefreshViewArea();
        }

        /// <summary>
        /// 根据第一个Item的位置重新计算每个Item的位置
        /// </summary>
        /// <param name="firstItemPos">第一个Item的位置</param>
        protected void ResetItemPos(Vector2 firstItemPos)
        {
            int rows = grid.y;
            int cols = grid.x;
            Vector2 itemPos = firstItemPos;
            Vector2 cellSize = FirstChild.Value.sizeDelta;
            float xDeltaPos = cellSize.x + gap.x;
            float yDeltaPos = cellSize.y + gap.y;

            //按下面的方法确保content的前面rows个为第一列或前cols个为第一行
            if (scrollDir == ScrollDirection.Vertical) //垂直滚动时位置按行一行一行的设置
            {
                for (int i = 0; i < rows; i++)
                {
                    for (int j = 0; j < cols; j++)
                    {
                        GetChild(i * cols + j).Value.anchoredPosition = itemPos;
                        itemPos.x += xDeltaPos;
                    }

                    itemPos.y -= yDeltaPos; //下一行
                    itemPos.x -= xDeltaPos * cols;
                }
            }
            else //水平滚动时位置按列一列一列的设置
            {
                for (int i = 0; i < cols; ++i)
                {
                    for (int j = 0; j < rows; ++j)
                    {
                        GetChild(i * rows + j).Value.anchoredPosition = itemPos;
                        itemPos.y -= yDeltaPos;
                    }

                    itemPos.x += xDeltaPos; //下一列
                    itemPos.y += yDeltaPos * rows;
                }
            }
        }

        /// <summary>
        /// 刷新视图
        /// </summary>
        /// <param name="force">是否为强制刷新</param>
        public abstract void Refresh(bool force = false);

        /// <summary>
        /// 重新计算ScrollRect的内容区域大小
        /// </summary>
        protected virtual void Resize()
        {
            float temp = scrollDir == ScrollDirection.Vertical ? Count / (float)grid.x : Count / (float)grid.y;
            Vector2 directionVector = scrollDir == ScrollDirection.Vertical ? new Vector2(0, 1) : new Vector2(1, 0);
            _scrollRect.content.sizeDelta = (Mathf.FloorToInt(temp) + 1) * (Distance * directionVector);

            //当前是否可以移动
            _scrollRect.movementType = Count <= gap.x * gap.y
                ? ScrollRect.MovementType.Clamped
                : ScrollRect.MovementType.Elastic;
        }

        /// <summary>
        /// 移动Item的位置
        /// </summary>
        /// <param name="direction">当前滚动方向</param>
        /// <param name="viewportCorners">可见区域的四个角的世界坐标</param>
        /// <param name="itemCorners">当前被移动的Item的四个角的世界坐标</param>
        /// <returns>true-还需继续移动 false-无需再移动</returns>
        protected abstract bool OnMoveItem(Direction direction, Vector3[] viewportCorners, Vector3[] itemCorners);

        protected LinkedListNode<RectTransform> GetChild(int index)
        {
            if (index >= _children.Count / 2)
            {
                index = _children.Count - 1 - index;
                LinkedListNode<RectTransform> node = _children.Last;
                for (int i = 0; i < index; i++)
                {
                    node = node.Previous;
                }

                return node;
            }
            else
            {
                LinkedListNode<RectTransform> node = _children.First;
                for (int i = 0; i < index; i++)
                {
                    node = node.Next;
                }

                return node;
            }
        }


        protected void SetAsLastSibling(LinkedListNode<RectTransform> child)
        {
            _children.Remove(child);
            _children.AddAfter(_children.Last, child);
        }

        protected void SetAsFirstSibling(LinkedListNode<RectTransform> child)
        {
            _children.Remove(child);
            _children.AddBefore(_children.First, child);
        }

        protected enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }
    }
}