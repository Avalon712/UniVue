using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Tween;
using UniVue.Utils;

namespace UniVue.View.Views
{
    /// <summary>
    /// 列表视图
    /// <para>注意:Item的锚点位置为左上角</para>
    /// </summary>
    public sealed class SListView : ScriptableView
    {
        #region 配置信息
        /// <summary>
        /// 设置滚动方向
        /// </summary>
        [Header("滚动方向 Horizontal|Vercital")]
        [SerializeField] internal Direction _scrollDir = Direction.Vertical;

        /// <summary>
        /// 是否循环滚动
        /// </summary>
        [Header("循环滚动")]
        [SerializeField] private bool _loop;

        /// <summary>
        /// 可见数量
        /// </summary>
        [Header("可见数量")]
        [SerializeField] private int _viewNum;

        /// <summary>
        /// 相连两个item在滚动方向上的距离
        /// </summary>
        [Header("相连两个item在滚动方向上的位置差")]
        [Tooltip("垂直方向滚动: upItem.localPos.y-downItem.localPos.y 水平方向滚动:rightItem.localPos.x-leftItem.localPos.x")]
        [SerializeField] private float _distance;

        #endregion

        private ListComp _listComp;

        public bool Loop { get => _listComp.Loop; set => _listComp.Loop = value; }

       
        public override void OnLoad()
        {
            ScrollRect scrollRect = ComponentFindUtil.BreadthFind<ScrollRect>(viewObject);

            if (scrollRect == null)
            {
                throw new Exception("viewObject身上未包含一个ScrollRect组件，该功能依赖该组件！");
            }
            _listComp = new(scrollRect,_distance, _viewNum, _scrollDir, _loop);

            InitState();
            //在进行查找UI组件时要排除content下面的物体（因为这下面的每个Item会作为一个单独的FlexibleView存在）
            BindEvent(scrollRect.content.gameObject);
        }

        public override void OnUnload()
        {
            _listComp.Destroy();
            _listComp = null;
            base.OnUnload();
        }

        /// <summary>
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindData<T>(List<T> newData) where T : IBindableModel
        {
            _listComp.RebindData(newData);
        }

        /// <summary>
        /// 为Item绑定显示数据
        /// </summary>
        /// <param name="data">绑定的数据</param>
        public void BindData<T>(List<T> data) where T : IBindableModel
        {
            _listComp.BindData(data);
        }

        /// <summary>
        /// 对列表进行排序，排序规则
        /// </summary>
        /// <param name="comparer">排序规则</param>
        public void Sort(Comparison<IBindableModel> comparer)
        {
            _listComp.Sort(comparer);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="newData">新加入的数据</param>
        public void AddData<T>(T newData) where T : IBindableModel
        {
            _listComp.AddData(newData);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        public void RemoveData<T>(T remove) where T : IBindableModel
        {
            _listComp.RemoveData(remove);
        }

        /// <summary>
        /// 刷新视图
        /// </summary>
        public void Refresh()
        {
            _listComp.Refresh();
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            _listComp.Clear();
        }

        /// <summary>
        /// 滚动到指定数据的哪儿
        /// </summary>
        public void ScrollTo<T>(T data) where T : IBindableModel
        {
            _listComp.ScrollTo(data);
        }
    }
}
