using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Utils;
using UniVue.View.Config;
using UniVue.View.Views;

namespace UniVue.View
{
    /// <summary>
    /// 视图构建器
    /// </summary>
    public static class ViewBuilder
    {
        public static void Build(SceneConfig config)
        {
            Build(config.canvasConfigs);
        }

        /// <summary>
        /// 构建视图
        /// </summary>
        private static void Build(CanvasConfig[] canvasConfigs)
        {
            for (int i = 0; i < canvasConfigs.Length; i++)
            {
                Build(canvasConfigs[i]);
            }
        }

        public static void Build(CanvasConfig canvasConfig)
        {
            GameObject canvas = GameObject.Find(canvasConfig.canvasName);

            if (canvas == null)
            {
                throw new ArgumentException($"请检查你的Canvas配置的canvasName是否正确,没有在当前场景中找到名称为{canvasConfig.canvasName}的Canvas对象!采用的是Unity自动的GameObject.Find(string)方法进行的Canvas查找。");
            }

            Build(canvas, canvasConfig.views);
        }

        public static void Build(GameObject canvas, ViewConfig[] viewConfigs)
        {
            Transform parent = canvas.transform;
            //Item1:视图配置 Item2:视图对象 Item3:嵌套层级
            List<ValueTuple<ViewConfig, GameObject, int>> roots = new List<ValueTuple<ViewConfig, GameObject, int>>();

            //1.加载出所有的ViewObject
            //1.1 先加载根视图
            for (int i = 0; i < viewConfigs.Length; i++)
            {
                ViewConfig config = viewConfigs[i];
                GameObject viewObject = PrefabCloneUtil.RectTransformClone(config.viewObjectPrefab, parent);
                roots.Add((config, viewObject, 0));
            }
            //1.2 加载所有的嵌套视图
            FindNestedViewObject(roots, 0);

            //2.按嵌套层级从深往浅进行加载视图
            roots.Sort((r1, r2) => r2.Item3 - r1.Item3); //降序排序
            for (int i = 0; i < roots.Count; i++)
            {
                IView view = roots[i].Item1.CreateView(roots[i].Item2);
                view.OnLoad();
            }

            //3. 按order层级进行排序 : 只对根视图执行排序操作
            roots.Sort((r1, r2) => r1.Item1.order - r2.Item1.order); //升序排序，因为order值越大越先被渲染
            for (int i = 0; i < roots.Count; i++)
            {
                if (roots[i].Item3 == 0)
                {
                    roots[i].Item2.transform.SetAsLastSibling();
                }
            }
        }


        /// <summary>
        /// 加载指定层级的嵌套视图的ViewObject
        /// </summary>
        private static void FindNestedViewObject(List<ValueTuple<ViewConfig, GameObject, int>> roots, int level)
        {
            int before = roots.Count;
            for (int i = 0; i < roots.Count; i++)
            {
                if (roots[i].Item3 == level)
                {
                    ViewConfig config = roots[i].Item1;
                    ViewConfig[] nestedViews = config.nestedViews;
                    if (nestedViews != null)
                    {
                        for (int j = 0; j < nestedViews.Length; j++)
                        {
                            GameObject viewObject = GameObjectFindUtil.BreadthFind(nestedViews[j].viewName, roots[i].Item2);
#if UNITY_EDITOR
                            if (viewObject == null)
                                LogUtil.Warning($"未能在{roots[i].Item1.viewName}的ViewObject下找到名为{nestedViews[j].viewName}的嵌套视图的ViewObject");
#endif
                            roots.Add((nestedViews[j], viewObject, level + 1));
                        }
                    }
                }
            }

            if (before < roots.Count)
                FindNestedViewObject(roots, level + 1);
        }

    }

}
