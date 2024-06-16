using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;
using UniVue.Utils;
using UniVue.ViewModel.Models;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 视图渲染器，更新视图
    /// </summary>
    public sealed class ViewUpdater
    {
        private List<UIBundle> _bundles;
        internal ViewUpdater() { _bundles = new List<UIBundle>(); }

        /// <summary>
        /// 当前发布更新UI通知消息的属性UI，在进行UI更新时将会排除此UI
        /// 此属性由PropertyUI进行填充，由ViewUpdater负责取消（即赋值为null）
        /// </summary>
        internal PropertyUI Publisher { get; set; }

        internal List<UIBundle> Bundles => _bundles;

        internal VMTable Table { get; private set; }

        /// <summary>
        /// 双向绑定
        /// </summary>
        internal void BindViewAndModel<T>(string viewName, T model, List<ValueTuple<Component, UIType>> uis, string modelName, bool allowUIUpdateModel) where T : IBindableModel
        {
            UIBundle bundle = UIBundleBuilder.Build(viewName, uis, model, modelName, allowUIUpdateModel);

            if (bundle != null)
            {
                AddBundle(bundle);
            }
#if UNITY_EDITOR
            else
            {
                LogUtil.Warning($"视图viewName={viewName}绑定模型数据失败，请检查命名是否符合规范");
            }
#endif
        }

        /// <summary>
        /// 优化UIBundle的查询
        /// <para>
        /// #2024/6/16 尚未修复的bug: 当一个视图绑定了两个不同的对象但是是同一类型时，可能更新会出问题或建立查询表时会出异常。考虑到
        /// 一个视图很少有可能会绑定不同对象但是是同一类型的情况，因此还没准备修复。
        /// </para>
        /// </summary>
        /// <remarks>此项的设置受配置的影响，同时UIBundle数量没有500以上没有开启的必要。开启优化后会占用更多的内存</remarks>
        public void OptimizeQuery()
        {
            if (Table == null && Vue.Config.OptimizeQuery)
                Table = new VMTable(_bundles);
        }


        /// <summary>
        /// 添加UIBundle
        /// </summary>
        /// <remarks>只有被ViewUpdater维护的UIBundle才会进行UI更新</remarks>
        /// <param name="bundle">UIBundle</param>
        public void AddBundle(UIBundle bundle)
        {
            _bundles.Add(bundle);
            Table?.UpdateTable_OnAdded(bundle, _bundles.Count - 1);
        }

        /// <summary>
        /// 更新所有UI
        /// </summary>
        public void UpdateAll()
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active)
                {
                    _bundles[i].Model.NotifyAll();
                }
            }
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, int propertyValue) where T : IBindableModel
        {
            if (Table != null)
            {
                Table.UpdateUI(model, propertyName, propertyValue);
                Publisher = null;
                return;
            }

            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, float propertyValue) where T : IBindableModel
        {
            if (Table != null)
            {
                Table.UpdateUI(model, propertyName, propertyValue);
                Publisher = null;
                return;
            }

            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, string propertyValue) where T : IBindableModel
        {
            if (Table != null)
            {
                Table.UpdateUI(model, propertyName, propertyValue);
                Publisher = null;
                return;
            }

            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, bool propertyValue) where T : IBindableModel
        {
            if (Table != null)
            {
                Table.UpdateUI(model, propertyName, propertyValue);
                Publisher = null;
                return;
            }

            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, Sprite propertyValue) where T : IBindableModel
        {
            if (Table != null)
            {
                Table.UpdateUI(model, propertyName, propertyValue);
                Publisher = null;
                return;
            }

            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, List<int> propertyValue) where T : IBindableModel
        {
            if (Table != null)
            {
                Table.UpdateUI(model, propertyName, propertyValue);
                Publisher = null;
                return;
            }

            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, List<float> propertyValue) where T : IBindableModel
        {
            if (Table != null)
            {
                Table.UpdateUI(model, propertyName, propertyValue);
                Publisher = null;
                return;
            }

            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, List<string> propertyValue) where T : IBindableModel
        {
            if (Table != null)
            {
                Table.UpdateUI(model, propertyName, propertyValue);
                Publisher = null;
                return;
            }

            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, List<bool> propertyValue) where T : IBindableModel
        {
            if (Table != null)
            {
                Table.UpdateUI(model, propertyName, propertyValue);
                Publisher = null;
                return;
            }

            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T>(T model, string propertyName, List<Sprite> propertyValue) where T : IBindableModel
        {
            if (Table != null)
            {
                Table.UpdateUI(model, propertyName, propertyValue);
                Publisher = null;
                return;
            }

            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        public void UpdateUI<T, V>(V model, string propertyName, List<T> propertyValue) where T : Enum
        {
            if (Table != null)
            {
                Table.UpdateUI(model, propertyName, propertyValue);
                Publisher = null;
                return;
            }

            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="bundleName">UIBundle的名称</param>
        public void UpdateUI(string bundleName, string propertyName, List<int> propertyValue)
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && _bundles[i].ViewName == bundleName)
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="bundleName">UIBundle的名称</param>
        public void UpdateUI(string bundleName, string propertyName, List<float> propertyValue)
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && _bundles[i].ViewName == bundleName)
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="bundleName">UIBundle的名称</param>
        public void UpdateUI(string bundleName, string propertyName, List<string> propertyValue)
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && _bundles[i].ViewName == bundleName)
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="bundleName">UIBundle的名称</param>
        public void UpdateUI(string bundleName, string propertyName, List<bool> propertyValue)
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && _bundles[i].ViewName == bundleName)
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        /// <param name="bundleName">UIBundle的名称</param>
        public void UpdateUI(string bundleName, string propertyName, List<Sprite> propertyValue)
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].active && _bundles[i].ViewName == bundleName)
                {
                    _bundles[i].UpdateUI(propertyName, propertyValue);
                }
            }
            Publisher = null;
        }

        /// <summary>
        /// 卸载指定视图的所有UIBundle
        /// </summary>
        /// <param name="viewName">要卸载UIBundle的视图名称</param>
        public void UnloadBundle(string viewName)
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].ViewName == viewName)
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
        public void Rebind<T>(string viewName, T newModel, T oldModel) where T : IBindableModel
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (ReferenceEquals(_bundles[i].Model, oldModel) && _bundles[i].ViewName == viewName)
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
            if (Table != null)
            {
                Table.Rebind(newModel, viewName);
                return;
            }

            Type type = newModel.GetType();
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (_bundles[i].ViewName == viewName && type == _bundles[i].Model.GetType())
                {
                    _bundles[i].Rebind(newModel);
                }
            }
        }

        /// <summary>
        /// 将之前所有绑定了的模型全部更该为当前模型进行绑定
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">新模型</param>
        public void Rebind<T>(T model) where T : IBindableModel
        {
            string typeName = model.GetType().FullName;
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (typeName.Equals(_bundles[i].Model.GetType().FullName))
                {
                    _bundles[i].Unbind();
                }
            }
        }

        /// <summary>
        /// 将之前所有绑定了的模型全部更该为当前模型进行绑定
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modelName">模型名称</param>
        public void Rebind(string modelName)
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (modelName.Equals(_bundles[i].ModelName))
                {
                    _bundles[i].Unbind();
                }
            }
        }

        /// <summary>
        ///  解除此模型与所有它绑定的视图的关系
        /// </summary>
        /// <remarks>实际上不会销毁已经创建的UIBundle对象，只是将其设置为失活状态。处于失活状态时无法进行UI更新以及通过UI更新模型</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">要解绑的模型</param>
        public void Unbind<T>(T model) where T : IBindableModel
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                if (ReferenceEquals(_bundles[i].Model, model))
                {
                    _bundles[i].Unbind();
                }
            }
        }

        /// <summary>
        /// 清空UIBundle
        /// </summary>
        /// <remarks>这步会真的销毁所有UIBundle对象</remarks>
        public void ClearBundles()
        {
            for (int i = 0; i < _bundles.Count; i++)
            {
                _bundles[i].Destroy();
            }
            _bundles.Clear();

            Table?.Destroy();
            Table = null;
        }

    }


}
