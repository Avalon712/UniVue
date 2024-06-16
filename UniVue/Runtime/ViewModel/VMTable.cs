using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;

namespace UniVue.ViewModel
{
    public class VMTable
    {
        /// <summary>
        /// 所有的UIBundle
        /// </summary>
        private List<UIBundle> _bundles;

        /// <summary>
        /// key = model的哈希值
        /// value = 绑定的视图
        /// </summary>
        private Dictionary<int, List<string>> _models;

        /// <summary>
        /// key = viewName
        /// value = 所有的UIBundle
        /// Dictionary<int, int> : key = 类型码 value = bundles中的索引index
        /// </summary>
        private Dictionary<string, Dictionary<int, int>> _views;

        public VMTable(List<UIBundle> bundles)
        {
            _bundles = bundles;
            _models = new Dictionary<int, List<string>>();
            _views = new Dictionary<string, Dictionary<int, int>>();
            for (int i = 0; i < bundles.Count; i++)
            {
                UpdateTable_OnAdded(bundles[i], i);
            }
        }


        public void UpdateTable_OnAdded(UIBundle bundle, int index)
        {
            int hashCode = bundle.Model.GetHashCode();
            int typeCode = bundle.Model.GetType().GetHashCode();
            string viewName = bundle.ViewName;

            if (_models.TryGetValue(hashCode, out List<string> views) && !views.Contains(viewName))
                views.Add(viewName);
            else
                _models.Add(hashCode, new List<string> { viewName });

            if (_views.TryGetValue(viewName, out Dictionary<int, int> table))
                table.Add(typeCode, index);
            else
            {
                table = new Dictionary<int, int>() { { typeCode, index } };
                _views.Add(viewName, table);
            }
        }

        public void Rebind<T>(T newModel, string viewName) where T : IBindableModel
        {
            int hashCodeNew = newModel.GetHashCode();
            int typeCodeNew = newModel.GetType().GetHashCode();

            UIBundle bundle = _bundles[_views[viewName][typeCodeNew]];
            IBindableModel oldModel = bundle.Model;
            int typeCodeOld = oldModel.GetType().GetHashCode();
            int hashCodeOld = oldModel.GetHashCode();

            if (!ReferenceEquals(oldModel, newModel) && typeCodeNew == typeCodeOld)
            {
                if (_models.TryGetValue(hashCodeOld, out List<string> viewsOld) && viewsOld.Contains(viewName))
                    viewsOld.Remove(viewName);

                if (_models.TryGetValue(hashCodeNew, out List<string> viewsNew) && !viewsNew.Contains(viewName))
                    viewsNew.Add(viewName);
                else
                    _models.Add(hashCodeNew, new List<string>() { viewName });

                bundle.Rebind(newModel);
            }
        }

        public UIBundle GetBundle(int typeCode, string viewName)
        {
            if (_views.TryGetValue(viewName, out Dictionary<int, int> table) && table.TryGetValue(typeCode, out int index))
            {
                return _bundles[index];
            }
            return null;
        }

        public void UpdateUI<T>(T model, string propertyName, int propertyValue) where T : IBindableModel
        {
            int hashCode = model.GetHashCode();
            int typeCode = model.GetType().GetHashCode();

            if (_models.TryGetValue(hashCode, out List<string> views))
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (_views.TryGetValue(views[i], out Dictionary<int, int> table) && table.TryGetValue(typeCode, out int index))
                    {
                        _bundles[index].UpdateUI(propertyName, propertyValue);
                    }
                }
            }
        }


        public void UpdateUI<T>(T model, string propertyName, float propertyValue) where T : IBindableModel
        {
            int hashCode = model.GetHashCode();
            int typeCode = model.GetType().GetHashCode();

            if (_models.TryGetValue(hashCode, out List<string> views))
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (_views.TryGetValue(views[i], out Dictionary<int, int> table) && table.TryGetValue(typeCode, out int index))
                    {
                        _bundles[index].UpdateUI(propertyName, propertyValue);
                    }
                }
            }
        }


        public void UpdateUI<T>(T model, string propertyName, string propertyValue) where T : IBindableModel
        {
            int hashCode = model.GetHashCode();
            int typeCode = model.GetType().GetHashCode();

            if (_models.TryGetValue(hashCode, out List<string> views))
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (_views.TryGetValue(views[i], out Dictionary<int, int> table) && table.TryGetValue(typeCode, out int index))
                    {
                        _bundles[index].UpdateUI(propertyName, propertyValue);
                    }
                }
            }
        }


        public void UpdateUI<T>(T model, string propertyName, bool propertyValue) where T : IBindableModel
        {
            int hashCode = model.GetHashCode();
            int typeCode = model.GetType().GetHashCode();

            if (_models.TryGetValue(hashCode, out List<string> views))
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (_views.TryGetValue(views[i], out Dictionary<int, int> table) && table.TryGetValue(typeCode, out int index))
                    {
                        _bundles[index].UpdateUI(propertyName, propertyValue);
                    }
                }
            }
        }


        public void UpdateUI<T>(T model, string propertyName, Sprite propertyValue) where T : IBindableModel
        {
            int hashCode = model.GetHashCode();
            int typeCode = model.GetType().GetHashCode();

            if (_models.TryGetValue(hashCode, out List<string> views))
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (_views.TryGetValue(views[i], out Dictionary<int, int> table) && table.TryGetValue(typeCode, out int index))
                    {
                        _bundles[index].UpdateUI(propertyName, propertyValue);
                    }
                }
            }
        }


        public void UpdateUI<T>(T model, string propertyName, List<int> propertyValue) where T : IBindableModel
        {
            int hashCode = model.GetHashCode();
            int typeCode = model.GetType().GetHashCode();

            if (_models.TryGetValue(hashCode, out List<string> views))
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (_views.TryGetValue(views[i], out Dictionary<int, int> table) && table.TryGetValue(typeCode, out int index))
                    {
                        _bundles[index].UpdateUI(propertyName, propertyValue);
                    }
                }
            }
        }


        public void UpdateUI<T>(T model, string propertyName, List<float> propertyValue) where T : IBindableModel
        {
            int hashCode = model.GetHashCode();
            int typeCode = model.GetType().GetHashCode();

            if (_models.TryGetValue(hashCode, out List<string> views))
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (_views.TryGetValue(views[i], out Dictionary<int, int> table) && table.TryGetValue(typeCode, out int index))
                    {
                        _bundles[index].UpdateUI(propertyName, propertyValue);
                    }
                }
            }
        }


        public void UpdateUI<T>(T model, string propertyName, List<string> propertyValue) where T : IBindableModel
        {
            int hashCode = model.GetHashCode();
            int typeCode = model.GetType().GetHashCode();

            if (_models.TryGetValue(hashCode, out List<string> views))
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (_views.TryGetValue(views[i], out Dictionary<int, int> table) && table.TryGetValue(typeCode, out int index))
                    {
                        _bundles[index].UpdateUI(propertyName, propertyValue);
                    }
                }
            }
        }


        public void UpdateUI<T>(T model, string propertyName, List<bool> propertyValue) where T : IBindableModel
        {
            int hashCode = model.GetHashCode();
            int typeCode = model.GetType().GetHashCode();

            if (_models.TryGetValue(hashCode, out List<string> views))
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (_views.TryGetValue(views[i], out Dictionary<int, int> table) && table.TryGetValue(typeCode, out int index))
                    {
                        _bundles[index].UpdateUI(propertyName, propertyValue);
                    }
                }
            }
        }


        public void UpdateUI<T>(T model, string propertyName, List<Sprite> propertyValue) where T : IBindableModel
        {
            int hashCode = model.GetHashCode();
            int typeCode = model.GetType().GetHashCode();

            if (_models.TryGetValue(hashCode, out List<string> views))
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (_views.TryGetValue(views[i], out Dictionary<int, int> table) && table.TryGetValue(typeCode, out int index))
                    {
                        _bundles[index].UpdateUI(propertyName, propertyValue);
                    }
                }
            }
        }


        public void UpdateUI<T, V>(V model, string propertyName, List<T> propertyValue) where T : Enum
        {
            int hashCode = model.GetHashCode();
            int typeCode = model.GetType().GetHashCode();

            if (_models.TryGetValue(hashCode, out List<string> views))
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (_views.TryGetValue(views[i], out Dictionary<int, int> table) && table.TryGetValue(typeCode, out int index))
                    {
                        _bundles[index].UpdateUI(propertyName, propertyValue);
                    }
                }
            }
        }

        public void Destroy()
        {
            _models.Clear();
            _views.Clear();
            _models = null;
            _views = null;
        }
    }
}
