using UnityEngine;
using UniVue.View;
using static UnityEngine.UI.ScrollRect;

namespace UniVue.Widgets
{
    /// <summary>
    /// 循环网格组件
    /// </summary>
    /// <remarks>每个Item都是一个ViewObject对象</remarks>
    [AddComponentMenu("UniVue/LoopGrid")]
    public sealed class LoopGrid : Loopable
    {
        #region Unity - 参数配置
        [SerializeField, Tooltip("网格可见的视图行数")]
        private int _rows;
        [SerializeField, Tooltip("网格可见的视图列数")]
        private int _cols;
        [SerializeField, Tooltip("right.x-left.x")]
        private float _xDeltaPos;
        [SerializeField, Tooltip("down.y-up.y")]
        private float _yDeltaPos;
        #endregion

        public override int viewCount => _rows * _cols;

        public override void Refresh(bool force = false)
        {
            //重新计算Content的大小
            Resize();
            if (force)
                ForceRefresh();
            else
                RefreshViewArea();
        }

        protected override void Resize()
        {
            Vector3 deltaPos = _direction == ScrollDirection.Vertical ? new Vector2(0, Mathf.Abs(_yDeltaPos)) : new Vector2(Mathf.Abs(_xDeltaPos), 0);
            float temp = _direction == ScrollDirection.Vertical ? _data.Count / (float)_cols : _data.Count / (float)_rows;
            _scrollRect.content.sizeDelta = (Mathf.FloorToInt(temp) + 1) * deltaPos;

            //当前是否可以移动
            _scrollRect.movementType = _data.Count <= _cols * _rows ? MovementType.Clamped : MovementType.Elastic;
        }

        protected override void ResetItemPos(Vector2 firstItemPos)
        {
            Transform content = _scrollRect.content;
            //按下面的方法确保contant的前面rows个为第一列或前cols个为第一行
            if (_direction == ScrollDirection.Vertical) //垂直滚动时位置按行一行一行的设置
            {
                for (int i = 0; i <= _rows; i++)//垂直滚动多一行
                {
                    for (int j = 0; j < _cols; j++)
                    {
                        (content.GetChild(i * _cols + j) as RectTransform).anchoredPosition = firstItemPos;
                        firstItemPos.x += _xDeltaPos;
                    }
                    firstItemPos.y += _yDeltaPos; //下一行
                    firstItemPos.x -= _xDeltaPos * _cols;
                }
            }
            else//水平滚动时位置按列一列一列的设置
            {
                for (int i = 0; i <= _cols; ++i)//水平滚动多一列
                {
                    for (int j = 0; j < _rows; ++j)
                    {
                        (content.GetChild(i * _rows + j) as RectTransform).anchoredPosition = firstItemPos;
                        firstItemPos.y += _yDeltaPos;
                    }
                    firstItemPos.x += _xDeltaPos; //下一列
                    firstItemPos.y -= _yDeltaPos * _rows;
                }
            }
        }

        #region 滚动算法的实现

        protected override void OnScroll(Vector2 pos)
        {
            RectTransform viewport = _scrollRect.viewport;
            Transform content = _scrollRect.content;

            //计算可视域的边界
            viewport.GetWorldCorners(_viewportCorners);

            if (_direction == ScrollDirection.Vertical)
            {
                float ySpeed = _scrollRect.velocity.y; //y > 0 向上滑动
                int lastRowFirstIdx = _rows * _cols; //最后一行的第一个索引
                int lastFirst = lastRowFirstIdx - _cols; //倒数第二行的第一个索引

                content.GetChild(ySpeed > 0 ? 0 : lastFirst).GetComponent<RectTransform>().GetWorldCorners(_itemCorners);

                //监听第一行的第一个以及倒数第二行的第一个
                if (ySpeed > 0 && _itemCorners[0].y > _viewportCorners[1].y && _tail > _head)
                    VercitalMovement0(content, lastRowFirstIdx);
                else if (ySpeed < 0 && _itemCorners[0].y < _viewportCorners[0].y && _head > 0)
                    VercitalMovement1(content, lastRowFirstIdx);
            }
            else
            {
                float xSpeed = _scrollRect.velocity.x; // xSpeed < 0 左滑动
                int lastColFirstIdx = _cols * _rows; //最后一列的第一个索引
                int lastFirst = lastColFirstIdx - _rows; //倒数第二列的第一个索引

                content.GetChild(xSpeed < 0 ? 0 : lastFirst).GetComponent<RectTransform>().GetWorldCorners(_itemCorners);

                //监听第一列的第一个和倒数第二列的第一个
                if (xSpeed < 0 && _itemCorners[2].x < _viewportCorners[1].x && _tail > _head)
                    HorizontalMovement0(content, lastColFirstIdx);
                else if (xSpeed > 0 && _itemCorners[3].x > _viewportCorners[3].x && _head > 0)
                    HorizontalMovement1(content, lastColFirstIdx);
            }
        }

        /// <summary>
        /// 将第一行的所有Item移动到最后一行
        /// </summary>
        private void VercitalMovement0(Transform content, int lastRowFirstIdx)
        {
            int dataCount = _data.Count;
            ViewRouter router = Vue.Router;
            for (int i = 0; i < _cols; i++)
            {
                Vector2 pos = (content.GetChild(lastRowFirstIdx + i) as RectTransform).anchoredPosition;
                pos.y += _yDeltaPos;

                RectTransform itemTrans = content.GetChild(i) as RectTransform;
                itemTrans.anchoredPosition = pos; //位置修改

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + 1) % dataCount;
                _head = (_head + 1) % dataCount;

                IView view = router.GetView(itemTrans.name);
                if (_tail >= _head)
                    WillBindModel(view, _tail);
                else
                    view.Close();
            }

            for (int i = 0; i < _cols; i++)
            {
                content.GetChild(0).SetAsLastSibling(); //逻辑位置修改
            }
        }

        /// <summary>
        /// 将最后一行移动到第一行
        /// </summary>
        private void VercitalMovement1(Transform content, int lastRowFirstIdx)
        {
            int dataCount = _data.Count;
            ViewRouter router = Vue.Router;
            for (int i = _cols - 1; i >= 0; i--) //保证数据显示的顺序正确性
            {
                Vector2 pos = (content.GetChild(i) as RectTransform).anchoredPosition;
                pos.y -= _yDeltaPos;
                RectTransform itemTrans = content.GetChild(lastRowFirstIdx + i) as RectTransform;
                itemTrans.anchoredPosition = pos; //位置修改

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;

                //向下滑动全部显示
                IView view = router.GetView(itemTrans.name);
                WillBindModel(view, _head);
            }

            int lastIdx = content.childCount - 1;
            for (int i = _cols - 1; i >= 0; i--)
            {
                content.GetChild(lastIdx).SetAsFirstSibling(); //逻辑位置修改
            }
        }

        /// <summary>
        /// 将第一列全部移动到最后一列
        /// </summary>
        private void HorizontalMovement0(Transform content, int lastColFirstIdx)
        {
            //向左滑动了一个Item的距离
            int dataCount = _data.Count;
            ViewRouter router = Vue.Router;
            for (int i = 0; i < _rows; i++)
            {
                Vector2 pos = (content.GetChild(lastColFirstIdx + i) as RectTransform).anchoredPosition;
                pos.x += _xDeltaPos;
                RectTransform itemTrans = content.GetChild(i) as RectTransform;
                itemTrans.anchoredPosition = pos; //位置改变

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + 1) % dataCount;
                _head = (_head + 1) % dataCount;

                IView view = router.GetView(itemTrans.name);
                if (_tail >= _head)
                    WillBindModel(view, _tail);
                else
                    view.Close();
            }

            for (int i = 0; i < _rows; i++)
            {
                content.GetChild(0).SetAsLastSibling(); //逻辑位置的改变
            }
        }

        /// <summary>
        /// 将最后一列全部移动到第一列 →
        /// </summary>
        private void HorizontalMovement1(Transform content, int lastColFirstIdx)
        {
            //当前正在向右滑动
            int dataCount = _data.Count;
            ViewRouter router = Vue.Router;
            for (int i = _rows - 1; i >= 0; i--)
            {
                Vector2 pos = (content.GetChild(i) as RectTransform).anchoredPosition;
                pos.x -= _xDeltaPos;
                RectTransform itemTrans = content.GetChild(lastColFirstIdx + i) as RectTransform;
                itemTrans.anchoredPosition = pos; //位置的改变

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;

                //向右滑动全部显示
                IView view = router.GetView(itemTrans.name);
                WillBindModel(view, _head);
            }

            int lastIdx = content.childCount - 1;
            for (int i = _rows - 1; i >= 0; i--)
            {
                content.GetChild(lastIdx).SetAsFirstSibling(); //逻辑位置修改
            }
        }

        #endregion
    }
}
