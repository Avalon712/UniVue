using UnityEngine;
using UniVue.Model;
using static UnityEngine.UI.ScrollRect;

namespace UniVue.Widgets
{
    /// <summary>
    /// 循环列表组件
    /// </summary>
    /// <remarks>每个Item都是一个ViewObject对象</remarks>
    [AddComponentMenu("UniVue/LoopList")]
    public sealed class LoopList : Loopable
    {
        #region Unity - 组件配置参数
        [SerializeField, Tooltip("可以看见的Item的最大数量")]
        private int _viewCount;
        [SerializeField, Tooltip("相连两个item在滚动方向上的距离 垂直方向滚动:up.y-down.y  水平方向滚动:right.x-left.x")]
        private float _distance;
        [SerializeField]
        private bool _unlimitScroll;
        [SerializeField]
        private bool _alwaysShowNewestData;
        [SerializeField, Tooltip("每滚动一个Item的距离使用的时间(秒)")]
        private float _perItemScrollTime = 0.1f;
        #endregion

        private int _targetIndex = -1; //要滚动到指定数据的目标位置
        private Vector2 _velocity;     //滚动速度

        /// <summary>
        /// 相连两个Item之间的位置差
        /// </summary>
        private Vector2 deltaPos => _direction == ScrollDirection.Vertical ?
            new Vector2(0, _distance) : new Vector2(_distance, 0);

        public override int viewCount => _viewCount;

        /// <summary>
        /// 滚动到指定数据的位置
        /// </summary>
        public void ScrollTo(IBindableModel data)
        {
            _targetIndex = _data.IndexOf(data);
            if (_targetIndex != -1 && _head != _targetIndex)
            {
                int flag = _direction == ScrollDirection.Horizontal ? -1 : 1;
                Vector3 startPos = _scrollRect.content.anchoredPosition;
                if (flag == 1)
                    startPos.y = (_head * deltaPos).y;
                else
                    startPos.x = (_head * deltaPos).x * flag;

                Vector3 sumDeltaPos = (_targetIndex - _head) * deltaPos * flag;
                Vector3 endPos = startPos + sumDeltaPos;

                //计算缓动时间
                float duration = _direction == ScrollDirection.Vertical ?
                   Mathf.Abs(sumDeltaPos.y / _distance) * _perItemScrollTime :
                   Mathf.Abs(sumDeltaPos.x / _distance) * _perItemScrollTime;

                _velocity = (endPos - startPos) / duration;
            }
        }


        private void FixedUpdate()
        {
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

            if (force || _alwaysShowNewestData)
            {
                if (_data.Count > _viewCount)
                    DoRefresh_ShowLast();
                else
                    ForceRefresh();
            }
            else
            {
                RefreshViewArea();
            }
        }

        protected override void Resize()
        {
            //会触发OnValueChanged事件
            _scrollRect.content.sizeDelta = deltaPos * _data.Count;
            if (_data.Count <= _viewCount)
                _scrollRect.movementType = MovementType.Clamped;
            else
                _scrollRect.movementType = _unlimitScroll ? MovementType.Unrestricted : MovementType.Elastic;
        }

        /// <summary>
        /// 刷新时总是显示最后那一个数据
        /// </summary>
        private void DoRefresh_ShowLast()
        {
            int startPtr = _data.Count - _viewCount;
            _head = startPtr < 0 ? 0 : startPtr;
            _scrollRect.normalizedPosition = _direction == ScrollDirection.Vertical ? Vector2.zero : Vector2.right;
            int singal = _direction == ScrollDirection.Vertical ? 1 : -1;
            ResetItemPos(Vector2.zero - singal * _head * deltaPos);
            RefreshViewArea();
        }

        protected override void ResetItemPos(Vector2 firstPos)
        {
            int singal = _direction == ScrollDirection.Vertical ? 1 : -1;
            Transform content = _scrollRect.content;
            Vector2 deltaPos = this.deltaPos;
            int childCount = content.childCount;
            for (int i = 0; i < childCount; i++)
            {
                (content.GetChild(i) as RectTransform).anchoredPosition = firstPos - singal * i * deltaPos;
            }
        }

        #region 滚动算法实现

        protected override void OnScroll(Vector2 pos)
        {
            Transform content = _scrollRect.content;

            //计算出视口区域的四个角
            _scrollRect.viewport.GetWorldCorners(_viewportCorners);

            //只需监听第一个和倒数第二个
            if (_direction == ScrollDirection.Vertical)
            {
                float ySpeed = _scrollRect.velocity.y;
                (content.GetChild(ySpeed > 0 ? 0 : content.childCount - 2) as RectTransform).GetWorldCorners(_itemCorners);
                VerticalMovement(_scrollRect.velocity.y, content, _viewportCorners, _itemCorners);
            }
            else
            {
                float xSpeed = _scrollRect.velocity.x;
                (content.GetChild(xSpeed < 0 ? 0 : content.childCount - 2) as RectTransform).GetWorldCorners(_itemCorners);
                HorizontalMovement(_scrollRect.velocity.x, content, _viewportCorners, _itemCorners);
            }
        }

        /// <summary>
        ///监听垂直方向的滚动（ 左下[0] 左上[1] 右上[2] 右下[3]）
        /// </summary>
        private void VerticalMovement(float ySpeed, Transform contentTrans, Vector3[] viewportCorners, Vector3[] corners)
        {
            int childCount = contentTrans.childCount;
            int dataCount = _data.Count;
            RectTransform itemTrans = null;
            int index = -1;

            //向上滑动只监听头部
            if (ySpeed > 0 && (_tail == dataCount - 1 ? _unlimitScroll : true) && corners[0].y > viewportCorners[1].y)
            {
                itemTrans = contentTrans.GetChild(0) as RectTransform;
                itemTrans.anchoredPosition = (contentTrans.GetChild(childCount - 1) as RectTransform).anchoredPosition - deltaPos;
                itemTrans.SetAsLastSibling(); //设置为最后一个位置

                _tail = (_tail + 1) % dataCount;
                _head = (_head + 1) % dataCount;
                index = _tail;
            }
            //向下滑动监听倒数第二个（因为实例化的数量=可视数+1）
            else if (ySpeed < 0 && (_head == 0 ? _unlimitScroll : true) && corners[0].y < viewportCorners[0].y)
            {
                itemTrans = contentTrans.GetChild(childCount - 1) as RectTransform;
                itemTrans.anchoredPosition = (contentTrans.GetChild(0) as RectTransform).anchoredPosition + deltaPos;
                itemTrans.SetAsFirstSibling(); //最后一个设置为第一个位置

                _tail = (_tail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;
                index = _head;
            }

            if (index != -1)
            {
                WillBindModel(Vue.Router.GetView(itemTrans.name), index);
            }
        }

        /// <summary>
        /// 水平滚动
        /// </summary>
        private void HorizontalMovement(float xSpeed, Transform contentTrans, Vector3[] viewportCorners, Vector3[] corners)
        {
            int childCount = contentTrans.childCount;
            int dataCount = _data.Count;

            RectTransform itemTrans = null;
            int index = -1;

            //向左滑动只监听最左边那个
            if (xSpeed < 0 && (_tail == dataCount - 1 ? _unlimitScroll : true) && corners[3].x < viewportCorners[1].x) //向左移动了一个Item的距离
            {
                itemTrans = contentTrans.GetChild(0) as RectTransform;
                itemTrans.anchoredPosition = (contentTrans.GetChild(childCount - 1) as RectTransform).anchoredPosition + deltaPos;
                itemTrans.SetAsLastSibling(); //设置为最后一个位置

                _tail = (_tail + 1) % dataCount;
                _head = (_head + 1) % dataCount;
                index = _tail;
            }
            //向右滑动监听倒数第二个（因为实例化的数量=可视数+1）
            else if (xSpeed > 0 && (_head == 0 ? _unlimitScroll : true) && corners[3].x > viewportCorners[3].x) //正在在向右移动
            {
                itemTrans = contentTrans.GetChild(childCount - 1) as RectTransform;
                itemTrans.anchoredPosition = (contentTrans.GetChild(0) as RectTransform).anchoredPosition - deltaPos;
                itemTrans.SetAsFirstSibling(); //最后一个设置为第一个位置

                _tail = (_tail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;
                index = _head;
            }

            if (index != -1)
            {
                WillBindModel(Vue.Router.GetView(itemTrans.name), index);
            }
        }


        #endregion
    }
}
