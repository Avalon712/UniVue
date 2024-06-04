using System;
using System.Collections.Generic;
using static UnityEngine.UI.ScrollRect;
using UnityEngine.UI;
using UnityEngine;
using UniVue.Model;
using UniVue.Tween;
using UniVue.Utils;

namespace UniVue.View.Views
{
    public sealed class GridComp : IViewComp
    {
        private ScrollRect _scrollRect;         //必须的ScrollRect组件
        private Direction _scrollDir;           //只能选Vertical（垂直）、Horizontal（水平）
        private int _rows;                      //网格可见的视图行数(实际的行数=可见的行数+1)
        private int _cols;                      //网格可见的视图列数(实际的列数=可见的列数+1)
        private float _x;                       //leftItemPos.x+x=rightItemPos.x
        private float _y;                       //upItemPos.y+y=downItemPos.y
        private List<IBindableModel> _models;   //获取绑定的模型数据
        private int _tail;                      //数据尾指针
        private int _head;                      //数据头指针
        private bool _dirty;                    //当前数据是否已经发送修改
        private bool _flag;                     //是否已经生成UIBundle对象

        /// <summary>
        /// 构建Grid视图组件
        /// </summary>
        /// <param name="scrollRect">必须的ScrollRect组件</param>
        /// <param name="rows">网格可见的视图行数</param>
        /// <param name="cols">网格可见的视图列数</param>
        /// <param name="x">水平方向上相连两个Item的位置差:  x = rightItemPos.x - leftItemPos.x</param>
        /// <param name="y">垂直方向上相连两个Item的位置差:  y = downItemPos.y - upItemPos.y</param>
        /// <param name="scrollDir">滚动方向</param>
        public GridComp(ScrollRect scrollRect,int rows,int cols,float x,float y, Direction scrollDir = Direction.Vertical)
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
            else if (!ReferenceEquals(newData,_models))
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
        public void BindData<T>(List<T> data) where T : IBindableModel
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

            CreateItems();

            //设置显示区域的大小
            Resize();

            int len = _scrollRect.content.childCount;
            for (int i = 0; i < len; i++)
            {
                int k = i;
                RectTransform itemRect = _scrollRect.content.GetChild(k).GetComponent<RectTransform>();

                FlexibleView dynamicView = new FlexibleView(itemRect.gameObject, null, ViewLevel.Permanent);

                if (_tail < data.Count)
                {
                    dynamicView.BindModel(data[_tail++]);
                }
                else
                {
                    //这一步是为了生成UIBundle，以此使用RebindModel()函数
                    if (data.Count > 0) { dynamicView.BindModel(data[0]); _flag = true; }
                    itemRect.gameObject.SetActive(false);
                }
            }

            BindScrollEvt();

            _tail--; //恢复到正确位置
        }

        /// <summary>
        /// 排序，本质上是对数据进行排序
        /// </summary>
        public void Sort(Comparison<IBindableModel> comparer)
        {
            _models.Sort(comparer);
            _dirty = true;
            Refresh(); //刷新
        }

        /// <summary>
        /// 添加数据(需要先绑定数据)
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

            _dirty = !_models.Contains(newData);
            if (_dirty)
            {
                _models.Add(newData);
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
            _dirty = _models.Contains(remove);
            if (_dirty)
            {
                ListUtil.TrailDelete(_models, _models.IndexOf(remove));
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
            Transform content = _scrollRect.content;

            //水平滚动：0为最左边，1为最右边；    垂直滚动：1为顶部，0为顶部
            Vector2 endPos = _scrollDir == Direction.Vertical ? Vector2.up : Vector2.zero;

            //均匀滚动
            float duration = _scrollDir == Direction.Vertical ?
                Mathf.Abs(content.localPosition.y / _y) * 0.1f :
                Mathf.Abs(content.localPosition.x / _x) * 0.1f;

            TweenBehavior.DoScroll(_scrollRect, duration, endPos).Call(DoRefresh);
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
            //计算出此索引所在的行和列
            int r = index / _rows;
            int c = index / _cols;
            if (index <= _models.Count && index >= 0 && _head != index)
            {

            }
        }

        private void DoRefresh()
        {
            _head = 0;
            _tail = 0;
            Transform content = _scrollRect.content;
            for (int i = 0; i < content.childCount; i++)
            {
                GameObject itemObj = content.GetChild(i).gameObject;
                if (_tail < _models.Count)
                {
                    if (!itemObj.activeSelf)
                    {
                        itemObj.SetActive(true);
                    }
                    Vue.Router.GetView(itemObj.name).RebindModel(_models[_tail]);
                    _models[_tail++].NotifyAll();
                }
                else
                {
                    itemObj.SetActive(false);
                }
            }
            --_tail;
            _dirty = false;
        }


        /// <summary>
        /// 重新计算content的大小
        /// </summary>
        private void Resize()
        {
            Vector3 deltaPos = _scrollDir == Direction.Vertical ? 
                new Vector2(0, Mathf.Abs(_y)) : 
                new Vector2(Mathf.Abs(_x), 0);

            float temp = _scrollDir == Direction.Vertical ? 
                _models.Count / (float)_cols :
                _models.Count / (float)_rows;

            _scrollRect.content.sizeDelta = (Mathf.FloorToInt(temp)+1) * deltaPos;

            //当前是否可以移动
            _scrollRect.movementType = _models.Count < _cols * _rows ? MovementType.Clamped : MovementType.Elastic;
        }

        private void CreateItems()
        {
            //创建Item
            Transform content = _scrollRect.content;
            Vector3 deltaPos = content.GetChild(0).localPosition;
            GameObject firstItem = content.GetChild(0).gameObject;
            string name = firstItem.name;

            //按下面的方法确保contant的前面rows个为第一列或前cols个为第一行
            if (_scrollDir == Direction.Vertical) //垂直滚动时位置按行一行一行的设置
            {
                for (int i = 0; i <= _rows; i++)//垂直滚动多一行
                {
                    for (int j = 0; j < _cols; j++)
                    {
                        GameObject itemViewObject = i + j == 0 ? firstItem :
                            PrefabCloneUtil.RectTransformClone(firstItem, content);
                        itemViewObject.transform.localPosition = deltaPos;
                        itemViewObject.name = name + (_cols * i + j);
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
                        GameObject itemViewObject = i + j == 0 ? firstItem :
                            PrefabCloneUtil.RectTransformClone(firstItem, content);
                        itemViewObject.transform.localPosition = deltaPos;
                        itemViewObject.name = name + (_rows * i + j);

                        deltaPos.y += _y;
                    }
                    deltaPos.x += _x; //下一列
                    deltaPos.y -= _y * _rows;
                }
            }
        }

        private void BindScrollEvt()
        {
            //4.监听边界
            Vector3[] corners0 = new Vector3[4];
            Vector3[] corners1 = new Vector3[4];
            Vector3[] viewportCorners = new Vector3[4];

            if (_scrollDir == Direction.Vertical)
            {
                int lastRowFirstIdx = _rows * _cols; //最后一行的第一个索引
                int lastFirst = lastRowFirstIdx - _cols; //倒数第二行的第一个索引

                _scrollRect.onValueChanged.AddListener((v2) =>
                {
                    Transform content = _scrollRect.content;
                    content.GetChild(0).GetComponent<RectTransform>().GetWorldCorners(corners0);
                    content.GetChild(lastFirst).GetComponent<RectTransform>().GetWorldCorners(corners1);
                    //5.计算可视域的边界
                    _scrollRect.GetComponent<RectTransform>().GetWorldCorners(viewportCorners);
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
                    Transform content = _scrollRect.content;
                    content.GetChild(0).GetComponent<RectTransform>().GetWorldCorners(corners0);
                    content.GetChild(lastFirst).GetComponent<RectTransform>().GetWorldCorners(corners1);
                    //5.计算可视域的边界
                    _scrollRect.GetComponent<RectTransform>().GetWorldCorners(viewportCorners);
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

        public void Destroy()
        {
            _scrollRect.onValueChanged.RemoveAllListeners();
            _scrollRect = null;
            _models?.Clear();
            _models = null;
        }

        #region 算法实现

        private void VercitalListenerCorners0(Transform content, int lastRowFirstIdx)
        {
            List<IBindableModel> data = _models; //指令优化
            int dataCount = data.Count;
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
                //当前数据不是脏数据才进行重新渲染
                if (!_dirty)
                {
                    Vue.Router.GetView(itemTrans.name).RebindModel(data[_tail]);
                }
                //如果没有设置无限滚动则数据渲染到底时隐藏显示，但是任然进行数据渲染
                if (_tail < _head) { itemTrans.gameObject.SetActive(false); }
            }

            for (int i = 0; i < _cols; i++)
            {
                content.GetChild(0).SetAsLastSibling(); //逻辑位置修改
            }
        }

        private void VercitalListenerCorners1(Transform content, int lastRowFirstIdx)
        {
            List<IBindableModel> data = _models; //指令优化
            int dataCount = data.Count;
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
                //当前数据不是脏数据才进行重新渲染
                if (!_dirty)
                {
                    Vue.Router.GetView(itemTrans.name).RebindModel(data[_head]);
                }
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
            List<IBindableModel> data = _models; //指令优化
            //向左滑动了一个Item的距离
            int dataCount = data.Count;
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
                //当前数据不是脏数据才进行重新渲染
                if (!_dirty)
                {
                    Vue.Router.GetView(itemTrans.name).RebindModel(data[_tail]);
                }
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
            List<IBindableModel> data = _models; //指令优化
            //当前正在向右滑动
            int dataCount = _models.Count;
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
                //当前数据不是脏数据才进行重新渲染
                if (!_dirty)
                {
                    Vue.Router.GetView(itemTrans.name).RebindModel(data[_head]);
                }
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
