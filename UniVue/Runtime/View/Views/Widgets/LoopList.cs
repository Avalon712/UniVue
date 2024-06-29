using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Tween;
using UniVue.Utils;
using UniVue.ViewModel;
using static UnityEngine.UI.ScrollRect;

namespace UniVue.View.Widgets
{
    /// <summary>
    /// List高性能滚动组件
    /// </summary>
    /// <remarks>第一个Item的锚点位置必须为左上角，位置为(0,0,0)，否则位置计算可能会出错</remarks>
    public sealed class LoopList : Widget
    {
        private readonly Direction _scrollDir;      //滚动方向
        private readonly int _viewCount;            //可见的数量
        private readonly float _distance;           //相连两个item在滚动方向上的距离
        private ScrollRect _scrollRect;             //必须的滚动组件
        private bool _playScrollEffectOnRefresh;    //当刷新视图时是否播放滚动效果
        private bool _alwaysShowNewestData;         //是否总是显示最新的数据
        private bool _renderModelOnScroll;          //在进行滚动动画时是否重新绑定模型数据（减少数据重新渲染的开销）
        private List<IBindableModel> _models;       //绑定的数据
        private IObservableList _observer;          //绑定的数据 ---> 不会产生数据冗余
        private int _tail;                          //数据尾指针
        private int _head;                          //数据头指针
        private bool _loop;                         //是否循环滚动
        private bool _OnScroll;                     //指示当前是否在进行滚动动画

        private Vector3 deltaPos => _scrollDir == Direction.Vertical ?
            new Vector3(0, _distance, 0) : new Vector3(_distance, 0, 0);

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
        /// 总是显示最新的数据（如果没有最新的数据则是显示最后那个数据）
        /// </summary>
        public bool AlwaysShowNewestData
        {
            get => _alwaysShowNewestData;
            set => _alwaysShowNewestData = value;
        }

        /// <summary>
        /// 当刷新数据时是否播放滚动动画
        /// </summary>
        public bool PlayScrollEffectOnRefresh
        {
            get => _playScrollEffectOnRefresh;
            set => _playScrollEffectOnRefresh = value;
        }

        /// <summary>
        /// 在进行滚动动画时是否重新绑定模型数据（减少数据重新渲染的开销）
        /// </summary>
        public bool RenderModelOnScroll
        {
            get => _renderModelOnScroll;
            set => _renderModelOnScroll = value;
        }

        public int Count => _observer == null ? _models.Count : _observer.Count;

        private IBindableModel this[int index]
        {
            get
            {
                if (_observer != null)
                    return _observer.Get<IBindableModel>(index);
                else
                    return _models[index];
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
        public LoopList(ScrollRect scrollRect, float distance, int viewNum, Direction scrollDir = Direction.Vertical, bool loop = false)
        {
            _scrollDir = scrollDir;
            _distance = distance;
            _viewCount = viewNum;
            _scrollRect = scrollRect;
            _loop = loop;
            _renderModelOnScroll = Vue.Config.RenderModelOnScroll;
            scrollRect.movementType = loop ? MovementType.Unrestricted : MovementType.Elastic;
            scrollRect.horizontal = scrollDir == Direction.Horizontal;
            scrollRect.vertical = scrollDir == Direction.Vertical;
        }

        /// <summary>
        /// 绑定集合
        /// </summary>
        /// <remarks>使用这个函数可以防止数据的冗余。同时能够在数据发生变化时自动更新响应变化</remarks>
        public void BindList(IObservableList observer)
        {
            if (_models != null || _observer != null) { return; }
            _observer = observer;
            BindListeners();
            Init();
        }

        /// <summary>
        /// 绑定集合
        /// </summary>
        /// <remarks>
        /// 使用这个函数可以防止数据的冗余。同时能够在数据发生变化时自动更新响应变化</remarks>
        public void RebindList(IObservableList observer)
        {
            if (_models != null) { return; }
            if (_observer == null)
                BindList(observer);
            else if (!ReferenceEquals(_observer, observer))
            {
                _observer.RemoveListeners(GetHashCode()); //移除上次绑定的数据
                _observer = observer;
                BindListeners();
                Refresh(true); //刷新数据
            }
        }

        /// <summary>
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindList<T>(List<T> newData) where T : IBindableModel
        {
            CheckBindedList();

            if (_models == null)
                BindList(newData);
            else if (!ReferenceEquals(_models, newData))
            {
                ListUtil.Copy(_models, newData);
                Refresh(true); //刷新数据
            }
        }

        /// <summary>
        /// 为Item绑定显示数据
        /// </summary>
        /// <param name="data">绑定的数据</param>
        public void BindList<T>(List<T> data) where T : IBindableModel
        {
            CheckBindedList();
            if (_models != null)
            {
#if UNITY_EDITOR
                LogUtil.Warning("ListView已经绑定了List数据，不能再进行绑定。请使用RebindList()函数进行重绑定");
#endif
                return;
            }

            _models = new List<IBindableModel>(data.Count);
            for (int i = 0; i < data.Count; i++) { _models.Add(data[i]); }
            Init();
        }

        /// <summary>
        /// 对列表进行排序，排序规则
        /// </summary>
        /// <remarks>仅在非IObservableList模式下有效</remarks>
        /// <param name="comparer">排序规则</param>
        public void Sort<T>(Comparison<T> comparer) where T : IBindableModel
        {
            if (_observer == null && _models != null)
            {
                _models.Sort((b1, b2) => comparer((T)b1, (T)b2));
                Refresh(); //刷新
            }
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <remarks>仅在非IObservableList模式下有效</remarks>
        /// <param name="newData">新加入的数据</param>
        public void AddData<T>(T newData) where T : IBindableModel
        {
            if (_observer == null && _models != null && !_models.Contains(newData))
            {
                _models.Add(newData);
                Refresh(Count <= _viewCount + 1);
            }
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <remarks>仅在非IObservableList模式下有效</remarks>
        public void RemoveData<T>(T remove, int index = -1) where T : IBindableModel
        {
            if (_observer == null && remove != null && _models.Contains(remove))
            {
                index = index < 0 ? _models.IndexOf(remove) : index;
                _models.RemoveAt(index);
                Refresh(Count <= _viewCount + 1); //刷新
            }
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        /// <remarks>仅在非IObservableList模式下有效</remarks>
        public void Clear()
        {
            if (_models != null && _observer == null)
            {
                _models.Clear();
                Refresh(true);
            }
        }

        /// <summary>
        /// 滚动到指定数据的哪儿
        /// </summary>
        public void ScrollTo<T>(T data) where T : IBindableModel
        {
            int index = _observer == null ? _models.IndexOf(data) : _observer.IndexOf(data);
            if (index <= Count && index >= 0 && _head != index)
            {
                int flag = _scrollDir == Direction.Horizontal ? -1 : 1;
                Vector3 startPos = _scrollRect.content.localPosition;
                if (flag == 1)
                    startPos.y = (_head * deltaPos).y;
                else
                    startPos.x = (_head * deltaPos).x * flag;

                Vector3 sumDeltaPos = (index - _head) * deltaPos * flag;
                Vector3 endPos = startPos + sumDeltaPos;

                //计算缓动时间
                float duration = _scrollDir == Direction.Vertical ?
                   Mathf.Abs(sumDeltaPos.y / _distance) * Vue.Config.PerItemScrollTime :
                   Mathf.Abs(sumDeltaPos.x / _distance) * Vue.Config.PerItemScrollTime;

                _OnScroll = true;
                TweenBehavior.DoLocalMove(_scrollRect.content, duration, endPos).Call(RefreshViewAreaData);
            }
        }

        /// <summary>
        /// 刷新视图
        /// </summary>
        /// <param name="force">是否为强制刷新</param>
        public void Refresh(bool force = false)
        {
            //重新计算Content的大小
            Resize();

            if (force)
            {
                Action call = AlwaysShowNewestData && Count > _viewCount ? DoRefresh_ShowLast : DoRefresh_ShowFirst;
                if (PlayScrollEffectOnRefresh)
                {
                    Vector2 endPos = GetNormalizedPos(!AlwaysShowNewestData);
                    //均匀滚动
                    float perTime = Vue.Config.PerItemScrollTime;
                    float duration = AlwaysShowNewestData ? (Count - _tail - 1) * perTime : _head * perTime;
                    _OnScroll = true;
                    TweenBehavior.DoScroll(_scrollRect, duration, endPos).Call(call);
                }
                else
                    call.Invoke();
            }
            else
                RefreshViewAreaData();
        }


        public override void Destroy()
        {
            _scrollRect.onValueChanged.RemoveAllListeners();
            _observer?.RemoveListeners(GetHashCode());
            _models?.Clear();
            _scrollRect = null;
            _observer = null;
            _models = null;
        }


        //刷新时总是显示最后那一个数据
        private void DoRefresh_ShowLast()
        {
            int startPtr = Count - _viewCount;
            _head = startPtr < 0 ? 0 : startPtr;
            _scrollRect.normalizedPosition = GetNormalizedPos(false);
            int singal = _scrollDir == Direction.Vertical ? 1 : -1;
            ResetItemPos(Vector3.zero - singal * _head * deltaPos);
            RefreshViewAreaData();
        }

        //刷新时总是显示显示第一个数据
        private void DoRefresh_ShowFirst()
        {
            _head = 0;
            _scrollRect.normalizedPosition = GetNormalizedPos();
            ResetItemPos(Vector3.zero);
            RefreshViewAreaData();
        }

        private void RefreshViewAreaData()
        {
            Transform content = _scrollRect.content;
            int len = content.childCount;
            int count = Count;
            _tail = _head;
            for (int i = 0; i < len; i++)
            {
                GameObject itemObj = content.GetChild(i).gameObject;
                ViewUtil.SetActive(itemObj, _tail < count);
                if (_tail < count)
                    Rebind(itemObj, this[_tail++]);
            }
            --_tail;
            _OnScroll = false;
        }

        private Vector2 GetNormalizedPos(bool start = true)
        {
            if (start)
                return _scrollDir == Direction.Vertical ? Vector2.up : Vector2.zero;
            else
                return _scrollDir == Direction.Vertical ? Vector2.zero : Vector2.right;
        }

        private void CreateItems()
        {
            Resize(); //重新计算content的大小
            Transform content = _scrollRect.content;
            GameObject firstItem = content.GetChild(0).gameObject;
            //创建Item
            for (int i = 1; i <= _viewCount; i++)
            {
                GameObject itemViewObject = PrefabCloneUtil.RectTransformClone(firstItem, content);
                itemViewObject.name += i;
            }
            ResetItemPos(Vector3.zero);
        }

        private void ResetItemPos(Vector3 firstPos)
        {
            int singal = _scrollDir == Direction.Vertical ? 1 : -1;
            Transform content = _scrollRect.content;
            Vector3 deltaPos = this.deltaPos;
            for (int i = 0; i < content.childCount; i++)
            {
                (content.GetChild(i) as RectTransform).anchoredPosition = firstPos - singal * i * deltaPos;
            }
        }

        private void Init()
        {
            if (_scrollRect.content.childCount == _viewCount + 1) { return; }

            //创建Item
            CreateItems();

            int count = Count;
            int itemsCount = _scrollRect.content.childCount;

            //绑定Item数据
            for (int i = 0; i < itemsCount; i++)
            {
                RectTransform itemRectTrans = _scrollRect.content.GetChild(i).GetComponent<RectTransform>();
                //数据渲染
                if (_tail < count)
                    Rebind(itemRectTrans.gameObject, this[_tail++]);
                else
                    itemRectTrans.gameObject.SetActive(false);
            }

            //绑定滑动事件
            BindScrollEvt();

            --_tail; //恢复正确的位置
        }

        private void BindListeners()
        {
            int observerId = GetHashCode();
            int modifyIndex = -1;
            //注册事件
            _observer.AddListener_OnAdded<IBindableModel>(observerId, v => modifyIndex = _observer.IndexOf(v));
            _observer.AddListener_OnRemoved<IBindableModel>(observerId, (v, index) => modifyIndex = index);
            _observer.AddListener_OnReplaced<IBindableModel>(observerId, (index, r1, r2) => modifyIndex = index);
            _observer.AddListener_OnChanged(observerId, mode =>
            {
                bool force = mode != NotificationMode.Sort && Count <= _viewCount + 1 ||
                             modifyIndex >= 0 && modifyIndex < _tail;
                Refresh(force);
                modifyIndex = -1;
            });
        }

        private void Rebind(GameObject itemViewObject, IBindableModel model)
        {
            UIBundle bundle = UIQuerier.Query(itemViewObject.name, model);
            if (bundle == null)
            {
                bundle = ViewUtil.Patch3PassButNoBinding(itemViewObject.gameObject, model);
                Vue.Updater.Table.BindV(itemViewObject.name, bundle);
            }
            bundle.Rebind(model);
            model.UpdateAll(bundle);
        }

        /// <summary>
        /// 重新计算Content区域的大小
        /// </summary>
        private void Resize()
        {
            //会触发OnValueChanged事件
            _scrollRect.content.sizeDelta = deltaPos * Count;
            _scrollRect.movementType = Count <= _viewCount ? MovementType.Clamped : MovementType.Elastic;
        }


        #region 异常检查
        private void CheckBindedList()
        {
            if (_observer != null)
                throw new Exception("已经将视图和一个IObserverList集合进行绑定，对数据的操作只能依靠IObserverList，同时如果要要绑定新数据也请使用BindList()");
        }
        #endregion

        #region 滚动算法实现

        private void BindScrollEvt()
        {
            //监听边界
            Vector3[] corners0 = new Vector3[4];
            Vector3[] corners1 = new Vector3[4];
            Vector3[] viewportCorners = new Vector3[4];

            ScrollRect scrollRect = _scrollRect;
            RectTransform viewport = scrollRect.viewport;
            Transform content = scrollRect.content;
            if (_scrollDir == Direction.Vertical)
            {
                scrollRect.onValueChanged.AddListener((v2) =>
                {
                    //计算出视口区域的四个角
                    viewport.GetWorldCorners(viewportCorners);
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
                    viewport.GetWorldCorners(viewportCorners);
                    content.GetChild(0).GetComponent<RectTransform>().GetWorldCorners(corners0);
                    content.GetChild(content.childCount - 2).GetComponent<RectTransform>().GetWorldCorners(corners1);
                    //只需监听第一个和倒数第二个
                    HorizontalListenCorners(content, viewportCorners, corners0, corners1);
                });
            }
        }


        /// <summary>
        ///监听垂直方向的滚动（ 左下[0] 左上[1] 右上[2] 右下[3]）
        /// </summary>
        private void VerticalListenCorners(Transform contentTrans, Vector3[] viewportCorners, Vector3[] corners0, Vector3[] corners1)
        {
            int childCount = contentTrans.childCount;
            int dataCount = Count;
            Transform itemTrans = null;
            int index = -1;

            //向上移动只监听头部
            if ((_tail == dataCount - 1 ? _loop : true) && corners0[0].y > viewportCorners[1].y)
            {
                itemTrans = contentTrans.GetChild(0);
                itemTrans.localPosition = contentTrans.GetChild(childCount - 1).localPosition - deltaPos;
                itemTrans.SetAsLastSibling(); //设置为最后一个位置

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + 1) % dataCount;
                _head = (_head + 1) % dataCount;

                index = _tail;
            }
            //向下移动监听倒数第二个（因为实例化的数量=可视数+1）
            else if ((_head == 0 ? _loop : true) && corners1[0].y < viewportCorners[0].y)
            {
                itemTrans = contentTrans.GetChild(childCount - 1);
                itemTrans.localPosition = contentTrans.GetChild(0).localPosition + deltaPos;
                itemTrans.SetAsFirstSibling(); //最后一个设置为第一个位置

                //渲染数据 先计算指针再渲染数据 不写if语句！！！
                _tail = (_tail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;
                index = _head;
            }

            if (itemTrans != null && (!_OnScroll || _OnScroll && RenderModelOnScroll))
            {
                ViewUtil.SetActive(itemTrans.gameObject, true);
                Rebind(itemTrans.gameObject, this[index]);
            }
        }

        /// <summary>
        /// 水平滚动
        /// </summary>
        private void HorizontalListenCorners(Transform contentTrans, Vector3[] viewportCorners, Vector3[] corners0, Vector3[] corners1)
        {
            int childCount = contentTrans.childCount;
            int dataCount = Count;

            Transform itemTrans = null;
            int index = -1;

            //向左移动只监听最左边那个
            if ((_tail == dataCount - 1 ? _loop : true) && corners0[3].x < viewportCorners[1].x) //向左移动了一个Item的距离
            {
                itemTrans = contentTrans.GetChild(0);
                itemTrans.localPosition = contentTrans.GetChild(childCount - 1).localPosition + deltaPos;
                itemTrans.SetAsLastSibling(); //设置为最后一个位置

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + 1) % dataCount;
                _head = (_head + 1) % dataCount;

                index = _tail;
            }
            //向下移动监听倒数第二个（因为实例化的数量=可视数+1）
            else if ((_head == 0 ? _loop : true) && corners1[3].x > viewportCorners[3].x) //正在在向右移动
            {
                itemTrans = contentTrans.GetChild(childCount - 1);
                itemTrans.localPosition = contentTrans.GetChild(0).localPosition - deltaPos;
                itemTrans.SetAsFirstSibling(); //最后一个设置为第一个位置

                //渲染数据 先计算指针再渲染数据 不写if语句！！！
                _tail = (_tail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;

                index = _head;
            }

            if (itemTrans != null && (!_OnScroll || _OnScroll && RenderModelOnScroll))
            {
                ViewUtil.SetActive(itemTrans.gameObject, true);
                Rebind(itemTrans.gameObject, this[index]);
            }

        }


        #endregion

    }
}
