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
    /// Grid高性能滚动组件，轻松实现任何背包等网格组件
    /// </summary>
    /// <remarks>第一个Item的锚点位置必须为左上角，位置为(0,0,0)，否则位置计算可能会出错</remarks>
    public sealed class LoopGrid : Widget
    {
        private Direction _scrollDir;               //只能选Vertical（垂直）、Horizontal（水平）
        private ScrollRect _scrollRect;             //必须的ScrollRect组件
        private int _rows;                          //网格可见的视图行数(实际的行数=可见的行数+1)
        private int _cols;                          //网格可见的视图列数(实际的列数=可见的列数+1)
        private float _x;                           //leftItemPos.x+x=rightItemPos.x
        private float _y;                           //upItemPos.y+y=downItemPos.y
        private bool _playScrollEffectOnRefresh;    //当刷新视图时是否播放滚动效果
        private bool _renderModelOnScroll;          //在进行滚动动画时是否重新绑定模型数据（减少数据重新渲染的开销）
        private List<IBindableModel> _models;       //获取绑定的模型数据
        private IObservableList _observer;          //IObservableList模式
        private int _tail;                          //数据尾指针
        private int _head;                          //数据头指针
        private bool _OnScroll;                     //指示当前是否在进行滚动动画

        /// <summary>
        /// 当重新进行数据绑定时调用
        /// </summary>
        internal event Action<RectTransform, int> OnRebind;

        /// <summary>
        /// 构建Grid视图组件
        /// </summary>
        /// <param name="scrollRect">必须的ScrollRect组件</param>
        /// <param name="rows">网格可见的视图行数</param>
        /// <param name="cols">网格可见的视图列数</param>
        /// <param name="x">水平方向上相连两个Item的位置差:  x = rightItemPos.x - leftItemPos.x</param>
        /// <param name="y">垂直方向上相连两个Item的位置差:  y = downItemPos.y - upItemPos.y</param>
        /// <param name="scrollDir">滚动方向</param>
        public LoopGrid(ScrollRect scrollRect, int rows, int cols, float x, float y, Direction scrollDir = Direction.Vertical)
        {
            _scrollRect = scrollRect;
            _rows = rows;
            _cols = cols;
            _x = x;
            _y = y;
            _scrollDir = scrollDir;
            scrollRect.movementType = MovementType.Elastic;
            scrollRect.horizontal = scrollDir == Direction.Horizontal;
            scrollRect.vertical = scrollDir == Direction.Vertical;
        }

        private IBindableModel this[int index]
        {
            get
            {
                if (_observer == null)
                    return _models[index];
                else
                    return _observer.Get<IBindableModel>(index);
            }
            set
            {
                if (_observer == null)
                    _models[index] = value;
                else
                    _observer.Set(value, index);
            }
        }

        public int Count => _observer == null ? _models.Count : _observer.Count;

        /// <summary>
        /// 获取滚动组件
        /// </summary>
        public ScrollRect ScrollRect => _scrollRect;

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


        internal IBindableModel GetData(int index) => this[index];

        internal void SetData(int index, IBindableModel model) => this[index] = model;

        internal void RemoveData(int index)
        {
            if (_observer == null)
                RemoveData(this[index], index);
            else
                _observer.RemoveAt(index);
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
            if (_models == null)
            {
                BindList(newData);
            }
            else if (!ReferenceEquals(newData, _models))
            {
                ListUtil.Copy(_models, newData);
                Refresh(); //刷新数据
            }
        }

        /// <summary>
        /// 绑定item数据
        /// <para>若为引用绑定，则无需使用AddData/RemoveData函数进行对数据的增删</para>
        /// </summary>
        /// <param name="data">绑定数据</param>
        public void BindList<T>(List<T> data) where T : IBindableModel
        {
            if (_models != null)
            {
#if UNITY_EDITOR
                LogUtil.Warning("GridView已经绑定了List数据，不能再进行绑定");
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
                Refresh(Count <= _cols * _rows);
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
                Refresh(Count <= _rows * _cols); //刷新
            }
        }

        /// <summary>
        /// 视图刷新
        /// </summary>
        public void Refresh(bool force = false)
        {
            //重新计算Content的大小
            Resize();
            if (force)
            {
                if (PlayScrollEffectOnRefresh)
                {
                    Transform content = _scrollRect.content;
                    //均匀滚动
                    float perTime = Vue.Config.PerItemScrollTime;
                    //均匀滚动
                    float duration = _scrollDir == Direction.Vertical ?
                    Mathf.Abs(content.localPosition.y / _y) * Vue.Config.PerItemScrollTime :
                        Mathf.Abs(content.localPosition.x / _x) * Vue.Config.PerItemScrollTime;
                    _OnScroll = true;
                    TweenBehavior.DoScroll(_scrollRect, duration, GetNormalizedPos()).Call(ForceRefresh);
                }
                else
                    ForceRefresh();
            }
            else
                RefreshViewAreaData();
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

        public override void Destroy()
        {
            _scrollRect.onValueChanged.RemoveAllListeners();
            _observer?.RemoveListeners(GetHashCode());
            _models?.Clear();
            _scrollRect = null;
            _observer = null;
            _models = null;
        }

        //刷新时总是显示显示第一个数据
        private void ForceRefresh()
        {
            _head = 0;
            _scrollRect.normalizedPosition = GetNormalizedPos();
            ResetItemPos();
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
                RectTransform item = content.GetChild(i) as RectTransform;
                ViewUtil.SetActive(item.gameObject, _tail < count);
                if (_tail < count)
                {
                    Rebind(item, this[_tail], _tail);
                    _tail++;
                }

            }
            --_tail;
            _OnScroll = false;
        }

        private Vector2 GetNormalizedPos()
        {
            return _scrollDir == Direction.Vertical ? Vector2.up : Vector2.zero;
        }

        private void Rebind(RectTransform item, IBindableModel model, int dataIndex)
        {
            UIBundle bundle = UIQuerier.Query(item.name, model);
            if (bundle == null)
                ViewUtil.Patch3Pass(item.gameObject, model);
            else
                Vue.Updater.Rebind(item.name, model);
            model.NotifyAll();

            OnRebind?.Invoke(item, dataIndex);
        }

        /// <summary>
        /// 重新计算content的大小
        /// </summary>
        private void Resize()
        {
            Vector3 deltaPos = _scrollDir == Direction.Vertical ? new Vector2(0, Mathf.Abs(_y)) : new Vector2(Mathf.Abs(_x), 0);
            float temp = _scrollDir == Direction.Vertical ? Count / (float)_cols : Count / (float)_rows;
            _scrollRect.content.sizeDelta = (Mathf.FloorToInt(temp) + 1) * deltaPos;

            //当前是否可以移动
            _scrollRect.movementType = Count <= _cols * _rows ? MovementType.Clamped : MovementType.Elastic;
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
                bool force = mode != NotificationMode.Sort && Count <= _rows * _cols ||
                             modifyIndex >= 0 && modifyIndex < _tail;
                Refresh(force);
                modifyIndex = -1;
            });
        }

        private void Init()
        {
            if (_scrollRect.content.childCount > 1) { return; }

            CreateItems();

            int len = _scrollRect.content.childCount;
            int count = Count;
            for (int i = 0; i < len; i++)
            {
                RectTransform itemRect = _scrollRect.content.GetChild(i).GetComponent<RectTransform>();
                if (_tail < count)
                    Rebind(itemRect, this[_tail], _tail++);
                else
                    itemRect.gameObject.SetActive(false);
            }

            _tail--; //恢复到正确位置
            BindScrollEvt();
        }

        private void CreateItems()
        {
            Resize();
            //创建Item
            Transform content = _scrollRect.content;
            GameObject firstItem = content.GetChild(0).gameObject;
            string name = firstItem.name;

            int iMax = _scrollDir == Direction.Vertical ? _rows : _cols;
            int jMax = _scrollDir == Direction.Vertical ? _cols : _rows;

            for (int i = 0; i <= iMax; i++)//垂直滚动多一行
            {
                for (int j = 0; j < jMax; j++)
                {
                    GameObject itemViewObject = i + j == 0 ? firstItem :
                        PrefabCloneUtil.RectTransformClone(firstItem, content);
                    itemViewObject.name = name + (jMax * i + j);
                }
            }

            ResetItemPos();
        }

        private void ResetItemPos()
        {
            Vector3 deltaPos = Vector3.zero; //第一个位置一定是(0,0,0)
            Transform content = _scrollRect.content;
            //按下面的方法确保contant的前面rows个为第一列或前cols个为第一行
            if (_scrollDir == Direction.Vertical) //垂直滚动时位置按行一行一行的设置
            {
                for (int i = 0; i <= _rows; i++)//垂直滚动多一行
                {
                    for (int j = 0; j < _cols; j++)
                    {
                        //Debug.Log($"{content.GetChild(i * _cols + j).name} => ({i}, {j}) = " + deltaPos);
                        (content.GetChild(i * _cols + j) as RectTransform).anchoredPosition = deltaPos;
                        deltaPos.x += _x;
                    }
                    deltaPos.y += _y; //下一行
                    deltaPos.x -= _x * _cols;
                }
            }
            else if (_scrollDir == Direction.Horizontal)//水平滚动时位置按列一列一列的设置
            {
                for (int i = 0; i <= _cols; ++i)//水平滚动多一列
                {
                    for (int j = 0; j < _rows; ++j)
                    {
                        (content.GetChild(i * _rows + j) as RectTransform).anchoredPosition = deltaPos;
                        deltaPos.y += _y;
                    }
                    deltaPos.x += _x; //下一列
                    deltaPos.y -= _y * _rows;
                }
            }
        }

        #region 算法实现

        private void BindScrollEvt()
        {
            //4.监听边界
            Vector3[] corners0 = new Vector3[4];
            Vector3[] corners1 = new Vector3[4];
            Vector3[] viewportCorners = new Vector3[4];

            RectTransform viewport = _scrollRect.viewport;
            Transform content = _scrollRect.content;

            if (_scrollDir == Direction.Vertical)
            {
                int lastRowFirstIdx = _rows * _cols; //最后一行的第一个索引
                int lastFirst = lastRowFirstIdx - _cols; //倒数第二行的第一个索引

                _scrollRect.onValueChanged.AddListener((v2) =>
                {
                    //5.计算可视域的边界
                    viewport.GetWorldCorners(viewportCorners);
                    content.GetChild(0).GetComponent<RectTransform>().GetWorldCorners(corners0);
                    content.GetChild(lastFirst).GetComponent<RectTransform>().GetWorldCorners(corners1);

                    //监听第一行的第一个以及倒数第二行的第一个
                    if (corners0[0].y > viewportCorners[1].y && _tail > _head)
                    {
                        VercitalListenerCorners0(content, lastRowFirstIdx);
                    }
                    else if (corners1[0].y < viewportCorners[0].y && _head > 0)
                    {
                        VercitalListenerCorners1(content, lastRowFirstIdx);
                    }
                });
            }
            else
            {
                int lastColFirstIdx = _cols * _rows; //最后一列的第一个索引
                int lastFirst = lastColFirstIdx - _rows; //倒数第二列的第一个索引

                _scrollRect.onValueChanged.AddListener((v2) =>
                {
                    //5.计算可视域的边界
                    viewport.GetWorldCorners(viewportCorners);
                    content.GetChild(0).GetComponent<RectTransform>().GetWorldCorners(corners0);
                    content.GetChild(lastFirst).GetComponent<RectTransform>().GetWorldCorners(corners1);

                    //监听第一列的第一个和倒数第二列的第一个
                    if (corners0[2].x < viewportCorners[1].x && _tail > _head)
                    {
                        HorizontalListenerCorners0(content, lastColFirstIdx);
                    } //倒数第二列的第一个正在向右移动 
                    else if (corners1[3].x > viewportCorners[3].x && _head > 0)
                    {
                        HorizontalListenerCorners1(content, lastColFirstIdx);
                    }
                });
            }
        }

        private void VercitalListenerCorners0(Transform content, int lastRowFirstIdx)
        {
            int dataCount = Count;
            //将第一行的所有Item移动到最后一行
            for (int i = 0; i < _cols; i++)
            {
                Vector3 pos = content.GetChild(lastRowFirstIdx + i).localPosition;
                pos.y += _y;

                Transform itemTrans = content.GetChild(i);
                itemTrans.localPosition = pos; //位置修改

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + 1) % dataCount;
                _head = (_head + 1) % dataCount;

                if (_OnScroll && RenderModelOnScroll || !_OnScroll)
                    Rebind(itemTrans as RectTransform, this[_tail], _tail);
                if (_tail < _head) { itemTrans.gameObject.SetActive(false); }
            }

            for (int i = 0; i < _cols; i++)
            {
                content.GetChild(0).SetAsLastSibling(); //逻辑位置修改
            }
        }

        private void VercitalListenerCorners1(Transform content, int lastRowFirstIdx)
        {
            int dataCount = Count;
            //将最后一行移动到第一行
            for (int i = _cols - 1; i >= 0; i--) //保证数据显示的顺序正确性
            {
                Vector3 pos = content.GetChild(i).localPosition;
                pos.y -= _y;
                Transform itemTrans = content.GetChild(lastRowFirstIdx + i);
                itemTrans.localPosition = pos; //位置修改

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;

                if (_OnScroll && RenderModelOnScroll || !_OnScroll)
                    Rebind(itemTrans as RectTransform, this[_head], _head);
                //向下滑动全部显示
                if (!itemTrans.gameObject.activeSelf) { itemTrans.gameObject.SetActive(true); }
            }

            int lastIdx = content.childCount - 1;
            for (int i = _cols - 1; i >= 0; i--)
            {
                content.GetChild(lastIdx).SetAsFirstSibling(); //逻辑位置修改
            }
        }

        private void HorizontalListenerCorners0(Transform content, int lastColFirstIdx)
        {
            //向左滑动了一个Item的距离
            int dataCount = Count;
            //将第一列全部移动到最后一列
            for (int i = 0; i < _rows; i++)
            {
                Vector3 pos = content.GetChild(lastColFirstIdx + i).localPosition;
                pos.x += _x;
                Transform itemTrans = content.GetChild(i);
                itemTrans.localPosition = pos; //位置改变

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + 1) % dataCount;
                _head = (_head + 1) % dataCount;

                if (_OnScroll && RenderModelOnScroll || !_OnScroll)
                    Rebind(itemTrans as RectTransform, this[_tail], _tail);
                //如果没有设置无限滚动则数据渲染到底时隐藏显示，但是任然进行数据渲染
                if (_tail < _head) { itemTrans.gameObject.SetActive(false); }
            }

            for (int i = 0; i < _rows; i++)
            {
                content.GetChild(0).SetAsLastSibling(); //逻辑位置的改变
            }
        }

        private void HorizontalListenerCorners1(Transform content, int lastColFirstIdx)
        {
            //当前正在向右滑动
            int dataCount = Count;
            //将最后一列全部移动到第一列 →
            for (int i = _rows - 1; i >= 0; i--)
            {
                Vector3 pos = content.GetChild(i).localPosition;
                pos.x -= _x;
                Transform itemTrans = content.GetChild(lastColFirstIdx + i);
                itemTrans.localPosition = pos; //位置的改变

                //渲染数据 先计算指针再渲染数据
                _tail = (_tail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;

                if (_OnScroll && RenderModelOnScroll || !_OnScroll)
                    Rebind(itemTrans as RectTransform, this[_head], _head);
                //向右滑动全部显示
                if (!itemTrans.gameObject.activeSelf) { itemTrans.gameObject.SetActive(true); }
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
