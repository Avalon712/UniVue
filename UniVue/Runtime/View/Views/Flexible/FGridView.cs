using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniVue.Model;
using UniVue.Utils;

namespace UniVue.View.Views
{
    public sealed class FGridView : FlexibleView
    {
        private GridWidget _gridComp;

        public FGridView(GridWidget gridComp, GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common) : base(viewObject, viewName, level)
        {
            _gridComp = gridComp;
        }

        public override void OnLoad()
        {
            var scrollRect = ComponentFindUtil.BreadthFind<ScrollRect>(viewObject);

            if (scrollRect == null)
            {
                throw new Exception("viewObject身上未包含一个ScrollRect组件，该功能依赖该组件！");
            }

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
        /// 重新绑定数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="newData">绑定的新数据，注意必须与旧数据的类型一致！</param>
        public void RebindData<T>(List<T> newData) where T : IBindableModel
        {
            _gridComp.RebindData(newData);
        }

        /// <summary>
        /// 绑定item数据
        /// <para>若为引用绑定，则无需使用AddData/RemoveData函数进行对数据的增删</para>
        /// </summary>
        /// <param name="data">绑定数据</param>
        public void BindData<T>(List<T> data) where T : IBindableModel
        {
            _gridComp.BindData(data);
        }

        /// <summary>
        /// 排序，本质上是对数据进行排序
        /// </summary>
        public void Sort(Comparison<IBindableModel> comparer)
        {
            _gridComp.Sort(comparer);
        }

        /// <summary>
        /// 添加数据(需要先绑定数据)
        /// </summary>
        /// <param name="newData">新加入的数据</param>
        public void AddData<T>(T newData) where T : IBindableModel
        {
            _gridComp.AddData(newData);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="remove">要移除的数据[如果知道索引则不用传递改参数]</param>
        public void RemoveData<T>(T remove) where T : IBindableModel
        {
            _gridComp.RemoveData(remove);
        }

        /// <summary>
        /// 视图刷新
        /// </summary>
        public void Refresh()
        {
            _gridComp.Refresh();
        }

        /// <summary>
        /// 清空数据
        /// </summary>
        public void Clear()
        {
            _gridComp.Clear();
        }
    }
}
