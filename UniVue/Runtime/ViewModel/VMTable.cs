using System.Collections.Generic;
using UniVue.Internal;
using UniVue.Model;

namespace UniVue.ViewModel
{
    public sealed class VMTable
    {
        /// <summary>
        /// key=模型  value=绑定了此模型的所有视图的viewName
        /// </summary>
        private readonly Dictionary<IBindableModel, List<string>> _models;

        /// <summary>
        /// key=viewName value=此视图的所有ModelUI
        /// </summary>
        private readonly Dictionary<string, List<ModelUI>> _views;

        public VMTable(int tableSize)
        {
            _models = new Dictionary<IBindableModel, List<string>>(tableSize);
            _views = new Dictionary<string, List<ModelUI>>(tableSize);
        }


        public void AddVM(string viewName, ModelUI modelUI)
        {
            if (_models.TryGetValue(modelUI.Model, out List<string> viewNames))
                viewNames.Add(viewName);
            else
            {
                viewNames = (List<string>)CachePool.GetCache(InternalType.List_String);
                viewNames.Add(viewName);
                _models.Add(modelUI.Model, viewNames);
            }

            if (_views.TryGetValue(viewName, out List<ModelUI> modelUIs))
                modelUIs.Add(modelUI);
            else
            {
                modelUIs = (List<ModelUI>)CachePool.GetCache(InternalType.List_ModelUI);
                modelUIs.Add(modelUI);
                _views.Add(viewName, modelUIs);
            }
        }

        public bool TryGetModelUIs(string viewName, out List<ModelUI> modelUIs)
        {
            return _views.TryGetValue(viewName, out modelUIs);
        }


        public bool TryGetViews(IBindableModel model, out List<string> views)
        {
            return _models.TryGetValue(model, out views);
        }

        public IEnumerable<List<ModelUI>> GetAllModelUI()
        {
            foreach (var modelUIs in _views.Values)
            {
                yield return modelUIs;
            }
        }

        public void Rebind(string viewName, IBindableModel newModel, string modelName)
        {
            if (_views.TryGetValue(viewName, out List<ModelUI> modelUIs))
            {
                for (int i = 0; i < modelUIs.Count; i++)
                {
                    IBindableModel oldModel = modelUIs[i].Model;
                    if (modelUIs[i].TryRebind(modelName, newModel))
                    {
                        RemoveBindView(viewName, oldModel);
                        AddBindView(viewName, newModel);
                    }
                }
            }
        }

        public void Unbind(IBindableModel model)
        {
            if (TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (TryGetModelUIs(views[i], out List<ModelUI> modelUIs))
                        for (int j = 0; j < modelUIs.Count; j++)
                            if (modelUIs[j].active && ReferenceEquals(modelUIs[j].Model, model))
                                modelUIs[j].Unbind();
        }

        public bool ContainsViewName(string viewName)
        {
            return _views.ContainsKey(viewName);
        }

        private void RemoveBindView(string viewName, IBindableModel model)
        {
            if (_models.TryGetValue(model, out List<string> viewNames) && viewNames.Contains(viewName))
                viewNames.Remove(viewName);
        }


        private void AddBindView(string viewName, IBindableModel model)
        {
            if (_models.TryGetValue(model, out List<string> viewNames) && !viewNames.Contains(viewName))
                viewNames.Add(viewName);
            else if (viewNames == null)
            {
                viewNames = (List<string>)CachePool.GetCache(InternalType.List_String);
                viewNames.Add(viewName);
                _models.Add(model, viewNames);
            }
        }

        internal void ClearTable()
        {
            foreach (var list in _models.Values)
            {
                list.Clear();
                CachePool.AddCache(InternalType.List_String, list, false);
            }
            foreach (var list in _views.Values)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Destroy();
                }
                list.Clear();
                CachePool.AddCache(InternalType.List_ModelUI, list, false);
            }
            _models.Clear();
            _views.Clear();
        }

    }
}
