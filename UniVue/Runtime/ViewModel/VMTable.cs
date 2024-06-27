using System;
using System.Collections.Generic;
using UniVue.Model;

namespace UniVue.ViewModel
{
    public sealed class VMTable
    {
        /// <summary>
        /// key=模型  value=绑定了此模型的所有视图的viewName
        /// </summary>
        /// <remarks>适合不易经常进行Rebind()使用</remarks>
        private Dictionary<IBindableModel, List<string>> _models;

        /// <summary>
        /// key=viewName value=此视图的所有UIBundle
        /// </summary>
        /// <remarks>适用经常Rebind()使用</remarks>
        private Dictionary<string, List<UIBundle>> _views;

        public int ModelCount => _models.Count;

        public int BundleCount { get; private set; }

        public int PropertyUICount { get; private set; }


        public VMTable(int tableSize)
        {
            _models = new Dictionary<IBindableModel, List<string>>(tableSize);
            _views = new Dictionary<string, List<UIBundle>>(tableSize);
        }


        public void BindVM(string viewName, UIBundle bundle)
        {
            BindV(viewName, bundle);
            BindM(viewName, bundle);
        }


        public void BindV(string viewName, UIBundle bundle)
        {
            if (_views.TryGetValue(viewName, out List<UIBundle> bundles))
                bundles.Add(bundle);
            else
            {
                BundleCount += 1;
                PropertyUICount += bundle.ProertyUIs.Count;
                _views.Add(viewName, new List<UIBundle>(1) { bundle });
            }
        }


        public void BindM(string viewName, UIBundle bundle)
        {
            if (_models.TryGetValue(bundle.Model, out List<string> viewNames))
                viewNames.Add(viewName);
            else
                _models.Add(bundle.Model, new List<string>(1) { viewName });
        }


        public bool TryGetBundles(string viewName, out List<UIBundle> bundles)
        {
            return _views.TryGetValue(viewName, out bundles);
        }


        public bool TryGetViews(IBindableModel model, out List<string> views)
        {
            return _models.TryGetValue(model, out views);
        }

        public void RemoveBundles(string viewName)
        {
            if (_views.ContainsKey(viewName))
                _views.Remove(viewName);

            foreach (var views in _models.Values)
            {
                if (views.Contains(viewName))
                    views.Remove(viewName);
            }
        }

        public IEnumerable<List<UIBundle>> GetAllUIBundles()
        {
            foreach (var bundles in _views.Values)
            {
                yield return bundles;
            }
        }

        public void Rebind(string viewName, IBindableModel oldModel, IBindableModel newModel)
        {
            RemoveBindView(viewName, oldModel);
            AddBindView(viewName, newModel);

            if (_views.TryGetValue(viewName, out List<UIBundle> bundles))
            {
                for (int i = 0; i < bundles.Count; i++)
                {
                    if (ReferenceEquals(bundles[i].Model, oldModel))
                    {
                        bundles[i].Rebind(newModel);
                    }
                }
            }
        }


        public void Rebind(string viewName, IBindableModel newModel)
        {
            AddBindView(viewName, newModel);

            Type newModelType = newModel.GetType();
            if (_views.TryGetValue(viewName, out List<UIBundle> bundles))
            {
                for (int i = 0; i < bundles.Count; i++)
                {
                    IBindableModel oldModel = bundles[i].Model;
                    if (oldModel.GetType() == newModelType)
                    {
                        RemoveBindView(viewName, oldModel);
                        bundles[i].Rebind(newModel);
                    }
                }
            }
        }

        public void Unbind(IBindableModel model)
        {
            if (TryGetViews(model, out List<string> views))
                for (int i = 0; i < views.Count; i++)
                    if (TryGetBundles(views[i], out List<UIBundle> bundles))
                        for (int j = 0; j < bundles.Count; j++)
                            if (bundles[j].active && ReferenceEquals(bundles[j].Model, model))
                                bundles[j].Unbind();
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
                _models.Add(model, new List<string>(1) { viewName });
        }

        internal void ClearTable()
        {
            _models.Clear();
            _views.Clear();
        }

    }
}
