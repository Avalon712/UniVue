using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Utils;
using UniVue.View.Widgets;

namespace UniVue.View.Views
{
    public sealed class ListView : BaseView
    {
        private LoopList _listComp;

        public bool Loop { get => _listComp.Loop; set => _listComp.Loop = value; }

        public ListView(LoopList listComp, GameObject viewObject, string viewName = null,
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
        /// 绑定集合
        /// </summary>
        /// <remarks>使用这个函数可以防止数据的冗余。同时能够在数据发生变化时自动更新响应变化</remarks>
        public void BindList(IObservableList observer)
        {
            _listComp.BindList(observer);
        }

        /// <summary>
        /// 绑定集合
        /// </summary>
        /// <remarks>
        /// 使用这个函数可以防止数据的冗余。同时能够在数据发生变化时自动更新响应变化</remarks>
        public void RebindList(IObservableList observer)
        {
            _listComp.RebindList(observer);
        }

        /// <summary>
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindList<T>(List<T> newData) where T : IBindableModel
        {
            _listComp.RebindList(newData);
        }

        /// <summary>
        /// 为Item绑定显示数据
        /// </summary>
        /// <param name="data">绑定的数据</param>
        public void BindList<T>(List<T> data) where T : IBindableModel
        {
            _listComp.BindList(data);
        }

        /// <summary>
        /// 对列表进行排序，排序规则
        /// </summary>
        /// <param name="comparer">排序规则</param>
        public void Sort<T>(Comparison<T> comparer) where T : IBindableModel
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
        public void RemoveData<T>(T remove, int index = -1) where T : IBindableModel
        {
            _listComp.RemoveData(remove, index);
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

        /// <summary>
        /// 刷新视图
        /// </summary>
        /// <param name="force">是否强制刷新</param>
        public void Refresh(bool force = false)
        {
            _listComp.Refresh(force);
        }

        public override T GetWidget<T>()
        {
            return _listComp as T;
        }
    }
}
