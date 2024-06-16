using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;
using UniVue.Utils;
using UniVue.ViewModel.Models;

namespace UniVue.ViewModel
{
    public static class UIQuerier
    {
        /// <summary>
        /// 查询指定类型的UI组件
        /// </summary>
        /// <typeparam name="T">UI类型</typeparam>
        /// <param name="viewName">视图名称</param>
        /// <param name="modelName">模型名</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>所有找到的UI组件</returns>
        public static IEnumerable<T> Query<T>(string viewName, string modelName, string propertyName) where T : Component
        {
            List<UIBundle> bundles = Vue.Updater.Bundles;
            for (int i = 0; i < bundles.Count; i++)
            {
                if (bundles[i].ViewName == viewName && bundles[i].ModelName == modelName)
                {
                    List<PropertyUI> propertyUIs = bundles[i].ProertyUIs;
                    for (int j = 0; j < propertyUIs.Count; j++)
                    {
                        if (propertyUIs[j].PropertyName == propertyName)
                        {
                            using (var it = propertyUIs[j].GetUI<T>().GetEnumerator())
                            {
                                while (it.MoveNext())
                                {
                                    yield return it.Current;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查询指定类型的UI组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName">视图名称</param>
        /// <param name="model">模型</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>IEnumerable<T></returns>
        public static IEnumerable<T> Query<T>(string viewName, IBindableModel model, string propertyName) where T : Component
        {
            List<UIBundle> bundles = Vue.Updater.Bundles;
            for (int i = 0; i < bundles.Count; i++)
            {
                if (bundles[i].ViewName == viewName && ReferenceEquals(bundles[i].Model, model))
                {
                    List<PropertyUI> propertyUIs = bundles[i].ProertyUIs;
                    for (int j = 0; j < propertyUIs.Count; j++)
                    {
                        if (propertyUIs[j].PropertyName == propertyName)
                        {
                            using (var it = propertyUIs[j].GetUI<T>().GetEnumerator())
                            {
                                while (it.MoveNext())
                                {
                                    yield return it.Current;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查询指定类型的UI组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">模型</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>IEnumerable<T></returns>
        public static IEnumerable<T> Query<T>(IBindableModel model, string propertyName) where T : Component
        {
            List<UIBundle> bundles = Vue.Updater.Bundles;
            for (int i = 0; i < bundles.Count; i++)
            {
                if (ReferenceEquals(bundles[i].Model, model))
                {
                    List<PropertyUI> propertyUIs = bundles[i].ProertyUIs;
                    for (int j = 0; j < propertyUIs.Count; j++)
                    {
                        if (propertyUIs[j].PropertyName == propertyName)
                        {
                            using (var it = propertyUIs[j].GetUI<T>().GetEnumerator())
                            {
                                while (it.MoveNext())
                                {
                                    yield return it.Current;
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 查询某个GameObject下所有符合条件的UI组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject">GameObject</param>
        /// <param name="match">匹配条件</param>
        /// <returns>IEnumerable<T></returns>
        public static IEnumerable<T> Query<T>(GameObject gameObject, Predicate<T> match) where T : Component
        {
            List<T> components = ComponentFindUtil.GetAllComponents<T>(gameObject);
            for (int i = 0; i < components.Count; i++)
            {
                if (match(components[i]))
                {
                    yield return components[i];
                }
            }
        }

        /// <summary>
        /// 查询某个视图是否已经绑定了指定模型类型的UIBundle对象
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="viewName">视图名称</param>
        /// <param name="model">模型</param>
        /// <returns>已经生成过的此类型的UIBundle对象</returns>
        public static UIBundle Query<T>(string viewName, T model) where T : IBindableModel
        {
            VMTable table = Vue.Updater.Table;
            if (table != null)
            {
                int typeCode = model.GetType().GetHashCode();
                return table.GetBundle(typeCode, viewName);
            }

            List<UIBundle> bundles = Vue.Updater.Bundles;
            Type type = model.GetType();
            for (int i = 0; i < bundles.Count; i++)
            {
                if (bundles[i].ViewName == viewName && type == bundles[i].Model.GetType())
                {
                    return bundles[i];
                }
            }

            return null;
        }

        /// <summary>
        /// 查询所有符合条件的UIBundle对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="match">匹配条件</param>
        /// <returns>IEnumerable<UIBundle></returns>
        public static IEnumerable<UIBundle> Query(Predicate<UIBundle> match)
        {
            List<UIBundle> bundles = Vue.Updater.Bundles;
            for (int i = 0; i < bundles.Count; i++)
            {
                if (match(bundles[i]))
                {
                    yield return bundles[i];
                }
            }
        }

        /// <summary>
        /// 查询UIBundle
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="modelName">模型名称</param>
        /// <returns>IEnumerable<UIBundle></returns>
        public static IEnumerable<UIBundle> Query(string viewName, string modelName)
        {
            List<UIBundle> bundles = Vue.Updater.Bundles;
            for (int i = 0; i < bundles.Count; i++)
            {
                if (bundles[i].ViewName == viewName && bundles[i].ModelName == modelName)
                {
                    yield return bundles[i];
                }
            }
        }

    }
}
