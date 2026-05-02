using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.ScrollRect;

namespace UniVue.UI.Widgets
{
    [DontGenUICode(Code = UIGenCode.Class)]
    [AddComponentMenu("UniVue/UI Widgets/LoopList")]
    public sealed class LoopList : Loopable
    {
        [SerializeField]
        private bool _unlimitScroll;

        [SerializeField]
        private bool _alwaysNewest;

        [SerializeField]
        private float _perItemScrollTime = 0.1f;

        private int _targetIndex = -1; //要滚动到指定数据的目标位置
        private Vector2 _velocity; //滚动速度

        private int VisibleCount => scrollDir == ScrollDirection.Vertical ? grid.y : grid.x;

        protected override int LastIndex => ChildCount - 2;

        /// <summary>
        /// 滚动到指定数据的位置
        /// </summary>
        public void ScrollTo(int index)
        {
            _targetIndex = index;
            if (_targetIndex != -1 && _head != _targetIndex)
            {
                int flag = scrollDir == ScrollDirection.Horizontal ? -1 : 1;
                Vector3 startPos = _scrollRect.content.anchoredPosition;
                if (flag == 1)
                    startPos.y = (_head * Distance).y;
                else
                    startPos.x = (_head * Distance).x * flag;

                Vector3 sumDeltaPos = Distance * ((_targetIndex - _head) * flag);
                Vector3 endPos = startPos + sumDeltaPos;

                //计算缓动时间
                float duration = scrollDir == ScrollDirection.Vertical
                    ? Mathf.Abs(sumDeltaPos.y / Distance.y) * _perItemScrollTime
                    : Mathf.Abs(sumDeltaPos.x / Distance.x) * _perItemScrollTime;

                _velocity = (endPos - startPos) / duration;
            }
        }

        protected override void OnUpdate(in float deltaTime)
        {
            base.OnUpdate(in deltaTime);
            //滚动动画的实现
            if (_targetIndex != -1)
            {
                if (_targetIndex >= _head && _targetIndex <= _tail)
                {
                    _velocity = Vector2.zero;
                    _targetIndex = -1;
                }

                _scrollRect.velocity = _velocity;
            }
        }

        public override void Refresh(bool force = false)
        {
            //重新计算Content的大小
            Resize();

            if (force)
            {
                ForceRefresh();
            }
            else
            {
                if (_alwaysNewest && Count >= VisibleCount)
                    DoRefreshShowLast();
                else
                    RefreshViewArea();
            }
        }

        protected override void Resize()
        {
            base.Resize();
            //当前是否可以移动
            _scrollRect.movementType = Count <= gap.x * gap.y
                ? MovementType.Clamped
                : _unlimitScroll
                    ? MovementType.Unrestricted
                    : MovementType.Elastic;
        }

        /// <summary>
        /// 刷新时总是显示最后那一个数据
        /// </summary>
        private void DoRefreshShowLast()
        {
            int startPtr = Count - VisibleCount;
            _head = startPtr < 0 ? 0 : startPtr;
            _scrollRect.normalizedPosition = scrollDir == ScrollDirection.Vertical ? Vector2.zero : Vector2.right;
            // int singal = direction == ScrollDirection.Vertical ? 1 : -1;
            // ResetItemPos(Vector2.zero - singal * _head * Distance);
            // RefreshViewArea();
        }


#region 滚动算法实现

        protected override bool OnMoveItem(Direction direction, Vector3[] viewportCorners, Vector3[] itemCorners)
        {
            int dataCount = Count;
            LinkedListNode<RectTransform> itemTrans = null;
            int index = -1;

            switch (direction)
            {
                case Direction.Up:
                    if ((_tail != Count - 1 || _unlimitScroll) && itemCorners[0].y > viewportCorners[1].y)
                    {
                        itemTrans = FirstChild;
                        itemTrans.Value.anchoredPosition = LastChild.Value.anchoredPosition - Distance;
                        SetAsLastSibling(itemTrans); //设置为最后一个位置

                        _tail = (_tail + 1) % dataCount;
                        _head = (_head + 1) % dataCount;
                        index = _tail;
                    }

                    break;
                case Direction.Down:
                    if ((_head != 0 || _unlimitScroll) && itemCorners[0].y < viewportCorners[0].y)
                    {
                        itemTrans = LastChild;
                        itemTrans.Value.anchoredPosition = FirstChild.Value.anchoredPosition + Distance;
                        SetAsFirstSibling(itemTrans); //最后一个设置为第一个位置

                        _tail = (_tail + dataCount - 1) % dataCount;
                        _head = (_head + dataCount - 1) % dataCount;
                        index = _head;
                    }

                    break;
                case Direction.Left:
                    if ((_tail != dataCount - 1 || _unlimitScroll) && itemCorners[3].x < viewportCorners[1].x)
                    {
                        itemTrans = FirstChild;
                        itemTrans.Value.anchoredPosition = LastChild.Value.anchoredPosition + Distance;
                        SetAsLastSibling(itemTrans); //设置为最后一个位置

                        _tail = (_tail + 1) % dataCount;
                        _head = (_head + 1) % dataCount;
                        index = _tail;
                    }

                    break;
                case Direction.Right:
                    if ((_head != 0 || _unlimitScroll) && itemCorners[3].x > viewportCorners[3].x)
                    {
                        itemTrans = LastChild;
                        itemTrans.Value.anchoredPosition = FirstChild.Value.anchoredPosition - Distance;
                        SetAsFirstSibling(itemTrans); //最后一个设置为第一个位置

                        _tail = (_tail + dataCount - 1) % dataCount;
                        _head = (_head + dataCount - 1) % dataCount;
                        index = _head;
                    }

                    break;
            }

            if (index != -1 && itemTrans != null)
            {
                LoopItem item = itemTrans.Value.GetComponent<LoopItem>();
                OnItemRender?.Invoke(index, item);
            }

            return index != -1;
        }

#endregion
    }
}