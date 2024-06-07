using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;
using UniVue.Utils;
using UniVue.View.Views;

namespace UniVue.Runtime.View.Views
{
    /// <summary>
    /// 不能滚动的列表视图，可以方便实现显示和隐藏子物体
    /// </summary>
    public sealed class SClampListView : ScriptableView
    {

        #region 配置信息
        /// <summary>
        /// 显示所有子物体的父物体
        /// </summary>
        [Header("所有子物体的父物体的名称")]
        [SerializeField] private string _content;

        #endregion

        private ClampListWidget _comp;

        public override void OnLoad()
        {
            var content = GameObjectFindUtil.BreadthFind(_content, viewObject)?.transform;

            _comp = new(content);

            InitState();
            BindEvent(content.gameObject);
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
