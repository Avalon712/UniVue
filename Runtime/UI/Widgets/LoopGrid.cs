using System.Collections.Generic;
using UnityEngine;

namespace UniVue.UI.Widgets
{
    [DontGenUICode(Code = UIGenCode.Class)]
    [AddComponentMenu("UniVue/UI Widgets/LoopGrid")]
    public sealed class LoopGrid : Loopable
    {
        protected override int LastIndex
        {
            get
            {
                int rows = ViewRows;
                int cols = ViewCols;
                if (scrollDir == ScrollDirection.Vertical)
                    return rows * cols - cols; //倒数第二行的第一个索引
                return cols * rows - rows; //倒数第二列的第一个索引
            }
        }

        private int ViewCols => grid.x - (scrollDir == ScrollDirection.Vertical ? 0 : 1);

        private int ViewRows => grid.y - (scrollDir == ScrollDirection.Vertical ? 1 : 0);

        public override void Refresh(bool force = false)
        {
            //重新计算Content的大小
            Resize();
            if (force)
                ForceRefresh();
            else
                RefreshViewArea();
        }

#region 滚动算法的实现

        protected override bool OnMoveItem(Direction direction, Vector3[] viewportCorners, Vector3[] itemCorners)
        {
            int lastRowFirstIdx = ViewCols * ViewRows; //最后一行的第一个索引
            int lastColFirstIdx = lastRowFirstIdx; //最后一列的第一个索引

            switch (direction)
            {
                case Direction.Up:
                    if (itemCorners[0].y > viewportCorners[1].y && _tail > _head)
                        return VerticalMovement0(lastRowFirstIdx);
                    break;
                case Direction.Down:
                    if (itemCorners[0].y < viewportCorners[0].y && _head > 0)
                        return VerticalMovement1(lastRowFirstIdx);
                    break;
                case Direction.Left:
                    if (itemCorners[3].x < viewportCorners[1].x && _tail > _head)
                        return HorizontalMovement0(lastColFirstIdx);
                    break;
                case Direction.Right:
                    if (itemCorners[3].x > viewportCorners[3].x && _head > 0)
                        return HorizontalMovement1(lastColFirstIdx);
                    break;
            }

            return false;
        }

        /// <summary>
        /// 将第一行的所有Item移动到最后一行
        /// </summary>
        private bool VerticalMovement0(int lastRowFirstIdx)
        {
            int dataCount = Count;
            int cols = ViewCols;
            LinkedListNode<RectTransform> node = GetChild(lastRowFirstIdx);
            LinkedListNode<RectTransform> node2 = GetChild(0);
            for (int i = 0; i < cols; i++)
            {
                Vector2 pos = node.Value.anchoredPosition;
                pos.y -= Distance.y;

                RectTransform itemTrans = node2.Value;
                itemTrans.anchoredPosition = pos; //位置修改

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + 1) % dataCount;
                _head = (_head + 1) % dataCount;

                LoopItem item = itemTrans.GetComponent<LoopItem>();
                if (_tail >= _head)
                    OnItemRender?.Invoke(_tail, item);
                else
                    item.Hide();

                node = node.Next;
                node2 = node2.Next;
            }

            for (int i = 0; i < cols; i++)
                SetAsLastSibling(FirstChild); //逻辑位置修改
            return true;
        }

        /// <summary>
        /// 将最后一行移动到第一行
        /// </summary>
        private bool VerticalMovement1(int lastRowFirstIdx)
        {
            int dataCount = Count;
            int cols = ViewCols;
            LinkedListNode<RectTransform> node = GetChild(cols - 1);
            LinkedListNode<RectTransform> node2 = GetChild(lastRowFirstIdx + cols - 1);
            for (int i = cols - 1; i >= 0; i--) //保证数据显示的顺序正确性
            {
                Vector2 pos = node.Value.anchoredPosition;
                pos.y += Distance.y;
                RectTransform itemTrans = node2.Value;
                itemTrans.anchoredPosition = pos; //位置修改

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;

                //向下滑动全部显示
                LoopItem item = itemTrans.GetComponent<LoopItem>();
                OnItemRender?.Invoke(_head, item);

                node = node.Previous;
                node2 = node2.Previous;
            }

            for (int i = cols - 1; i >= 0; i--)
                SetAsFirstSibling(LastChild); //逻辑位置修改
            return true;
        }

        /// <summary>
        /// 将第一列全部移动到最后一列
        /// </summary>
        private bool HorizontalMovement0(int lastColFirstIdx)
        {
            //向左滑动了一个Item的距离
            int dataCount = Count;
            int rows = ViewRows;

            LinkedListNode<RectTransform> node = GetChild(lastColFirstIdx);
            LinkedListNode<RectTransform> node2 = GetChild(0);

            for (int i = 0; i < rows; i++)
            {
                Vector2 pos = node.Value.anchoredPosition;
                pos.x += Distance.x;
                RectTransform itemTrans = node2.Value;
                itemTrans.anchoredPosition = pos; //位置改变

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + 1) % dataCount;
                _head = (_head + 1) % dataCount;

                LoopItem item = itemTrans.GetComponent<LoopItem>();
                if (_tail >= _head)
                    OnItemRender?.Invoke(_tail, item);
                else
                    item.Hide();

                node = node.Next;
                node2 = node2.Next;
            }

            for (int i = 0; i < rows; i++)
                SetAsLastSibling(FirstChild); //逻辑位置的改变
            return true;
        }

        /// <summary>
        /// 将最后一列全部移动到第一列 →
        /// </summary>
        private bool HorizontalMovement1(int lastColFirstIdx)
        {
            //当前正在向右滑动
            int dataCount = Count;
            int rows = ViewRows;

            LinkedListNode<RectTransform> node = GetChild(rows - 1);
            LinkedListNode<RectTransform> node2 = GetChild(lastColFirstIdx + rows - 1);

            for (int i = rows - 1; i >= 0; i--)
            {
                Vector2 pos = node.Value.anchoredPosition;
                pos.x -= Distance.x;
                RectTransform itemTrans = node2.Value;
                itemTrans.anchoredPosition = pos; //位置的改变

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;

                //向右滑动全部显示
                LoopItem item = itemTrans.GetComponent<LoopItem>();
                OnItemRender?.Invoke(_head, item);

                node = node.Previous;
                node2 = node2.Previous;
            }

            for (int i = rows - 1; i >= 0; i--)
                SetAsFirstSibling(LastChild); //逻辑位置修改
            return true;
        }

#endregion
    }
}