using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Tween;
using UniVue.Utils;
using static UnityEngine.UI.ScrollRect;

namespace UniVue.View.Views
{
    [Serializable]
    public sealed class ListWidget : IWidget
    {
        [SerializeField]
        private Direction _scrollDir;           //滚动方向
        [SerializeField]
        private int _viewCount;                 //可见的数量
        [SerializeField]
        private float _distance;                //相连两个item在滚动方向上的距离
        [SerializeField]
        private ScrollRect _scrollRect;         //必须的滚动组件
        private Vector3 _deltaPos;              //相连两个Item直接的位置差
        private List<IBindableModel> _models;   //绑定的数据
        private int _tail;                      //数据尾指针
        private int _head;                      //数据头指针
        private bool _dirty;                    //当前数据是否已经发送修改
        private bool _flag;                     //是否已经生成UIBundle对象
        private bool _loop;                     //是否循环滚动

        /// <summary>
        /// 是否循环滚动
        /// </summary>
        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                if (value == true)
                {
                    _scrollRect.movementType = MovementType.Unrestricted;
                }
            }
        }

        /// <summary>
        /// 构建List视图组件
        /// </summary>
        /// <param name="scrollRect">滚动组件</param>
        /// <param name="distance">
        /// 相连两个item在滚动方向上的距离
        /// <para>垂直方向滚动: </para>
        /// <para>upItem.localPos.y-downItem.localPos.y </para>   
        /// <para>水平方向滚动:</para>
        /// <para>rightItem.localPos.x-leftItem.localPos.x</para> 
        /// </param>
        /// <param name="viewNum">可见数量</param>
        /// <param name="scrollDir">滚动方向</param>
        /// <param name="loop">是否循环滚动</param>
        public ListWidget(ScrollRect scrollRect, float distance,int viewNum, Direction scrollDir = Direction.Vertical, bool loop = false)
        {
            _scrollDir = scrollDir;
            _distance = distance;
            _viewCount = viewNum;
            _scrollRect = scrollRect;
            _loop = loop;
            scrollRect.movementType = loop ? MovementType.Unrestricted : MovementType.Elastic;
            scrollRect.horizontal = scrollDir == Direction.Horizontal;
            scrollRect.vertical = scrollDir == Direction.Vertical;
            _deltaPos = scrollDir == Direction.Vertical ? new Vector3(0, distance, 0) : new Vector3(distance, 0, 0);
        }

        /// <summary>
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindData<T>(List<T> newData) where T : IBindableModel
        {
            if (_models == null)
            {
                BindData(newData);
            }
            else if (!ReferenceEquals(_models,newData))
            {
                ListUtil.Copy(_models, newData);
                Refresh(); //刷新数据
            }
        }

        /// <summary>
        /// 为Item绑定显示数据
        /// </summary>
        /// <param name="data">绑定的数据</param>
        public void BindData<T>(List<T> data) where T : IBindableModel
        {
            if (_models != null)
            {
#if UNITY_EDITOR
                LogUtil.Warning("ListView已经绑定了List数据，不能再进行绑定");
#endif
                return;
            }

            _models = new List<IBindableModel>(data.Count);
            for (int i = 0; i < data.Count; i++) { _models.Add(data[i]); }

            //创建Item
            CreateItems();

            int len = _scrollRect.content.childCount;

            //绑定Item数据
            for (int i = 0; i < len; i++)
            {
                RectTransform itemRectTrans = _scrollRect.content.GetChild(i).GetComponent<RectTransform>();

                FlexibleView dynamicView = new FlexibleView(itemRectTrans.gameObject, null, ViewLevel.Permanent);

                //数据渲染
                if (_tail < data.Count)
                {
                    dynamicView.BindModel(_models[_tail++]);
                }
                else
                {
                    //这一步是为了生成UIBundle，以此使用RebindModel()函数
                    if (data.Count > 0) { dynamicView.BindModel(_models[0]); _flag = true; }
                    itemRectTrans.gameObject.SetActive(false);
                }
            }

            //绑定滑动事件
            BindScrollEvt();

            --_tail; //恢复正确的位置

        }

        /// <summary>
        /// 对列表进行排序，排序规则
        /// </summary>
        /// <param name="comparer">排序规则</param>
        public void Sort(Comparison<IBindableModel> comparer)
        {
            _dirty = true;
            _models.Sort(comparer);
            Refresh(); //刷新
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="newData">新加入的数据</param>
        public void AddData<T>(T newData) where T : IBindableModel
        {
            //如果当前所有的Item都还没有生成UIBundle则先生成UIBundle
            if (!_flag)
            {
                _flag = true;
                Transform content = _scrollRect.content;
                for (int i = 0; i < content.childCount; i++)
                {
                    Vue.Router.GetView(content.GetChild(i).name).BindModel(newData);
                }
            }

            if (!_models.Contains(newData))
            {
                _models.Add(newData);
                Resize(); //重新计算content的大小
                _dirty = true;
                Refresh();
            }
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        public void RemoveData<T>(T remove) where T : IBindableModel
        {
            if (remove != null && _models.Contains(remove))
            {
                ListUtil.TrailDelete(_models, _models.IndexOf(remove));
                Resize(); //重新计算content的大小
                _dirty = true;
                Refresh();
            }
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            _models.Clear();
            Refresh();
        }

        /// <summary>
        /// 滚动到指定数据的哪儿
        /// </summary>
        public void ScrollTo<T>(T data) where T : IBindableModel
        {
            int index = _models.IndexOf(data);
            if (index<= _models.Count && index>=0 && _head != index)
            {
                int flag = _scrollDir == Direction.Horizontal ? -1 : 1;
                Vector3 startPos = _scrollRect.content.localPosition;
                if(flag == 1)
                    startPos.y = (_head * _deltaPos).y;
                else
                    startPos.x = (_head * _deltaPos).x * flag;

                //减一的原因是为了减少偏差
                Vector3 sumDeltaPos = (index - _head) * _deltaPos * flag;
                Vector3 endPos = startPos + sumDeltaPos; 

                //计算缓动时间
                float duration = _scrollDir == Direction.Vertical ?
                   Mathf.Abs(sumDeltaPos.y / _distance) * 0.1f :
                   Mathf.Abs(sumDeltaPos.x / _distance) * 0.1f;

                TweenBehavior.DoLocalMove(_scrollRect.content, duration, endPos);
            }
        }

        /// <summary>
        /// 刷新视图
        /// </summary>
        public void Refresh()
        {
            Vector2 endPos = _scrollDir == Direction.Vertical ? Vector2.up : Vector2.zero;

            //均匀滚动
            Vector3 localPos = _scrollRect.content.localPosition;
            float duration = _scrollDir == Direction.Vertical ?
                   Mathf.Abs(localPos.y / _distance) * 0.1f :
                   Mathf.Abs(localPos.x / _distance) * 0.1f;

            TweenBehavior.DoScroll(_scrollRect, duration, endPos).Call(DoRefresh);
        }

        private void DoRefresh()
        {
            _head = 0;
            _tail = 0;
            Transform content = _scrollRect.content;
            int len = content.childCount;
            for (int i = 0; i < len; i++)
            {
                GameObject itemObj = content.GetChild(i).gameObject;
                if (_tail < _models.Count)
                {
                    if (!itemObj.activeSelf)
                    {
                        itemObj.SetActive(true);
                    }

                    Rebind(itemObj.name, _models[_tail++]);
                }
                else
                {
                    itemObj.SetActive(false);
                }
            }
            --_tail;
            _dirty = false;
        }

        private void CreateItems()
        {
            Resize(); //重新计算content的大小

            int singal = _scrollDir == Direction.Vertical ? 1 : -1;
            Transform content = _scrollRect.content;
            Vector3 startPos = content.GetChild(0).localPosition;
            GameObject firstItem = content.GetChild(0).gameObject;

            Vector3 deltaPos = this._deltaPos;
            //创建Item
            for (int i = 1; i <= _viewCount; i++)
            {
                GameObject itemViewObject = PrefabCloneUtil.RectTransformClone(firstItem, content);
                itemViewObject.name += i;
                itemViewObject.transform.localPosition = startPos - singal * i * deltaPos;
            }
        }

        private void BindScrollEvt()
        {
            //监听边界
            Vector3[] corners0 = new Vector3[4];
            Vector3[] corners1 = new Vector3[4];
            Vector3[] viewportCorners = new Vector3[4];

            ScrollRect scrollRect = this._scrollRect;

            if (_scrollDir == Direction.Vertical)
            {
                scrollRect.onValueChanged.AddListener((v2) =>
                {
                    //计算出视口区域的四个角
                    scrollRect.GetComponent<RectTransform>().GetWorldCorners(viewportCorners);
                    Transform content = scrollRect.content;
                    content.GetChild(0).GetComponent<RectTransform>().GetWorldCorners(corners0);
                    content.GetChild(content.childCount - 2).GetComponent<RectTransform>().GetWorldCorners(corners1);
                    //只需监听第一个和倒数第二个
                    VerticalListenCorners(content, viewportCorners, corners0, corners1);
                });
            }
            else
            {
                scrollRect.onValueChanged.AddListener((v2) =>
                {
                    //计算出视口区域的四个角
                    scrollRect.GetComponent<RectTransform>().GetWorldCorners(viewportCorners);
                    Transform content = scrollRect.content;
                    content.GetChild(0).GetComponent<RectTransform>().GetWorldCorners(corners0);
                    content.GetChild(content.childCount - 2).GetComponent<RectTransform>().GetWorldCorners(corners1);
                    //只需监听第一个和倒数第二个
                    HorizontalListenCorners(content, viewportCorners, corners0, corners1);
                });
            }
        }

        private void Rebind(string itemName, IBindableModel model)
        {
            Vue.Router.GetView(itemName).RebindModel(model);
            model.NotifyAll();
        }

        /// <summary>
        /// 重新计算Content区域的大小
        /// </summary>
        private void Resize()
        {
            _scrollRect.content.sizeDelta = _deltaPos * _models.Count;

            _scrollRect.movementType = _models.Count < _viewCount ? MovementType.Clamped : MovementType.Elastic;
        }

        public void Destroy()
        {
            _scrollRect.onValueChanged.RemoveAllListeners();
            _scrollRect = null;
            _models?.Clear();
            _models = null;
        }

        #region 滚动算法实现

        /// <summary>
        ///监听垂直方向的滚动（ 左下[0] 左上[1] 右上[2] 右下[3]）
        /// </summary>
        private void VerticalListenCorners(Transform contentTrans, Vector3[] viewportCorners, Vector3[] corners0, Vector3[] corners1)
        {
            List<IBindableModel> data = _models;
            int childCount = contentTrans.childCount;
            int dataCount = data.Count;

            //向上移动只监听头部
            if ((_tail == dataCount - 1 ? _loop : true) && corners0[0].y > viewportCorners[1].y)
            {
                Transform itemTrans = contentTrans.GetChild(0);
                itemTrans.localPosition = contentTrans.GetChild(childCount - 1).localPosition - _deltaPos;
                itemTrans.SetAsLastSibling(); //设置为最后一个位置

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + 1) % dataCount;
                _head = (_head + 1) % dataCount;
                //当前数据不是脏数据才进行重新渲染
                if (!_dirty) { Rebind(itemTrans.name, data[_tail]); }
            }
            //向下移动监听倒数第二个（因为实例化的数量=可视数+1）
            else if ((_head == 0 ? _loop : true) && corners1[0].y < viewportCorners[0].y)
            {
                Transform lastItemTrans = contentTrans.GetChild(childCount - 1);
                lastItemTrans.localPosition = contentTrans.GetChild(0).localPosition + _deltaPos;
                lastItemTrans.SetAsFirstSibling(); //最后一个设置为第一个位置

                //渲染数据 先计算指针再渲染数据 不写if语句！！！
                _tail = (_tail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;
                //当前数据不是脏数据才进行重新渲染
                if (!_dirty) { Rebind(lastItemTrans.name, data[_head]); }
            }
        }

        /// <summary>
        /// 水平滚动
        /// </summary>
        private void HorizontalListenCorners(Transform contentTrans, Vector3[] viewportCorners, Vector3[] corners0, Vector3[] corners1)
        {
            List<IBindableModel> data = _models;
            int childCount = contentTrans.childCount;
            int dataCount = data.Count;

            //向左移动只监听最左边那个
            if ((_tail == dataCount - 1 ? _loop : true) && corners0[3].x < viewportCorners[1].x) //向左移动了一个Item的距离
            {
                Transform itemTrans = contentTrans.GetChild(0);
                itemTrans.localPosition = contentTrans.GetChild(childCount - 1).localPosition + _deltaPos;
                itemTrans.SetAsLastSibling(); //设置为最后一个位置

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + 1) % dataCount;
                _head = (_head + 1) % dataCount;
                //当前数据不是脏数据才进行重新渲染
                if (!_dirty) { Rebind(itemTrans.name, data[_tail]); }
            }
            //向下移动监听倒数第二个（因为实例化的数量=可视数+1）
            else if ((_head == 0 ? _loop : true) && corners1[3].x > viewportCorners[3].x) //正在在向右移动
            {
                Transform lastItemTrans = contentTrans.GetChild(childCount - 1);
                lastItemTrans.localPosition = contentTrans.GetChild(0).localPosition - _deltaPos;
                lastItemTrans.SetAsFirstSibling(); //最后一个设置为第一个位置

                //渲染数据 先计算指针再渲染数据 不写if语句！！！
                _tail = (_tail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;
                //当前数据不是脏数据才进行重新渲染
                if (!_dirty) { Rebind(lastItemTrans.name, data[_head]); }
            }
        }


        #endregion
    }
}
