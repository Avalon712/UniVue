using System.Collections;
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
        internal ViewUpdater() 
        { 
            Table = new VMTable(Vue.Config.TabelSize); 
            Vue.OnLanguageEnvironmentChanged += OnLanguageEnvironmentChanged;
        }

        internal VMTable Table { get; }


        /// <summary>
        /// 当语言环境发生改变时
        /// </summary>
        private void OnLanguageEnvironmentChanged()
        {
            using (var it = Table.GetAllModelUI().GetEnumerator())
            {
                while (it.MoveNext())
                {
                    List<ModelUI> bundles = it.Current;
                    for (int i = 0; i < bundles.Count; i++)
                    {
                        List<PropertyUI> propertyUIs = bundles[i].ProertyUIs;
                        for (int j = 0; j < propertyUIs.Count; j++)
                        {
                            if (propertyUIs[j] is II18nPropertyUI i18n)
                            {
                                i18n.OnLanguageEnvironmentChanged();
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 添加ModelUI
        /// </summary>
        /// <remarks>只有被ViewUpdater维护的ModelUI才会进行UI更新</remarks>
        /// <param name="viewName">此ModelUI所属的视图</param>
        /// <param name="modelUI">ModelUI</param>
        internal void AddViewModel(string viewName, ModelUI modelUI)
        {
            if (modelUI != null)
            {
                Table.AddVM(viewName, modelUI);
            }
        }

        /// <summary>
        /// 更新所有绑定了model的视图
        /// </summary>
        public void UpdateAll(IBindableModel model)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetModelUIs(views[i], out List<ModelUI> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                model.UpdateAll(bundles[j]);
        }

        public void UpdateUI(IBindableModel model, string propertyName, int propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetModelUIs(views[i], out List<ModelUI> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);

        }

        public void UpdateUI(IBindableModel model, string propertyName, float propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetModelUIs(views[i], out List<ModelUI> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);

        }

        public void UpdateUI(IBindableModel model, string propertyName, string propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetModelUIs(views[i], out List<ModelUI> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);

        }

        public void UpdateUI(IBindableModel model, string propertyName, bool propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetModelUIs(views[i], out List<ModelUI> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);

        }

        public void UpdateUI(IBindableModel model, string propertyName, Sprite propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetModelUIs(views[i], out List<ModelUI> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);

        }

        public void UpdateUI(IBindableModel model, string propertyName, List<int> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetModelUIs(views[i], out List<ModelUI> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);

        }

        public void UpdateUI(IBindableModel model, string propertyName, List<float> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetModelUIs(views[i], out List<ModelUI> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);

        }

        public void UpdateUI(IBindableModel model, string propertyName, List<string> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetModelUIs(views[i], out List<ModelUI> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);

        }

        public void UpdateUI(IBindableModel model, string propertyName, List<bool> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetModelUIs(views[i], out List<ModelUI> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);

        }

        public void UpdateUI(IBindableModel model, string propertyName, List<Sprite> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetModelUIs(views[i], out List<ModelUI> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);

        }

        /// <summary>
        /// 更新List&lt;Enum&gt;
        /// </summary>
        public void UpdateUI(IBindableModel model, string propertyName, IList propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (table.TryGetModelUIs(views[i], out List<ModelUI> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].UpdateUI(propertyName, propertyValue);

        }

        public void UpdateUI(string viewName, string propertyName, List<int> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetModelUIs(viewName, out List<ModelUI> bundles))
                for (int j = 0; j < bundles.Count; j++)
                    bundles[j].UpdateUI(propertyName, propertyValue);
        }

        public void UpdateUI(string viewName, string propertyName, List<float> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetModelUIs(viewName, out List<ModelUI> bundles))
                for (int j = 0; j < bundles.Count; j++)
                    bundles[j].UpdateUI(propertyName, propertyValue);
        }

        public void UpdateUI(string viewName, string propertyName, List<string> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetModelUIs(viewName, out List<ModelUI> bundles))
                for (int j = 0; j < bundles.Count; j++)
                    bundles[j].UpdateUI(propertyName, propertyValue);

        }

        public void UpdateUI(string viewName, string propertyName, List<bool> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetModelUIs(viewName, out List<ModelUI> bundles))
                for (int j = 0; j < bundles.Count; j++)
                    bundles[j].UpdateUI(propertyName, propertyValue);
        }

        public void UpdateUI(string viewName, string propertyName, List<Sprite> propertyValue)
        {
            VMTable table = Table;
            if (table.TryGetModelUIs(viewName, out List<ModelUI> bundles))
                for (int j = 0; j < bundles.Count; j++)
                    bundles[j].UpdateUI(propertyName, propertyValue);
        }

        public void UpdateUI(IConsumableModel consumer, string propertyName, string viewName)
        {
            if (Table.TryGetModelUIs(viewName, out List<ModelUI> bundles))
            {
                for (int i = 0; i < bundles.Count; i++)
                {
                    consumer.UpdateUI(propertyName, bundles[i]);
                }
            }
        }

        public void UpdateUI(IConsumableModel consumer, string viewName)
        {
            if (Table.TryGetModelUIs(viewName, out List<ModelUI> bundles))
            {
                for (int i = 0; i < bundles.Count; i++)
                {
                    consumer.UpdateAll(bundles[i]);
                }
            }
        }

        /// <summary>
        /// 重新绑定模型
        /// </summary>
        /// <param name="viewName">需要重新绑定模型的视图名称</param>
        /// <param name="newModel">新模型</param>
        /// <param name="modelName">指定的模型名称</param>
        public void Rebind(string viewName, IBindableModel newModel, string modelName)
        {
            Table.Rebind(viewName, newModel, modelName);
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
        internal void UnloadResources()
        {
            Table.ClearTable();
        }

    }


}
