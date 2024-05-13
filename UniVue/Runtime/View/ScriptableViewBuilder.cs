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
    public sealed class ScriptableViewBuilder
    {
        private ScriptableViewBuilder() { }

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

        private static void Build(CanvasConfig canvasConfig)
        {
            GameObject canvas = GameObject.Find(canvasConfig.canvasName);
            
            if (canvas == null)
            {
                throw new ArgumentException($"请检查你的Canvas配置的canvasName是否正确,没有在当前场景中找到名称为{canvasConfig.canvasName}的Canvas对象!采用的是Unity自动的GameObject.Find(string)方法进行的Canvas查找。");
            }

            Build(canvas, canvasConfig.views);
        }

        public static void Build(GameObject canvas,BaseView[] views)
        {
            Queue<BaseView> roots = new Queue<BaseView>();

            //先加载所有没有根视图的视图
            for (int j = 0; j < views.Length; j++)
            {
                views[j].viewObject = PrefabCloneUtil.RectTransformClone(views[j].viewObjectPrefab, canvas.transform);
                if(views[j].nestedViews != null) { roots.Enqueue(views[j]); }
            }

            //加载所有嵌套视图
            while (roots.Count > 0)
            {
                BaseView root = roots.Dequeue();

                BaseView[] nestedViews = root.nestedViews;
                string[] strs = new string[nestedViews.Length];

                for (int k = 0; k < nestedViews.Length; k++)
                {
                    strs[k] = nestedViews[k].name;
                    if (nestedViews[k].nestedViews != null) { roots.Enqueue(nestedViews[k]); }
                }

                //注：这儿找嵌套视图时是通过viewName来进行查找的，因此要保证嵌套视图的名称与视图对象（ViewObject）的名称保证一致
                using (var it = GameObjectFindUtil.BreadthFind(root.viewObject,strs).GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        BaseView view = Array.Find(nestedViews, (v) => v.name == it.Current.name);
                        if (view != null) { view.viewObject = it.Current; } 
                    }
                }

                //在编辑器模式下检查是否正确找到了所有嵌套视图的ViewObject对象，如果没有进行提醒
#if UNITY_EDITOR
                for (int k = 0; k < nestedViews.Length; k++)
                {
                    if (nestedViews[k].viewObject == null)
                    {
                        LogUtil.Warning($"视图名称为{nestedViews[k].name}未能在它的根视图下面找到它的ViewObject对象,请确保嵌套视图的名称与视图对象（ViewObject）的名称保证一致");
                    }
                }
#endif
            }

            //按order对根视图进行排序
            Array.Sort(views, (v1, v2) => v1.order - v2.order); 

            //先调用根视图的OnLoad()函数
            for (int i = 0; i < views.Length; i++)
            {
                //调用OnLoad函数
                views[i].OnLoad();

                if (views[i].nestedViews != null) { roots.Enqueue(views[i]); }

                //设置视图排序顺序
                views[i].viewObject.transform.SetAsFirstSibling();
            }

            //按层级深度依次调用所有嵌套视图的OnLoad()函数
            while (roots.Count > 0)
            {
                BaseView root = roots.Dequeue();
                BaseView[] nestedViews = root.nestedViews;
                for (int i = 0; i < nestedViews.Length; i++)
                {
                    nestedViews[i].OnLoad();
                    if (nestedViews[i].nestedViews != null) { roots.Enqueue(nestedViews[i]); }
                }
            }


        }

    }

}
