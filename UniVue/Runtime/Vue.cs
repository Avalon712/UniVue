using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Common;
using UniVue.Event;
using UniVue.i18n;
using UniVue.Rule;
using UniVue.View;
using UniVue.ViewModel;


namespace UniVue
{
    /// <summary>
    /// 全局单例对象
    /// </summary>
    public static class Vue
    {
        private static bool _initialized;
        private static VueConfig _config;
        private static ViewRouter _router;
        private static ViewUpdater _updater;
        private static EventManager _event;
        private static RuleEngine _rule;
        private static Language _language;

        /// <summary>
        /// 当语言环境发生改变时回调此函数
        /// </summary>
        public static event Action OnLanguageEnvironmentChanged;

        /// <summary>
        /// Vue的全局配置
        /// </summary>
        public static VueConfig Config { get { CheckInitialize(); return _config; } }

        /// <summary>
        /// 事件管理器
        /// </summary>
        public static EventManager Event { get { CheckInitialize(); return _event; } }

        /// <summary>
        /// 视图路由器
        /// </summary>
        public static ViewRouter Router { get { CheckInitialize(); return _router; } }

        /// <summary>
        /// 视图更新器
        /// </summary>
        public static ViewUpdater Updater { get { CheckInitialize(); return _updater; } }

        /// <summary>
        /// 规则引擎
        /// </summary>
        public static RuleEngine Rule { get { CheckInitialize(); return _rule; } }

        /// <summary>
        /// 当前游戏中使用的语言标识
        /// </summary>
        public static Language language
        {
            get
            {
                CheckInitialize();
                return _language;
            }
            set
            {
                CheckInitialize();
                if (_language != value)
                {
                    _language = value;
                    OnLanguageEnvironmentChanged.Invoke();
                }
            }
        }

        /// <summary>
        /// 初始化Vue
        /// </summary>
        /// <param name="config">Vue的渲染配置文件</param>
        public static void Initialize(VueConfig config)
        {
            if (!_initialized)
            {
                _initialized = true;
                _config = config;
                _language = config.DefaultLanguage;
                _rule = new RuleEngine();
                _event = new EventManager();
                _router = new ViewRouter();
                _updater = new ViewUpdater();
                #region 编辑器模式
#if UNITY_EDITOR
                UnityEditor.EditorApplication.playModeStateChanged += (mode) =>
                {
                    if (mode == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                    {
                        Dispose();
                    }
                };
#endif
                #endregion
            }
        }

        private static void CheckInitialize()
        {
            if (!_initialized) { throw new Exception("Vue尚未进行初始化操作!"); }
        }

        /// <summary>
        /// 卸载当前场景下的所有资源
        /// </summary>
        /// <remarks>场景切换时调用此方法释放上一个场景的资源</remarks>
        public static void UnloadCurrentSceneResources()
        {
            CheckInitialize();
            _updater.UnloadResources();
            _router.UnloadResources();
            _event.UnloadResources();
        }

        /// <summary>
        /// 判断一个GameObject是否是一个ViewObject
        /// </summary>
        /// <param name="gameObject">要验证的GameObject</param>
        /// <returns>true:是一个ViewObject</returns>
        public static bool IsViewObject(GameObject gameObject)
        {
            if (gameObject == null) return false;
            CheckInitialize();

            if (_config.Mode == MatchViewMode.NameEndWith_View)
                return gameObject.name.EndsWith("View");
            else
                return gameObject.tag == "ViewObject";
        }

        /// <summary>
        /// 加载当前场景下所有根Canvas下所有的视图对象 
        /// <para>期望更快的速度，使用LoadAllViewObject(List&lt;Canvas&gt;)重载方法，即指定所有的根Canvas对象，避免全局搜索根Canvas对象</para>
        /// </summary>
        /// <remarks>
        /// <para>根Canvas对象：当前Canvas的直系先辈中不存在Canvas组件</para>
        /// <para>当前场景加载完毕时调用此函数加载视图对象</para>
        /// </remarks>
        public static void LoadAllViewObject()
        {
            CheckInitialize();
            //1.找到当前场景下的所有根Canvas组件
            //2.遍历每个Canvas组件，找到视图对象
            //3.重构视图树
            //4.构建路由事件和UI事件
            Dictionary<string, GameObject> viewObjects = Router.ViewObjects;
            Canvas[] canvas = UnityEngine.Object.FindObjectsOfType<Canvas>(true);
            List<Canvas> temp = new List<Canvas>(); //最后一个元素是根Canvas，其余全部是非根Canvas
            for (int i = 0; i < canvas.Length; i++)
            {
                if (canvas[i] == null) continue;
                ComponentFindUtil.LookUpFindAllComponent(canvas[i].gameObject, temp);
                for (int j = 0; j < temp.Count - 1; j++)
                {
                    int idx = Array.IndexOf(canvas, temp[j], i);
                    if (idx != -1)
                    {
                        canvas[idx] = null; //标记非根对象
                    }
                }
                Canvas root = temp[temp.Count - 1];
                int index = Array.IndexOf(canvas, root, i);
                if (index != -1)
                {
                    GameObjectUtil.DepthFindAllViewObjects(root.gameObject, viewObjects);
                    canvas[index] = null; //标记根对象已经处理过了
                }
                temp.Clear();
            }
            Router.RebuildVTree();
            Rule.ExecuteLoadViewRule(viewObjects.Values);
        }

        /// <summary>
        /// 加载指定的所有根Canvas下所有的视图对象
        /// </summary>
        /// <remarks>
        /// <para>根Canvas对象：当前Canvas的直系先辈中不存在Canvas组件</para>
        /// <para>当前场景加载完毕时调用此函数加载视图对象</para>
        /// </remarks>
        /// <param name="rootCanvas">所有根Canvas对象</param>
        public static void LoadAllViewObject(List<Canvas> rootCanvas)
        {
            CheckInitialize();
            Dictionary<string, GameObject> viewObjects = Router.ViewObjects;
            for (int i = 0; i < rootCanvas.Count; i++)
            {
                GameObjectUtil.DepthFindAllViewObjects(rootCanvas[i].gameObject, viewObjects);
            }
            Router.RebuildVTree();
            Rule.ExecuteLoadViewRule(viewObjects.Values);
        }

        #region 编辑器模式
#if UNITY_EDITOR
        /// <summary>
        /// 仅在Editor模式下执行
        /// </summary>
        /// <remarks>Help GC</remarks>
        private static void Dispose()
        {
            if (!_initialized) return;
            UnloadCurrentSceneResources();
            _rule = null;
            _router = null;
            _updater = null;
            _event = null;
            _initialized = false;
        }
#endif
        #endregion

    }
}
