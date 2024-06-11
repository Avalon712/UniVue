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
    /// 网格视图
    /// <para>注意:Item的锚点位置为左上角</para>
    /// </summary>
    public sealed class SGridView : ScriptableView 
    {
        #region 配置信息
        [Header("滚动方向 Horizontal|Vercital")]
        [SerializeField] private Direction scrollDir = Direction.Vertical;

        [Header("可见行数")]
        [SerializeField] private int rows;

        [Header("可见列数")]
        [SerializeField] private int cols;

        [Header("leftItemPos.x+x=rightItemPos.x")]
        [SerializeField] private float x;

        [Header("upItemPos.y+y=downItemPos.y")]
        [SerializeField] private float y;

        [Header("当刷新视图时是否播放滚动效果")]
        [SerializeField]
        private bool _playScrollEffectOnRefresh;
        #endregion

        private GridWidget _gridComp;

        public override void OnLoad()
        {
            var scrollRect = ComponentFindUtil.BreadthFind<ScrollRect>(viewObject);

            if (scrollRect == null)
            {
                throw new Exception("viewObject身上未包含一个ScrollRect组件，该功能依赖该组件！");
            }

            _gridComp = new(scrollRect, rows, cols, x, y, scrollDir) 
            { 
                PlayScrollEffectOnRefresh = _playScrollEffectOnRefresh
            };

            InitState();
            //在进行查找UI组件时要排除content下面的物体（因为这下面的每个Item会作为一个单独的FlexibleView存在）
            BindEvent(scrollRect.content.gameObject);
        }


        public override void OnUnload()
        {
            _gridComp.Destroy();
            _gridComp = null;
            base.OnUnload();
        }

        /// <summary>
        /// 绑定集合
        /// </summary>
        /// <remarks>使用这个函数可以防止数据的冗余。同时能够在数据发生变化时自动更新响应变化</remarks>
        public void BindList(IObservableList observer)
        {
            _gridComp.BindList(observer);
        }

        /// <summary>
        /// 绑定集合
        /// </summary>
        /// <remarks>
        /// 使用这个函数可以防止数据的冗余。同时能够在数据发生变化时自动更新响应变化</remarks>
        public void RebindList(IObservableList observer)
        {
            _gridComp.RebindList(observer);
        }

        /// <summary>
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindList<T>(List<T> newData) where T : IBindableModel
        {
            _gridComp.RebindList(newData);
        }

        /// <summary>
        /// 为Item绑定显示数据
        /// </summary>
        /// <param name="data">绑定的数据</param>
        public void BindList<T>(List<T> data) where T : IBindableModel
        {
            _gridComp.BindList(data);
        }

        /// <summary>
        /// 对列表进行排序，排序规则
        /// </summary>
        /// <param name="comparer">排序规则</param>
        public void Sort<T>(Comparison<T> comparer) where T : IBindableModel
        {
            _gridComp.Sort(comparer);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="newData">新加入的数据</param>
        public void AddData<T>(T newData) where T : IBindableModel
        {
            _gridComp.AddData(newData);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        public void RemoveData<T>(T remove, int index = -1) where T : IBindableModel
        {
            _gridComp.RemoveData(remove, index);
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            _gridComp.Clear();
        }


        /// <summary>
        /// 刷新视图
        /// </summary>
        /// <param name="force">是否强制刷新</param>
        public void Refresh(bool force=false)
        {
            _gridComp.Refresh(force);
        }
    }

}
