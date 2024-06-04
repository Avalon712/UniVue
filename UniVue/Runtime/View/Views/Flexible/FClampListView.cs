using System.Collections.Generic;
using System;
using UnityEngine;
using UniVue.Model;
using UniVue.Utils;

namespace UniVue.View.Views
{
    public sealed class FClampListView : FlexibleView
    {
        private ClampListComp _comp;

        public FClampListView(Transform content, GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common) : base(viewObject, viewName, level)
        {
            _comp = new(content); 
            BindEvent(content.gameObject);
        }

        public FClampListView(string content,GameObject viewObject, string viewName = null,
            ViewLevel level = ViewLevel.Common) : this(GameObjectFindUtil.BreadthFind(content, viewObject)?.transform, viewObject, viewName, level)
        {
        }

        public override void OnLoad()
        {
            
        }

        public override void OnUnload()
        {
            _comp.Destroy();
            _comp = null;
            base.OnUnload();
        }

        /// <summary>
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindData<T>(List<T> newData) where T : IBindableModel
        {
            _comp.RebindData(newData);
        }

        /// <summary>
        /// 为Item绑定显示数据
        /// </summary>
        /// <param name="data">绑定的数据</param>
        public void BindData<T>(List<T> data) where T : IBindableModel
        {
            _comp.BindData(data);
        }

        /// <summary>
        /// 对列表进行排序，排序规则
        /// </summary>
        /// <param name="comparer">排序规则</param>
        public void Sort(Comparison<IBindableModel> comparer)
        {
            _comp.Sort(comparer);
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            _comp.Clear();
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
        /// 刷新视图
        /// </summary>
        public void Refresh()
        {
            _comp.Refresh();
        }
    }
}
