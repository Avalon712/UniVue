using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.ScrollRect;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Tween;
using UniVue.Utils;

namespace UniVue.View.Views.Flexible
{
    public sealed class FListView : FlexibleView
    {
        private Vector3 _deltaPos;
        private ScrollRect _scrollRect;
        private List<IBindableModel> _models;
        private int _head, _trail; //数据头尾指针
        private bool _isDirty;
        private bool _flag;//标志当前所有的Item是否都已经生成UIBundle了
        private int _hash; //RebindData()时判断新绑定的数据和之前绑定的数据是否是同一个对象

        /// <summary>
        /// 设置滚动方向
        /// </summary>
        public Direction scrollDir { get; set; } = Direction.Vertical;

        /// <summary>
        /// 是否循环滚动
        /// </summary>
        public bool isLoop { get; set; }

        /// <summary>
        /// 可见数量
        /// </summary>
        public int viewNum { get; set; }

        /// <summary>
        /// 相连两个item在滚动方向上的距离
        /// </summary>
        public float distance { get; set; }

        public FListView(GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common) : base(viewObject, viewName, level)
        {
            _scrollRect = ComponentFindUtil.BreadthFind<ScrollRect>(viewObject);

            if (_scrollRect == null)
            {
                throw new Exception("viewObject身上未包含一个ScrollRect组件，该功能依赖该组件！");
            }

            //设置增量位置
            _deltaPos = scrollDir == Direction.Vertical ? new Vector3(0, distance, 0) : new Vector3(distance, 0, 0);
            //设置是否无限滚动
            _scrollRect.movementType = isLoop ? MovementType.Unrestricted : MovementType.Elastic;
        }


        public override void OnUnload()
        {
            _scrollRect.onValueChanged.RemoveAllListeners();
            _scrollRect = null;
            _models.Clear();
            _models = null;
            base.OnUnload();
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
            else if (_hash != newData.GetHashCode())
            {
                ListUtil.Copy(_models, newData);
                Refresh(); //刷新数据
                _hash = newData.GetHashCode();
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
            _hash = data.GetHashCode();
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
                if (_trail < data.Count)
                {
                    dynamicView.BindModel(data[_trail++]);
                }
                else
                {
                    //这一步是为了生成UIBundle，以此使用RebindModel()函数
                    if (data.Count > 0) { dynamicView.BindModel(data[0]); _flag = true; }
                    itemRectTrans.gameObject.SetActive(false);
                }
            }

            //绑定滑动事件
            BindScrollEvt();

            --_trail; //恢复正确的位置

        }

        /// <summary>
        /// 对列表进行排序，排序规则
        /// </summary>
        /// <param name="comparer">排序规则</param>
        public void Sort(Comparison<IBindableModel> comparer)
        {
            _isDirty = true;
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
                _isDirty = true;
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
                _isDirty = true;
                Refresh();
            }
        }

        /// <summary>
        /// 刷新视图，当List中某个数据发生了更改可调用此函数进行更新
        /// </summary>
        public void Refresh()
        {
            Vector2 endPos = scrollDir == Direction.Vertical ? Vector2.up : Vector2.zero;

            //均匀滚动
            Vector3 localPos = _scrollRect.content.localPosition;
            float duration = scrollDir == Direction.Vertical ?
                   Mathf.Abs(localPos.y / distance) * 0.1f :
                   Mathf.Abs(localPos.x / distance) * 0.1f;

            TweenBehavior.DoScroll(_scrollRect, duration, endPos).Call(DoRefresh);
        }

        private void DoRefresh()
        {
            _head = 0;
            _trail = 0;
            Transform content = _scrollRect.content;
            int len = content.childCount;
            for (int i = 0; i < len; i++)
            {
                GameObject itemObj = content.GetChild(i).gameObject;
                if (_trail < _models.Count)
                {
                    if (!itemObj.activeSelf)
                    {
                        itemObj.SetActive(true);
                    }

                    Rebind(itemObj.name, _models[_trail++]);
                }
                else
                {
                    itemObj.SetActive(false);
                }
            }
            --_trail;
            _isDirty = false;
        }

        private void CreateItems()
        {
            Resize(); //重新计算content的大小

            int singal = scrollDir == Direction.Vertical ? 1 : -1;
            Transform content = _scrollRect.content;
            Vector3 startPos = content.GetChild(0).localPosition;
            GameObject firstItem = content.GetChild(0).gameObject;

            //创建Item
            for (int i = 1; i <= viewNum; i++)
            {
                GameObject itemViewObject = PrefabCloneUtil.RectTransformClone(firstItem, content);
                itemViewObject.name += i;
                itemViewObject.transform.localPosition = startPos - singal * i * _deltaPos;
            }
        }

        private void BindScrollEvt()
        {
            //监听边界
            Vector3[] corners0 = new Vector3[4];
            Vector3[] corners1 = new Vector3[4];
            Vector3[] viewportCorners = new Vector3[4];

            ScrollRect scrollRect = _scrollRect;

            if (scrollDir == Direction.Vertical)
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
            if ((_trail == dataCount - 1 ? isLoop : true) && corners0[0].y > viewportCorners[1].y)
            {
                Transform itemTrans = contentTrans.GetChild(0);
                itemTrans.localPosition = contentTrans.GetChild(childCount - 1).localPosition - _deltaPos;
                itemTrans.SetAsLastSibling(); //设置为最后一个位置

                //渲染数据 先计算指针再渲染数据
                _trail = (_trail + 1) % dataCount;
                _head = (_head + 1) % dataCount;
                //当前数据不是脏数据才进行重新渲染
                if (!_isDirty) { Rebind(itemTrans.name, data[_trail]); }
            }//向下移动监听倒数第二个（因为实例化的数量=可视数+1）
            else if ((_head == 0 ? isLoop : true) && corners1[0].y < viewportCorners[0].y)
            {
                Transform lastItemTrans = contentTrans.GetChild(childCount - 1);
                lastItemTrans.localPosition = contentTrans.GetChild(0).localPosition + _deltaPos;
                lastItemTrans.SetAsFirstSibling(); //最后一个设置为第一个位置

                //渲染数据 先计算指针再渲染数据 不写if语句！！！
                _trail = (_trail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;
                //当前数据不是脏数据才进行重新渲染
                if (!_isDirty) { Rebind(lastItemTrans.name, data[_head]); }
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
            if ((_trail == dataCount - 1 ? isLoop : true) && corners0[3].x < viewportCorners[1].x) //向左移动了一个Item的距离
            {
                Transform itemTrans = contentTrans.GetChild(0);
                itemTrans.localPosition = contentTrans.GetChild(childCount - 1).localPosition + _deltaPos;
                itemTrans.SetAsLastSibling(); //设置为最后一个位置

                //渲染数据 先计算指针再渲染数据
                _trail = (_trail + 1) % dataCount;
                _head = (_head + 1) % dataCount;
                //当前数据不是脏数据才进行重新渲染
                if (!_isDirty) { Rebind(itemTrans.name, data[_trail]); }
            }//向下移动监听倒数第二个（因为实例化的数量=可视数+1）
            else if ((_head == 0 ? isLoop : true) && corners1[3].x > viewportCorners[3].x) //正在在向右移动
            {
                Transform lastItemTrans = contentTrans.GetChild(childCount - 1);
                lastItemTrans.localPosition = contentTrans.GetChild(0).localPosition - _deltaPos;
                lastItemTrans.SetAsFirstSibling(); //最后一个设置为第一个位置

                //渲染数据 先计算指针再渲染数据 不写if语句！！！
                _trail = (_trail + dataCount - 1) % dataCount;
                _head = (_head + dataCount - 1) % dataCount;
                //当前数据不是脏数据才进行重新渲染
                if (!_isDirty) { Rebind(lastItemTrans.name, data[_head]); }
            }
        }

        #endregion

    }
}
