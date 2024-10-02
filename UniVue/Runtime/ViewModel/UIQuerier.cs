using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Event;
using UniVue.Model;

namespace UniVue.ViewModel
{
    public static class UIQuerier
    {
        /// <summary>
        /// 查询指定类型的UI组件
        /// </summary>
        /// <typeparam name="T">UI类型</typeparam>
        /// <param name="viewName">视图名称</param>
        /// <param name="model">模型</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>所有找到的UI组件</returns>
        public static IEnumerable<T> QueryPropertyUI<T>(string viewName, IBindableModel model, string propertyName) where T : Component
        {
            if (Vue.Updater.Table.TryGetModelUIs(viewName, out List<ModelUI> modelUIs))
            {
                for (int i = 0; i < modelUIs.Count; i++)
                {
                    if (modelUIs[i].Model == model)
                    {
                        List<PropertyUI> propertyUIs = modelUIs[i].ProertyUIs;
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
        }


        /// <summary>
        /// 查询指定类型的UI组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="model">模型</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>IEnumerable<T></returns>
        public static IEnumerable<T> QueryPropertyUI<T>(IBindableModel model, string propertyName) where T : Component
        {
            if (Vue.Updater.Table.TryGetViews(model, out List<string> views))
            {
                for (int i = 0; i < views.Count; i++)
                {
                    if (Vue.Updater.Table.TryGetModelUIs(views[i], out List<ModelUI> modelUIs))
                    {
                        for (int j = 0; j < modelUIs.Count; j++)
                        {
                            if (modelUIs[j].Model == model)
                            {
                                List<PropertyUI> propertyUIs = modelUIs[j].ProertyUIs;
                                for (int k = 0; k < propertyUIs.Count; k++)
                                {
                                    if (propertyUIs[k].PropertyName == propertyName)
                                    {
                                        using (var it = propertyUIs[k].GetUI<T>().GetEnumerator())
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
                }
            }
        }

        /// <summary>
        /// 查询某个视图是否已经绑定了指定模型类型的ModelUI对象
        /// </summary>
        /// <typeparam name="T">模型类型</typeparam>
        /// <param name="viewName">视图名称</param>
        /// <param name="model">模型</param>
        /// <returns>已经生成过的此类型的ModelUI对象</returns>
        public static ModelUI QueryModelUI(string viewName, IBindableModel model)
        {
            string fullName = model.TypeInfo.typeFullName;
            if (Vue.Updater.Table.TryGetModelUIs(viewName, out List<ModelUI> modelUIs))
            {
                for (int i = 0; i < modelUIs.Count; i++)
                {
                    if (modelUIs[i].Model.TypeInfo.typeFullName == fullName)
                        return modelUIs[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 获取一个视图身上绑定的所有模型的ModelUI
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <returns>List<ModelUI></returns>
        public static List<ModelUI> QueryAllModelUI(string viewName)
        {
            if (Vue.Updater.Table.TryGetModelUIs(viewName, out List<ModelUI> modelUIs))
            {
                return modelUIs;
            }
            return null;
        }

        /// <summary>
        /// 查询所有符合条件的ModelUI对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="match">匹配条件</param>
        /// <returns>IEnumerable<ModelUI></returns>
        public static IEnumerable<ModelUI> QueryModelUI(Predicate<ModelUI> match)
        {
            using (var it = Vue.Updater.Table.GetAllModelUI().GetEnumerator())
            {
                while (it.MoveNext())
                {
                    List<ModelUI> modelUIs = it.Current;
                    for (int i = 0; i < modelUIs.Count; i++)
                    {
                        if (match(modelUIs[i]))
                        {
                            yield return modelUIs[i];
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 查询指定视图下指定事件的UIEvent
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="viewName">视图名称</param>
        /// <returns>UIEvent</returns>
        public static EventUI QueryEventUI(string eventName, string viewName)
        {
            List<EventUI> events = Vue.Event.Events;
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].EventName == eventName && events[i].ViewName == viewName)
                {
                    return events[i];
                }
            }
            return null;
        }

        /// <summary>
        /// 查询指定名称的事件的所有UIEvent
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <returns>IEnumerable<UIEvent>/returns>
        public static IEnumerable<EventUI> QueryEventUI(string eventName)
        {
            List<EventUI> events = Vue.Event.Events;
            for (int i = 0; i < events.Count; i++)
            {
                if (events[i].EventName == eventName)
                {
                    yield return events[i];
                }
            }
        }

    }
}
