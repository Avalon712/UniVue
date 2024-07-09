using UnityEngine;
using UniVue.Input;
using UniVue.Model;
using UniVue.Rule;
using UniVue.View.Views;
using UniVue.ViewModel;

namespace UniVue.Utils
{
    /// <summary>
    /// 视图封装工具
    /// </summary>
    public static class ViewUtil
    {
        public static void SetActive(GameObject obj, bool state)
        {
            if (obj.activeSelf != state)
            {
                obj.SetActive(state);
            }
        }

        /// <summary>
        /// 设置当前视图的拖拽配置信息
        /// </summary>
        /// <param name="configs">拖拽配置信息</param>
        public static void SetDraggable(GameObject viewObject, params DragInputConfig[] configs)
        {
            if (configs != null)
            {
                for (int i = 0; i < configs.Length; i++)
                {
                    DragInputConfig config = configs[i];
                    if (config == null) { continue; }

                    GameObject moverObj = GameObjectFindUtil.BreadthFind(config.Mover, viewObject);
                    if (moverObj != null)
                    {
                        DragInput input = moverObj.AddComponent<DragInput>();
                        input.Draggable = config.Draggable;
                        input.RealtimeCalculateLimitArea = config.RealtimeCalculateLimitArea;
                        if (!string.IsNullOrEmpty(config.LimitArea))
                            input.LimitArea = GameObjectFindUtil.BreadthFind(config.LimitArea, viewObject)?.GetComponent<RectTransform>();
                    }
                }
            }
        }

        public static void BindModel(IView view, IBindableModel model, bool allowUIUpdateModel = true, string modelName = null, bool forceRebind = false)
        {
            //先查询之前是否生成过相同模型类型的UIBundle对象，防止重复生成
            UIBundle bundle = UIQuerier.Query(view.Name, model);

            //当前尚未为此模型生成任何UIBundle对象
            if (bundle == null)
            {
                ModelFilter filter = new ModelFilter(model, modelName);
                Vue.Rule.Filter(view.ViewObject, filter, view);
                bundle = filter.Bundle;
                Vue.Updater.BindViewModel(view.Name, bundle);
                model.UpdateAll(bundle);
            }
            else if (forceRebind || !bundle.active)
            {
                Vue.Updater.Rebind(view.Name, model);
            }
#if UNITY_EDITOR
            else
            {
                LogUtil.Warning($"名称为{view.Name}的视图已经绑定了模型{model.GetType().Name}!");
            }
#endif
        }

        public static void BindModel(GameObject viewObject, IBindableModel model, bool allowUIUpdateModel = true, string modelName = null, bool forceRebind = false)
        {
            //先查询之前是否生成过相同模型类型的UIBundle对象，防止重复生成
            UIBundle bundle = UIQuerier.Query(viewObject.name, model);

            //当前尚未为此模型生成任何UIBundle对象
            if (bundle == null)
            {
                ModelFilter filter = new ModelFilter(model, modelName);
                Vue.Rule.Filter(viewObject, filter);
                bundle = filter.Bundle;
                Vue.Updater.BindViewModel(viewObject.name, bundle);
                model.UpdateAll(bundle);
            }
            else if (forceRebind || !bundle.active)
            {
                Vue.Updater.Rebind(viewObject.name, model);
            }
#if UNITY_EDITOR
            else
            {
                LogUtil.Warning($"名称为{viewObject.name}的ViewObject已经绑定了模型{model.GetType().Name}!");
            }
#endif
        }


        /// <summary>
        /// 同时完成三个处理流程：构建UIEvent、绑定路由事件、绑定模型
        /// </summary>
        /// <remarks>这种方式无需创建视图对象(BaseView)</remarks>
        /// <param name="viewObject"></param>
        /// <param name="model">要绑定的模型</param>
        /// <param name="exclued">要排除的GameObject</param>
        public static void Patch3Pass(GameObject viewObject, IBindableModel model, params GameObject[] exclued)
        {
            Patch3Pass(viewObject, model, null, exclued);
        }


        /// <summary>
        /// 同时完成两个处理流程：构建UIEvent、绑定路由事件
        /// </summary>
        /// <param name="viewObject">GameObject</param>
        /// <param name="exclued">要排除的GameObject</param>
        public static void Patch2Pass(GameObject viewObject, params GameObject[] exclued)
        {
            Patch2Pass(viewObject, null, exclued);
        }

        /// <summary>
        /// 同时完成三个处理流程：构建UIEvent、绑定路由事件、绑定模型
        /// </summary>
        /// <remarks>这种方式无需创建视图对象(BaseView)</remarks>
        /// <param name="viewObject"></param>
        /// <param name="model">要绑定的模型</param>
        /// <param name="exclued">要排除的GameObject</param>
        public static void Patch3Pass(GameObject viewObject, IBindableModel model, IView view, params GameObject[] exclued)
        {
#if UNITY_EDITOR
            if (Vue.Updater.Table.ContainsViewName(viewObject.name))
            {
                LogUtil.Warning($"表中已经存在了一个相同名称{viewObject.name}的ViewObject,这可能将导致错误的结果,你应该确保viewName的唯一性");
            }
#endif

            ModelFilter modelFilter = new ModelFilter(model);
            EventFilter eventFilter = new EventFilter(view == null ? viewObject.name : view.Name);
            RouteFilter routeFilter = new RouteFilter(view == null ? viewObject.name : view.Name);

            Vue.Rule.Filter(viewObject, new IRuleFilter[3] { modelFilter, eventFilter, routeFilter }, view, exclued);

            UIBundle bundle = modelFilter.Bundle;
            Vue.Updater.BindViewModel(viewObject.name, bundle);
            model.UpdateAll(bundle);
        }


        /// <summary>
        /// 同时完成两个处理流程：构建UIEvent、绑定路由事件
        /// </summary>
        /// <param name="viewObject">GameObject</param>
        /// <param name="exclued">要排除的GameObject</param>
        public static void Patch2Pass(GameObject viewObject, IView view, params GameObject[] exclued)
        {
#if UNITY_EDITOR
            if (Vue.Updater.Table.ContainsViewName(viewObject.name))
            {
                LogUtil.Warning($"表中已经存在了一个相同名称{viewObject.name}的ViewObject,这可能将导致错误的结果,你应该确保viewName的唯一性");
            }
#endif
            EventFilter eventFilter = new EventFilter(view == null ? viewObject.name : view.Name);
            RouteFilter routeFilter = new RouteFilter(view == null ? viewObject.name : view.Name);

            Vue.Rule.Filter(viewObject, new IRuleFilter[2] { eventFilter, routeFilter }, view, exclued);
        }


        /// <summary>
        /// 同时完成三个处理流程：构建UIEvent、绑定路由事件、构建UIBundle
        /// </summary>
        /// <remarks>这种方式无需创建视图对象(BaseView)</remarks>
        /// <param name="viewObject"></param>
        /// <param name="model">用于创建UIBundle的模型</param>
        /// <param name="exclued">要排除的GameObject</param>
        /// <returns>UIBundle</returns> 
        public static UIBundle Patch3PassButNoBinding(GameObject viewObject, IBindableModel model, params GameObject[] exclued)
        {
#if UNITY_EDITOR
            if (Vue.Updater.Table.ContainsViewName(viewObject.name))
            {
                LogUtil.Warning($"表中已经存在了一个相同名称{viewObject.name}的ViewObject,这可能将导致错误的结果,你应该确保viewName的唯一性");
            }
#endif

            ModelFilter modelFilter = new ModelFilter(model);
            EventFilter eventFilter = new EventFilter(viewObject.name);
            RouteFilter routeFilter = new RouteFilter(viewObject.name);

            Vue.Rule.Filter(viewObject, new IRuleFilter[3] { modelFilter, eventFilter, routeFilter }, null, exclued);

            return modelFilter.Bundle;
        }

        /// <summary>
        /// 构建UI事件
        /// </summary>
        /// <param name="exclued">要排除的GameObject</param>
        public static void BuildUIEvents(GameObject viewObject, params GameObject[] exclued)
        {
            Vue.Rule.Filter(viewObject, new EventFilter(viewObject.name), null, exclued);
        }


        /// <summary>
        /// 构建路由事件
        /// </summary>
        /// <param name="exclued">要排除的GameObject</param>
        public static void BuildRoutEvents(GameObject viewObject, params GameObject[] exclued)
        {
            Vue.Rule.Filter(viewObject, new RouteFilter(viewObject.name), null, exclued);
        }
    }
}
