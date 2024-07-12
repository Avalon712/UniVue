using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;
using UniVue.Utils;
using UniVue.View.Widgets;

namespace UniVue.View.Views
{
    public sealed class ClampListView : BaseView
    {
        private ClampList _comp;

        public ClampListView(ClampList comp, GameObject viewObject, ViewLevel level = ViewLevel.Common) : base(viewObject, level)
        {
            _comp = comp;
        }

        public override void OnUnload()
        {
            _comp.Destroy();
            _comp = null;
            base.OnUnload();
        }

        /// <summary>
        /// 绑定集合
        /// </summary>
        /// <remarks>使用这个函数可以防止数据的冗余。同时能够在数据发生变化时自动更新响应变化</remarks>
        public void BindList(IObservableList observer)
        {
            _comp.BindList(observer);
        }

        /// <summary>
        /// 绑定集合
        /// </summary>
        /// <remarks>
        /// 使用这个函数可以防止数据的冗余。同时能够在数据发生变化时自动更新响应变化</remarks>
        public void RebindList(IObservableList observer)
        {
            _comp.RebindList(observer);
        }

        /// <summary>
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindList<T>(List<T> newData) where T : IBindableModel
        {
            _comp.RebindList(newData);
        }

        /// <summary>
        /// 为Item绑定显示数据
        /// </summary>
        /// <param name="data">绑定的数据</param>
        public void BindList<T>(List<T> data) where T : IBindableModel
        {
            _comp.BindList(data);
        }

        /// <summary>
        /// 对列表进行排序，排序规则
        /// </summary>
        /// <param name="comparer">排序规则</param>
        public void Sort<T>(Comparison<T> comparer) where T : IBindableModel
        {
            _comp.Sort(comparer);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="newData">新加入的数据</param>
        public void AddData<T>(T newData) where T : IBindableModel
        {
            _comp.AddData(newData);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        public void RemoveData<T>(T remove) where T : IBindableModel
        {
            _comp.RemoveData(remove);
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            _comp.Clear();
        }


        /// <summary>
        /// 刷新视图
        /// </summary>
        /// <param name="force">是否强制刷新</param>
        public void Refresh()
        {
            _comp.Refresh();
        }

        public override T GetWidget<T>()
        {
            return _comp as T;
        }
    }
}
