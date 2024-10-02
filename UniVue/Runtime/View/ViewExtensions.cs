using System.Collections.Generic;
using UnityEngine;
using UniVue.Common;
using UniVue.Model;
using UniVue.Rule;
using UniVue.ViewModel;

namespace UniVue.View
{
    public static class ViewExtensions
    {
        /// <summary>
        /// 绑定模型
        /// </summary>
        /// <param name="model">绑定的模型数据</param>
        /// <param name="allowUIUpdateModel">是否允许通过UI修改模型的值</param>
        /// <param name="modelName">模型名称，若为null则默认为该类型的TypeName</param>
        /// <param name="forceRebind">当已经绑定了数据时是否指示进行强制重新绑定</param>
        public static void BindModel(this IView view, IBindableModel model, bool allowUIUpdateModel = true, string modelName = null, bool forceRebind = false)
        {
            //先查询之前是否生成过相同模型类型的UIBundle对象，防止重复生成
            ModelUI modelUI = UIQuerier.QueryModelUI(view.Name, model);

            //当前尚未为此模型生成任何UIBundle对象
            if (modelUI == null)
            {
                ModelRule rule = new ModelRule(model, allowUIUpdateModel, modelName);
                Vue.Rule.Execute(view.GetViewObject(), rule);
                modelUI = rule.ModelUI;
                Vue.Updater.AddViewModel(view.Name, modelUI);
            }
            else if (forceRebind || !modelUI.active)
            {
                Vue.Updater.Rebind(view.Name, model, string.IsNullOrEmpty(modelName) ? model.TypeInfo.typeName : modelName);
            }
            else
            {
                ThrowUtil.ThrowWarn($"名称为{view.Name}的视图已经绑定了模型{model.TypeInfo.typeFullName}!");
            }
        }

        /// <summary>
        /// 重新绑定模型数据，注意新的模型类型应该与之前绑定过的模型类型一致
        /// </summary>
        /// <remarks>如果一个视图绑定了相同类型的多个模型数据，这两个模型将使用自定义的modelName加以区分，此
        /// 函数会重新绑定指定modelName的模型数据</remarks>
        /// <param name="newModel">新模型</param>
        /// <param name="modelName">指定的modelName，为null的话将默认为newModel的TypeName</param>
        public static void RebindModel(this IView view, IBindableModel newModel, string modelName = null)
        {
            if (modelName == null)
                modelName = newModel.TypeInfo.typeName;
            Vue.Updater.Rebind(view.Name, newModel, modelName);
        }

        /// <summary>
        /// 获取当前视图身上绑定的指定类型的模型
        /// </summary>
        /// <remarks>有多个相同类型的话，可以使用重载方法指定modelName获取</remarks>
        /// <typeparam name="T">实现IBindableModel接口</typeparam>
        /// <returns>当前视图身上绑定的指定类型的模型</returns>
        public static T GetModel<T>(this IView view) where T : IBindableModel
        {
            VMTable table = Vue.Updater.Table;
            if (table.TryGetModelUIs(view.Name, out List<ModelUI> modelUIs))
            {
                for (int i = 0; i < modelUIs.Count; i++)
                {
                    if (modelUIs[i].Model is T model)
                    {
                        return model;
                    }
                }
            }
            return default;
        }

        /// <summary>
        /// 获取当前视图身上绑定的指定类型指定ModelName的模型
        /// </summary>
        /// <typeparam name="T">实现IBindableModel接口</typeparam>
        /// <param name="modelName">模型名称</param>
        /// <returns>当前视图身上绑定的指定类型的模型</returns>
        public static T GetModel<T>(this IView view, string modelName) where T : IBindableModel
        {
            VMTable table = Vue.Updater.Table;
            if (table.TryGetModelUIs(view.Name, out List<ModelUI> modelUIs))
            {
                for (int i = 0; i < modelUIs.Count; i++)
                {
                    if (modelUIs[i].ModelName == modelName && modelUIs[i].Model is T model)
                    {
                        return model;
                    }
                }
            }
            return default;
        }


        /// <summary>
        /// 获取当前视图的视图对象ViewObject -> GameObject
        /// </summary>
        /// <returns>当前视图的视图对象 - GameObject</returns>
        public static GameObject GetViewObject(this IView view)
        {
            return Vue.Router.GetViewObject(view.Name);
        }


        /// <summary>
        /// 获取当前视图的父视图
        /// </summary>
        /// <returns>父视图，可能为null</returns>
        public static IView GetParent(this IView view)
        {
            return Vue.Router.GetParent(view.Name);
        }

    }
}
