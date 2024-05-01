using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;
using UniVue.Utils;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 视图渲染器，更新视图
    /// </summary>
    public sealed class ViewUpdater 
    {
        private List<UIBundle> _bundles;
        internal ViewUpdater(){ _bundles = new List<UIBundle>(); }

        /// <summary>
        /// 双向绑定
        /// </summary>
        public void BindViewAndModel<T>(string viewName, T model, List<CustomTuple<Component, UIType>> uis, string modelName,bool allowUIUpdateModel) where T : IBindableModel
        {
            UIBundle bundle = UIBundleBuilder.Build(viewName, uis, model, modelName, allowUIUpdateModel);

            if(bundle != null)
            {
                _bundles.Add(bundle);
            }
#if UNITY_EDITOR
            else
            {
                LogUtil.Warning($"视图viewName={viewName}绑定模型数据失败，请检查命名是否符合规范");
            }
#endif
        }

        public void AddUIBundle(UIBundle bundle)
        {
            _bundles.Add(bundle);
        }

        /// <summary>
        /// 更新所有UI
        /// </summary>
        public void UpdateAll()
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                _bundles[i].Model.NotifyAll();
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, int propertyValue) where T : IBindableModel
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (ReferenceEquals(_bundles[i].Model,model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, float propertyValue) where T : IBindableModel
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, string propertyValue) where T : IBindableModel
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, bool propertyValue) where T : IBindableModel
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model,string propertyName, Sprite propertyValue) where T : IBindableModel
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
        }

        /// <summary>
        /// 卸载指定视图的所有UIBundle
        /// </summary>
        /// <param name="viewName">要卸载UIBundle的视图名称</param>
        public void UnloadBundle(string viewName)
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].Name == viewName)
                {
                    ListUtil.TrailDelete(_bundles, i--);
                }
            }
        }

        /// <summary>
        /// 重新绑定模型
        /// </summary>
        /// <param name="viewName">需要重新绑定模型的视图名称</param>
        /// <param name="newModel">新模型</param>
        /// <param name="oldModel">旧模型</param>
        public void Rebind<T>(string viewName,T newModel,T oldModel) where T : IBindableModel
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (ReferenceEquals(_bundles[i].Model, oldModel) && _bundles[i].Name == viewName)
                {
                    _bundles[i].Rebind(newModel);
                }
            }
        }

        /// <summary>
        /// 重新绑定模型
        /// </summary>
        /// <param name="viewName">需要重新绑定模型的视图名称</param>
        /// <param name="newModel">新模型</param>
        public void Rebind<T>(string viewName, T newModel) where T : IBindableModel
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].Name == viewName && newModel.GetType() == _bundles[i].Model.GetType())
                {
                    _bundles[i].Rebind(newModel);
                }
            }
        }

        /// <summary>
        /// 清空UIBundle
        /// </summary>
        public void ClearBundles()
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                _bundles[i].Dispose();
            }
            _bundles.Clear();
        }

    }


}
