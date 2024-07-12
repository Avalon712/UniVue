using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;
using UniVue.Utils;
using UniVue.View.Widgets;

namespace UniVue.View.Views
{
    public sealed class GridView : BaseView
    {
        private LoopGrid _gridComp;

        public GridView(LoopGrid gridComp, GameObject viewObject, ViewLevel level = ViewLevel.Common) : base(viewObject, level)
        {
            _gridComp = gridComp;
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
        public void Refresh(bool force = false)
        {
            _gridComp.Refresh(force);
        }

        public override T GetWidget<T>()
        {
            return _gridComp as T;
        }
    }
}
