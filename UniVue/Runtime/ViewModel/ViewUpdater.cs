﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 视图渲染器，更新视图
    /// </summary>
    public sealed class ViewUpdater
    {
        internal ViewUpdater() { Table = new VMTable(Vue.Config.TabelSize); }

        /// <summary>
        /// 当前发布更新UI通知消息的属性UI，在进行UI更新时将会排除此UI
        /// 此属性由PropertyUI进行填充，由ViewUpdater负责取消（即赋值为null）
        /// </summary>
        internal PropertyUI Publisher { get; set; }

        public VMTable Table { get; private set; }

        /// <summary>
        /// 添加UIBundle
        /// </summary>
        /// <remarks>只有被ViewUpdater维护的UIBundle才会进行UI更新</remarks>
        /// <param name="viewName">此UIBundle所属的视图</param>
        /// <param name="bundle">UIBundle</param>
        public void BindViewModel(string viewName, UIBundle bundle)
        {
            if (bundle != null)
                Table.BindVM(viewName, bundle);
        }


        public void UpdateUI<T>(T model, string propertyName, int propertyValue) where T : IBindableModel
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetBundles(views[i], out List<UIBundle> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI<T>(T model, string propertyName, float propertyValue) where T : IBindableModel
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetBundles(views[i], out List<UIBundle> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI<T>(T model, string propertyName, string propertyValue) where T : IBindableModel
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetBundles(views[i], out List<UIBundle> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI<T>(T model, string propertyName, bool propertyValue) where T : IBindableModel
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetBundles(views[i], out List<UIBundle> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI<T>(T model, string propertyName, Sprite propertyValue) where T : IBindableModel
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetBundles(views[i], out List<UIBundle> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI<T>(T model, string propertyName, List<int> propertyValue) where T : IBindableModel
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetBundles(views[i], out List<UIBundle> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI<T>(T model, string propertyName, List<float> propertyValue) where T : IBindableModel
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetBundles(views[i], out List<UIBundle> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI<T>(T model, string propertyName, List<string> propertyValue) where T : IBindableModel
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetBundles(views[i], out List<UIBundle> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI<T>(T model, string propertyName, List<bool> propertyValue) where T : IBindableModel
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetBundles(views[i], out List<UIBundle> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI<T>(T model, string propertyName, List<Sprite> propertyValue) where T : IBindableModel
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetBundles(views[i], out List<UIBundle> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }

        /// <summary>
        /// 更新List&lt;Enum&gt;
        /// </summary>
        public void UpdateUI(IBindableModel model, string propertyName, IList propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetBundles(views[i], out List<UIBundle> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI(string viewName, string propertyName, List<int> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetBundles(viewName, out List<UIBundle> bundles))
                for (int j = 0; j < bundles.Count; j++)
                    bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI(string viewName, string propertyName, List<float> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetBundles(viewName, out List<UIBundle> bundles))
                for (int j = 0; j < bundles.Count; j++)
                    bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI(string viewName, string propertyName, List<string> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetBundles(viewName, out List<UIBundle> bundles))
                for (int j = 0; j < bundles.Count; j++)
                    bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI(string viewName, string propertyName, List<bool> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetBundles(viewName, out List<UIBundle> _bundles))
                for (int j = 0; j < _bundles.Count; j++)
                    _bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI(string viewName, string propertyName, List<Sprite> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetBundles(viewName, out List<UIBundle> _bundles))
                for (int j = 0; j < _bundles.Count; j++)
                    _bundles[j].UpdateUI(propertyName, propertyValue);
            Publisher = null;
        }


        public void UpdateUI(string propertyName, string viewName, IConsumableModel consumer)
        {
            if (Table.TryGetBundles(viewName, out List<UIBundle> bundles))
            {
                for (int i = 0; i < bundles.Count; i++)
                {
                    consumer.UpdateUI(propertyName, bundles[i]);
                }
            }
        }


        public void UpdateUI(string viewName, IConsumableModel consumer)
        {
            if (Table.TryGetBundles(viewName, out List<UIBundle> bundles))
            {
                for (int i = 0; i < bundles.Count; i++)
                {
                    consumer.UpdateAll(bundles[i]);
                }
            }
        }

        /// <summary>
        /// 更新所有的枚举UI的值
        /// </summary>
        /// <remarks>当切换语言时调用</remarks>
        internal void UpdateEnumUI()
        {
            using (var it = Table.GetAllUIBundles().GetEnumerator())
            {
                while (it.MoveNext())
                {
                    List<UIBundle> bundles = it.Current;
                    for (int i = 0; i < bundles.Count; i++)
                    {
                        List<PropertyUI> propertyUIs = bundles[i].ProertyUIs;
                        for (int j = 0; j < propertyUIs.Count; j++)
                        {
                            if (propertyUIs[j] is EnumUI enumUI)
                            {
                                enumUI.UpdateUI();
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 卸载指定视图的所有UIBundle
        /// </summary>
        /// <param name="viewName">要卸载UIBundle的视图名称</param>
        public void UnloadBundle(string viewName)
        {
            Table.RemoveBundles(viewName);
        }

        /// <summary>
        /// 重新绑定模型
        /// </summary>
        /// <param name="viewName">需要重新绑定模型的视图名称</param>
        /// <param name="newModel">新模型</param>
        /// <param name="oldModel">旧模型</param>
        public void Rebind(string viewName, IBindableModel newModel, IBindableModel oldModel)
        {
            Table.Rebind(viewName, oldModel, newModel);
        }

        /// <summary>
        /// 重新绑定模型
        /// </summary>
        /// <param name="viewName">需要重新绑定模型的视图名称</param>
        /// <param name="newModel">新模型</param>
        public void Rebind(string viewName, IBindableModel newModel)
        {
            Table.Rebind(viewName, newModel);
        }


        /// <summary>
        ///  解除此模型与所有它绑定的视图的关系
        /// </summary>
        /// <remarks>实际上不会销毁已经创建的UIBundle对象，只是将其设置为失活状态。处于失活状态时无法进行UI更新以及通过UI更新模型</remarks>
        /// <param name="model">要解绑的模型</param>
        public void Unbind(IBindableModel model)
        {
            Table.Unbind(model);
        }


        /// <summary>
        /// 清空所有UIBundle
        /// </summary>
        public void ClearBundles()
        {
            Table.ClearTable();
        }

    }


}
