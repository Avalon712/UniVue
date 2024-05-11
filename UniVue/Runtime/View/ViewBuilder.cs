using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Utils;
using UniVue.View.Config;
using UniVue.View.Views;

namespace UniVue.View
{
    /// <summary>
    /// 构建视图（注意配置文件中引用的canvas、viewObject必须是实例化了的）
    /// </summary>
    public sealed class ViewBuilder
    {
        internal ViewBuilder() { }

        public static void Build(SceneConfig config)
        {
            Build(config.canvasConfigs);
        }

        /// <summary>
        /// 构建视图
        /// </summary>
        private static void Build(List<CanvasConfig> canvasConfigs)
        {
            for (int i = 0; i < canvasConfigs.Count; i++)
            {
                Build(canvasConfigs[i]);
            }
        }

        private static void Build(CanvasConfig canvasConfig)
        {
            GameObject canvas = GameObject.Find(canvasConfig.canvasName);
            
            if (canvas == null)
            {
                throw new ArgumentException($"请检查你的Canvas配置的canvasName是否正确,没有在当前场景中找到名称为{canvasConfig.canvasName}的Canvas对象!采用的是Unity自动的GameObject.Find(string)方法进行的Canvas查找。");
            }

            Build(canvas, canvasConfig.views);
        }

        private static void Build(GameObject canvas,List<BaseView> views)
        {

            //先加载所有没有父视图的视图
            for (int j = 0; j < views.Count; j++)
            {
                views[j].viewObject = PrefabCloneUtil.RectTransformClone(views[j].viewObjectPrefab, canvas.transform);
                //加载子视图
                if (views[j].nestedViews != null)
                {
                    BaseView[] nestedViews = views[j].nestedViews;
                    for (int k = 0; k < nestedViews.Length; k++)
                    {
                        //注：这儿找嵌套视图时是通过viewName来进行查找的，因此要保证嵌套视图的名称与视图对象（ViewObject）的名称保证一致
                        GameObject nestedViewObject = GameObjectFindUtil.BreadthFind(nestedViews[k].name, views[j].viewObject);
                        if(nestedViewObject == null)
                        {
#if UNITY_EDITOR
                            LogUtil.Warning($"没有在根视图{views[j].name}下找到嵌套视图{nestedViews[k].name}!");
#endif
                        }
                        else
                        {
                            nestedViews[k].viewObject = nestedViewObject;
                        }
                    }
                }
            }


            //按order进行排序
            views.Sort((v1, v2) => v1.order - v2.order); //升序
            ViewRouter router = Vue.Router;

            for (int i = 0; i < views.Count; i++)
            {
                //调用OnLoad函数
                views[i].OnLoad();

                //调用嵌套视图的OnLoad函数
                if (views[i].nestedViews != null)
                {
                    BaseView[] nestedViews = views[i].nestedViews;
                    for (int k = 0; k < nestedViews.Length; k++)
                    {
                        //只有嵌套视图的ViewObject不为null才能调用其OnLoad()函数
                        if (nestedViews[k].viewObject != null)
                        {
                            nestedViews[k].OnLoad();
                        }
                    }
                }

                //设置视图排序顺序
                router.GetView(views[i].name).viewObject.transform.SetAsFirstSibling();
            }
        }

    }

}
