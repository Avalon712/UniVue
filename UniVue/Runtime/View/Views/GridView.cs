using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Evt;
using UniVue.Model;
using UniVue.Tween;
using UniVue.Utils;
using UniVue.View.Views.Flexible;
using static UnityEngine.UI.ScrollRect;

namespace UniVue.View.Views
{
    /// <summary>
    /// 网格视图
    /// <para>注意:Item的锚点位置为左上角</para>
    /// </summary>
    public sealed class GridView : BaseView 
    {
        private struct RuntimeData
        {
            public ScrollRect scrollRect;
            public int head, trial;
            public List<IBindableModel> models;
            public bool isDirty; //当数据被修改时，设为true===>Refresh
            public bool flag;//标志当前所有的Item是否都已经生成UIBundle了
            public int hash; //RebindData()时判断新绑定的数据和之前绑定的数据是否是同一个对象
        }

        #region 配置信息
        /// <summary>
        /// 设置滚动方向，若为垂直滚动，那么可视域内为5*6那么实际创建的网格为6*6，即多一行；
        /// 若为水平滚动，那么可视域内为5*6那么实际创建的网格为5*7，即多一列
        /// </summary>
        [Header("滚动方向 Horizontal|Vercital")]
        [SerializeField] internal Direction scrollDir = Direction.Vertical;

        /// <summary>
        /// 网格视图行数(实际的行数=可视域的行数+1)
        /// </summary>
        [Header("可见行数")]
        [SerializeField] internal int rows;

        /// <summary>
        /// 网格视图列数(实际的列数=可视域的列数+1)
        /// </summary>
        [Header("可见列数")]
        [SerializeField] internal int cols;

        /// <summary>
        /// 水平方向上当前item移动多少距离到达下一个item（注意区别于几何距离）
        /// </summary>
        [Header("leftItemPos.x+x=rightItemPos.x")]
        [SerializeField] internal float x;

        /// <summary>
        /// 垂直方向上当前item移动多少距离到达下一个item（注意区别于几何距离）
        /// </summary>
        [Header("upItemPos.y+y=downItemPos.y")]
        [SerializeField] internal float y;

        #endregion

        private RuntimeData _runtime;

        public override void OnLoad()
        {
            _runtime.scrollRect = ComponentFindUtil.BreadthFind<ScrollRect>(viewObject);

            if (_runtime.scrollRect == null)
            {
                throw new Exception("viewObject身上未包含一个ScrollRect组件，该功能依赖该组件！");
            }

            base.OnLoad();
        }

        protected override void AutoBindEvent()
        {
            //获取所有的ui组件
            var uis = ComponentFindUtil.FindAllSpecialUIComponents(viewObject, this, _runtime.scrollRect.content.gameObject);
            //构建UIEvent
            UIEventBuilder.Build(name, uis);
            //处理路由事件
            Vue.Router.BindRouteEvt(name, uis);
        }

        public override void OnUnload()
        {
            _runtime.scrollRect.onValueChanged.RemoveAllListeners();
            _runtime = default;
            base.OnUnload();
        }

        /// <summary>
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindData<T>(List<T> newData) where T : IBindableModel
        {
            if (_runtime.models == null)
            {
                BindData(newData);
            }
            else if(_runtime.hash != newData.GetHashCode())
            {
                ListUtil.Copy(_runtime.models, newData);
                Refresh(); //刷新数据
                _runtime.hash = newData.GetHashCode();
            }
        }

        /// <summary>
        /// 绑定item数据
        /// <para>若为引用绑定，则无需使用AddData/RemoveData函数进行对数据的增删</para>
        /// </summary>
        /// <param name="data">绑定数据</param>
        public void BindData<T>(List<T> data) where T : IBindableModel
        {
            if (_runtime.models != null)
            {
#if UNITY_EDITOR
                LogUtil.Warning("GridView已经绑定了List数据，不能再进行绑定");
#endif
                return;
            }

            _runtime.hash = data.GetHashCode();
            _runtime.models = new List<IBindableModel>(data.Count);
            for (int i = 0; i < data.Count; i++) { _runtime.models.Add(data[i]); }

            CreateItems();

            //设置显示区域的大小
            Resize();

            int len = _runtime.scrollRect.content.childCount;
            for (int i = 0; i < len; i++)
            {
                int k = i;
                RectTransform itemRect = _runtime.scrollRect.content.GetChild(k).GetComponent<RectTransform>();

                FlexibleView dynamicView = new FlexibleView(itemRect.gameObject, null, ViewLevel.Permanent);

                if (_runtime.trial < data.Count)
                {
                    dynamicView.BindModel(data[_runtime.trial++]);
                }
                else
                {
                    //这一步是为了生成UIBundle，以此使用RebindModel()函数
                    if (data.Count > 0) { dynamicView.BindModel(data[0]); _runtime.flag = true; }
                    itemRect.gameObject.SetActive(false);
                }
            }

            BindScrollEvt();

            _runtime.trial--; //恢复到正确位置
        }

        /// <summary>
        /// 排序，本质上是对数据进行排序
        /// </summary>
        public void Sort(Comparison<IBindableModel> comparer) 
        {
            _runtime.models.Sort(comparer);
            _runtime.isDirty = true;
            Refresh(); //刷新
        }

        /// <summary>
        /// 添加数据(需要先绑定数据)
        /// </summary>
        /// <param name="newData">新加入的数据</param>
        public void AddData<T>(T newData) where T : IBindableModel
        {
            //如果当前所有的Item都还没有生成UIBundle则先生成UIBundle
            if (!_runtime.flag)
            {
                _runtime.flag = true;
                Transform content = _runtime.scrollRect.content;
                for (int i = 0; i < content.childCount; i++)
                {
                    Vue.Router.GetView(content.GetChild(i).name).BindModel(newData);
                }
            }

            _runtime.isDirty = !_runtime.models.Contains(newData);
            if (_runtime.isDirty)
            {
                _runtime.models.Add(newData);
                Resize();
                Refresh();
            }
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="remove">要移除的数据[如果知道索引则不用传递改参数]</param>
        public void RemoveData<T>(T remove) where T : IBindableModel
        {
            _runtime.isDirty = _runtime.models.Contains(remove);
            if (_runtime.isDirty)
            {
                ListUtil.TrailDelete(_runtime.models, _runtime.models.IndexOf(remove));
                //重新计算content的大小
                Resize();
                Refresh();
            }
        }

        /// <summary>
        /// 视图刷新
        /// </summary>
        public void Refresh()
        {
            Transform content = _runtime.scrollRect.content;

            //水平滚动：0为最左边，1为最右边；    垂直滚动：1为顶部，0为顶部
            Vector2 endPos = scrollDir == Direction.Vertical ? Vector2.up : Vector2.zero;

            //均匀滚动
            float duration = scrollDir == Direction.Vertical ?
                Mathf.Abs(content.localPosition.y / y) * 0.1f :
                Mathf.Abs(content.localPosition.x / x) * 0.1f;

            TweenBehavior.DoScroll(_runtime.scrollRect, duration, endPos).Call(DoRefresh);
        }

        private void DoRefresh()
        {
            _runtime.head = 0;
            _runtime.trial = 0;
            Transform content = _runtime.scrollRect.content;
            for (int i = 0; i < content.childCount; i++)
            {
                GameObject itemObj = content.GetChild(i).gameObject;
                if (_runtime.trial < _runtime.models.Count)
                {
                    if (!itemObj.activeSelf)
                    {
                        itemObj.SetActive(true);
                    }
                    Vue.Router.GetView(itemObj.name).RebindModel(_runtime.models[_runtime.trial]);
                    _runtime.models[_runtime.trial++].NotifyAll();
                }
                else
                {
                    itemObj.SetActive(false);
                }
            }
            --_runtime.trial;
            _runtime.isDirty = false;
        }


        /// <summary>
        /// 重新计算content的大小
        /// </summary>
        private void Resize()
        {
            Vector3 deltaPos = scrollDir == Direction.Vertical ? new Vector2(0, Mathf.Abs(y)) : new Vector2(Mathf.Abs(x), 0);
            float temp = scrollDir==Direction.Vertical ? _runtime.models.Count / (float)cols : _runtime.models.Count / (float)rows;
            int num = (int)(temp - (int)temp > 0.1f ? temp + 1 : temp);
            _runtime.scrollRect.content.sizeDelta = num * deltaPos;

            //当前是否可以移动
            _runtime.scrollRect.movementType = _runtime.models.Count < cols * rows ? MovementType.Clamped : MovementType.Elastic;
        }

        private void CreateItems()
        {
            //创建Item
            Transform content = _runtime.scrollRect.content;
            Vector3 deltaPos = content.GetChild(0).localPosition;
            GameObject firstItem = content.GetChild(0).gameObject;
            string name = firstItem.name;

            //按下面的方法确保contant的前面rows个为第一列或前cols个为第一行
            if (scrollDir == Direction.Vertical) //垂直滚动时位置按行一行一行的设置
            {
                for (int i = 0; i <= rows; i++)//垂直滚动多一行
                {
                    for (int j = 0; j < cols; j++)
                    {
                        GameObject itemViewObject = i + j == 0 ? firstItem :
                            PrefabCloneUtil.RectTransformClone(firstItem, content);
                        itemViewObject.transform.localPosition = deltaPos;
                        itemViewObject.name = name + (cols * i + j);
                        deltaPos.x += x;
                    }
                    deltaPos.y += y; //下一行
                    deltaPos.x -= x * cols;
                }
            }
            else if (scrollDir == Direction.Horizontal)//水平滚动时位置按列一列一列的设置
            {
                for (int i = 0; i <= cols; ++i)//水平滚动多一列
                {
                    for (int j = 0; j < rows; ++j)
                    {
                        GameObject itemViewObject = i + j == 0 ? firstItem :
                            PrefabCloneUtil.RectTransformClone(firstItem, content);
                        itemViewObject.transform.localPosition = deltaPos;
                        itemViewObject.name = name + (rows * i + j);

                        deltaPos.y += y;
                    }
                    deltaPos.x += x; //下一列
                    deltaPos.y -= y * rows;
                }
            }
        }

        private void BindScrollEvt()
        {
            //4.监听边界
            Vector3[] corners0 = new Vector3[4];
            Vector3[] corners1 = new Vector3[4];
            Vector3[] viewportCorners = new Vector3[4];

            if (scrollDir == Direction.Vertical)
            {
                int lastRowFirstIdx = rows * cols; //最后一行的第一个索引
                int lastFirst = lastRowFirstIdx - cols; //倒数第二行的第一个索引

                _runtime.scrollRect.onValueChanged.AddListener((v2) =>
                {
                    Transform content = _runtime.scrollRect.content;
                    content.GetChild(0).GetComponent<RectTransform>().GetWorldCorners(corners0);
                    content.GetChild(lastFirst).GetComponent<RectTransform>().GetWorldCorners(corners1);
                    //5.计算可视域的边界
                    _runtime.scrollRect.GetComponent<RectTransform>().GetWorldCorners(viewportCorners);
                    //监听第一行的第一个以及倒数第二行的第一个
                    if (corners0[0].y > viewportCorners[1].y && _runtime.trial > _runtime.head)
                    {
                        VercitalListenerCorners0(ref _runtime, content, lastRowFirstIdx);
                    }
                    else if (corners1[0].y < viewportCorners[0].y && _runtime.head > 0)
                    {
                        VercitalListenerCorners1(ref _runtime, content, lastRowFirstIdx);
                    }
                });
            }
            else
            {
                int lastColFirstIdx = cols * rows; //最后一列的第一个索引
                int lastFirst = lastColFirstIdx - rows; //倒数第二列的第一个索引

                _runtime.scrollRect.onValueChanged.AddListener((v2) =>
                {
                    Transform content = _runtime.scrollRect.content;
                    content.GetChild(0).GetComponent<RectTransform>().GetWorldCorners(corners0);
                    content.GetChild(lastFirst).GetComponent<RectTransform>().GetWorldCorners(corners1);
                    //5.计算可视域的边界
                    _runtime.scrollRect.GetComponent<RectTransform>().GetWorldCorners(viewportCorners);
                    //监听第一列的第一个和倒数第二列的第一个
                    if (corners0[2].x < viewportCorners[1].x && _runtime.trial > _runtime.head)
                    {
                        HorizontalListenerCorners0(ref _runtime, content, lastColFirstIdx);
                    } //倒数第二列的第一个正在向右移动 
                    else if (corners1[3].x > viewportCorners[3].x && _runtime.head > 0)
                    {
                        HorizontalListenerCorners1(ref _runtime, content, lastColFirstIdx);
                    }
                });
            }
        }


        #region 算法实现

        private void VercitalListenerCorners0(ref RuntimeData runtime, Transform content, int lastRowFirstIdx)
        {
            List<IBindableModel> data = runtime.models; //指令优化
            int dataCount = data.Count;
            //将第一行的所有Item移动到最后一行
            for (int i = 0; i < cols; i++)
            {
                Vector3 pos = content.GetChild(lastRowFirstIdx + i).localPosition;
                pos.y += y;

                Transform itemTrans = content.GetChild(i);
                itemTrans.localPosition = pos; //位置修改

                //渲染数据 先计算指针再渲染数据
                runtime.trial = (runtime.trial + 1) % dataCount;
                runtime.head = (runtime.head + 1) % dataCount;
                //当前数据不是脏数据才进行重新渲染
                if (!runtime.isDirty) {
                    Vue.Router.GetView(itemTrans.name).RebindModel(data[runtime.trial]);
                }
                //如果没有设置无限滚动则数据渲染到底时隐藏显示，但是任然进行数据渲染
                if (runtime.trial < runtime.head) { itemTrans.gameObject.SetActive(false); }
            }

            for (int i = 0; i < cols; i++)
            {
                content.GetChild(0).SetAsLastSibling(); //逻辑位置修改
            }
        }

        private void VercitalListenerCorners1(ref RuntimeData runtime, Transform content, int lastRowFirstIdx)
        {
            List<IBindableModel> data = runtime.models; //指令优化
            int dataCount = data.Count;
            //将最后一行移动到第一行
            for (int i = cols - 1; i >= 0; i--) //保证数据显示的顺序正确性
            {
                Vector3 pos = content.GetChild(i).localPosition;
                pos.y -= y;
                Transform itemTrans = content.GetChild(lastRowFirstIdx + i);
                itemTrans.localPosition = pos; //位置修改

                //渲染数据 先计算指针再渲染数据
                runtime.trial = (runtime.trial + dataCount - 1) % dataCount;
                runtime.head = (runtime.head + dataCount - 1) % dataCount;
                //当前数据不是脏数据才进行重新渲染
                if (!runtime.isDirty) {
                    Vue.Router.GetView(itemTrans.name).RebindModel(data[runtime.head]);
                }
                //向下滑动全部显示
                if (!itemTrans.gameObject.activeSelf) { itemTrans.gameObject.SetActive(true); }
            }

            int lastIdx = content.childCount - 1;
            for (int i = cols - 1; i >= 0; i--)
            {
                content.GetChild(lastIdx).SetAsFirstSibling(); //逻辑位置修改
            }
        }

        private void HorizontalListenerCorners0(ref RuntimeData runtime, Transform content, int lastColFirstIdx)
        {
            List<IBindableModel> data = runtime.models; //指令优化
            //向左滑动了一个Item的距离
            int dataCount = data.Count;
            //将第一列全部移动到最后一列
            for (int i = 0; i < rows; i++)
            {
                Vector3 pos = content.GetChild(lastColFirstIdx + i).localPosition;
                pos.x += x;
                Transform itemTrans = content.GetChild(i);
                itemTrans.localPosition = pos; //位置改变

                //渲染数据 先计算指针再渲染数据
                runtime.trial = (runtime.trial + 1) % dataCount;
                runtime.head = (runtime.head + 1) % dataCount;
                //当前数据不是脏数据才进行重新渲染
                if (!runtime.isDirty) {
                    Vue.Router.GetView(itemTrans.name).RebindModel(data[runtime.trial]);
                }
                //如果没有设置无限滚动则数据渲染到底时隐藏显示，但是任然进行数据渲染
                if (runtime.trial < runtime.head) { itemTrans.gameObject.SetActive(false); }
            }

            for (int i = 0; i < rows; i++)
            {
                content.GetChild(0).SetAsLastSibling(); //逻辑位置的改变
            }
        }

        private void HorizontalListenerCorners1(ref RuntimeData runtime,Transform content, int lastColFirstIdx)
        {
            List<IBindableModel> data = runtime.models; //指令优化
            //当前正在向右滑动
            int dataCount = runtime.models.Count;
            //将最后一列全部移动到第一列 →
            for (int i = rows - 1; i >= 0; i--)
            {
                Vector3 pos = content.GetChild(i).localPosition;
                pos.x -= x;
                Transform itemTrans = content.GetChild(lastColFirstIdx + i);
                itemTrans.localPosition = pos; //位置的改变

                //渲染数据 先计算指针再渲染数据
                runtime.trial = (runtime.trial + dataCount - 1) % dataCount;
                runtime.head = (runtime.head + dataCount - 1) % dataCount;
                //当前数据不是脏数据才进行重新渲染
                if (!runtime.isDirty) {
                    Vue.Router.GetView(itemTrans.name).RebindModel(data[runtime.head]);
                }
                //向右滑动全部显示
                if (!itemTrans.gameObject.activeSelf) { itemTrans.gameObject.SetActive(true); }
            }

            int lastIdx = content.childCount - 1;
            for (int i = rows - 1; i >= 0; i--)
            {
                content.GetChild(lastIdx).SetAsFirstSibling(); //逻辑位置修改
            }
        }

        #endregion
    }

}
