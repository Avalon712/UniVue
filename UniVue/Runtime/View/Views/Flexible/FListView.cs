using System.Collections.Generic;
using System;
using UnityEngine;
using UniVue.Model;
using UnityEngine.UI;
using UniVue.Utils;

namespace UniVue.View.Views
{
    public sealed class FListView : FlexibleView
    {
        private ListWidget _listComp;

        public bool Loop { get => _listComp.Loop; set => _listComp.Loop = value; }

        public FListView(ListWidget listComp, GameObject viewObject, string viewName = null, 
            ViewLevel level = ViewLevel.Common) : base(viewObject, viewName, level)
        {
            _listComp = listComp;
        }

        public override void OnLoad()
        {
            ScrollRect scrollRect = ComponentFindUtil.BreadthFind<ScrollRect>(viewObject);

            if (scrollRect == null)
            {
                throw new Exception("viewObject身上未包含一个ScrollRect组件，该功能依赖该组件！");
            }

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
