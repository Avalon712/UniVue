using System;
using UnityEngine;
using UniVue.Evt;
using UniVue.Model;
using UniVue.Rule;
using UniVue.Utils;
using UniVue.View;
using UniVue.View.Config;
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

        /// <summary>
        /// Vue的全局渲染配置
        /// </summary>
        public static VueConfig Config => _config;

        /// <summary>
        /// 事件管理器
        /// </summary>
        public static EventManager Event => _event;

        /// <summary>
        /// 获取视图路由
        /// </summary>
        public static ViewRouter Router => _router;

        /// <summary>
        /// 获取视图更新器
        /// </summary>
        public static ViewUpdater Updater => _updater;

        /// <summary>
        /// 初始化Vue
        /// </summary>
        /// <param name="config">Vue的渲染配置文件</param>
        public static void Initialize(VueConfig config)
        {
            if (!_initialized)
            {
                _config = config;
                _event = new EventManager();
                _router = new ViewRouter();
                _updater = new ViewUpdater();
                _initialized = true;

#if UNITY_EDITOR
                UnityEditor.EditorApplication.playModeStateChanged += (mode) =>
                {
                    if (mode == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                    {
                        Dispose();
                    }
                };
#endif
            }
#if UNITY_EDITOR
            else
            {
                LogUtil.Warning("Vue已经初始化过了,只能进行一次初始化操作!");
            }
#endif
        }

        /// <summary>
        /// 加载视图
        /// <para>如果当前配置文件中viewObject尚未实例化则由框架进行实例化</para>
        /// <para>注：你应该只负责加载配置文件，实例化工作由框架进行，以确保视图的order顺序</para>
        /// </summary>
        /// <param name="config">当前场景下的视图配置文件</param>
        public static void LoadViews(SceneConfig config)
        {
            CheckInitialize();

            if(config == null)
            {
#if UNITY_EDITOR
                LogUtil.Warning("SceneConfig为null!");
#endif
                return;
            }
            //构建视图
            ScriptableViewBuilder.Build(config);
        }

        /// <summary>
        /// 卸载当前场景下的所有资源（UIBundle、View、UIEvent）
        /// </summary>
        public static void UnloadCurrentSceneResources()
        {
            CheckInitialize();

            _updater.ClearBundles();
            _router.UnloadAllViews();
            _event.UnregisterAllUIEvents();
        }

        /// <summary>
        /// 手动的方式创建AutowireInfo，以对不支持反射的时候实现自动装配与卸载的功能
        /// </summary>
        public static void BuildAutowireInfos(params Type[] types)
        {
            CheckInitialize();
            if (types != null) { _event.AddAutowireInfos(types); }
        }

        /// <summary>
        /// 自动装配EventCall
        /// 注意：这个函数只会被执行一次
        /// </summary>
        /// <param name="scanAssemblies">要扫描的程序集，如果为null则默认为"Assembly-CSharp"</param>
        public static void AutowireEventCalls(string[] scanAssemblies=null)
        {
            CheckInitialize();
            if (scanAssemblies == null) { scanAssemblies = new string[] { "Assembly-CSharp" }; }
            _event.ConfigAutowireEventCalls(scanAssemblies);
        }

        /// <summary>
        /// 自动装配指定场景的EventCall同时卸载不符合添加的EventCall
        /// </summary>
        /// <param name="currentSceneName">当前场景名称</param>
        public static void AutowireAndUnloadEventCalls(string currentSceneName)
        {
            CheckInitialize();
            _event.AutowireAndUnloadEventCalls(currentSceneName);
        }

        /// <summary>
        /// 卸载指定名称的视图
        /// </summary>
        /// <param name="viewName">要卸载的视图的名称</param>
        public static void UnloadView(string viewName)
        {
            CheckInitialize();
            _updater.UnloadBundle(viewName);
            _router.UnloadView(viewName);
            _event.UnregisterUIEvents(viewName);
        }

       
        /// <summary>
        /// 将模型绑定到一个UIBundle中，这个GameObject不是一个ViewObject的，但是他是一些UI的集合
        /// 这些UI可用与模型数据进行绑定
        /// </summary>
        /// <param name="uiBundle">一些UI的集合的根对象，这个GameObject不是一个ViewObject的</param>
        /// <param name="model">模型数据</param>
        /// <param name="allowUIUpdateModel">是否允许通过UI来修改模型数据</param>
        /// <param name="modelName">模型名称，若为null则将默认为模型TypeName</param>
        /// <returns>UIBundle</returns>
        public static UIBundle BuildUIBundle<T>(GameObject uiBundle,T model, bool allowUIUpdateModel = true, string modelName = null) where T : IBindableModel
        {
            CheckInitialize();

            var uis = ComponentFindUtil.FindAllSpecialUIComponents(uiBundle);
            UIBundle bundle = UIBundleBuilder.Build(uiBundle.name, uis, model, modelName, allowUIUpdateModel);
            if(bundle != null) { Updater.AddUIBundle(bundle); }
#if UNITY_EDITOR
            else
            {
                LogUtil.Warning("将模型数据与uiBundle的游戏对象进行绑定失败!可能的原因是绑定数据的UI的命名不合规范!");
            }
#endif
            uis.Clear();
            return bundle;
        }

        /// <summary>
        /// 生成UIEvent
        /// </summary>
        /// <param name="uiBundle">一些UI的集合的根对象，这个GameObject不是一个ViewObject的</param>
        public static void BuildUIEvents(GameObject uiBundle)
        {
            CheckInitialize();
            var uis = ComponentFindUtil.FindAllSpecialUIComponents(uiBundle);
            UIEventBuilder.Build(uiBundle.name, uis);
            uis.Clear();
        }

        /// <summary>
        /// 为指定的gameObject构建UIBundle和UIEvent
        /// </summary>
        /// <param name="uiBundle">一些UI的集合的根对象，这个GameObject不是一个ViewObject的</param>
        /// <param name="model">模型数据</param>
        /// <param name="allowUIUpdateModel">是否允许通过UI来修改模型数据</param>
        /// <param name="modelName">模型名称，若为null则将默认为模型TypeName</param>
        /// <returns>UIBundle</returns>
        public static UIBundle BuildUIBundleAndUIEvents<T>(GameObject uiBundle, T model, bool allowUIUpdateModel = true, string modelName = null) where T : IBindableModel
        {
            CheckInitialize();
            var uis = ComponentFindUtil.FindAllSpecialUIComponents(uiBundle);

            //构建UIEvent
            UIEventBuilder.Build(uiBundle.name, uis); 
            //构建UIBundle
            UIBundle bundle = UIBundleBuilder.Build(uiBundle.name, uis, model, modelName, allowUIUpdateModel);
            if (bundle != null) { Updater.AddUIBundle(bundle); }
#if UNITY_EDITOR
            else
            {
                LogUtil.Warning("将模型数据与uiBundle的游戏对象进行绑定失败!可能的原因是绑定数据的UI的命名不合规范!");
            }
#endif
            uis.Clear();
            return bundle;
        }

        /// <summary>
        /// 仅在Editor模式下执行
        /// </summary>
        private static void Dispose()
        {
            CheckInitialize();

            _router.UnloadAllViews();
            _updater.ClearBundles();
            _event.SignoutAll();
            _event.UnregisterAllUIEvents();
            _router = null;
            _updater = null;
            _event = null;
            _initialized = false;
        }

        private static void CheckInitialize()
        {
            if (!_initialized) { throw new Exception("Vue尚未进行初始化操作!"); }
        }
    }

    public sealed class VueConfig
    {
        private NamingFormat _format = NamingFormat.UI_Suffix | NamingFormat.UnderlineUpper;

        /// <summary>
        /// 命名风格
        /// </summary>
        /// <remarks>默认风格为 NamingFormat.UI_Suffix | NamingFormat.UnderlineUpper</remarks>
        public NamingFormat Format 
        {
            get => _format;
            set
            {
                if ((value & NamingFormat.UI_Prefix) != NamingFormat.UI_Prefix && (value & NamingFormat.UI_Suffix) != NamingFormat.UI_Suffix)
                {
                    throw new ArgumentException("命名格式必须指定UI名称的格式，指定NamedFormat.UI_Prefix或NamedFormat.UI_Suffix");
                }
                _format = value;
            }
        }

        /// <summary>
        /// 当更新Sprite、string类型时，如果Sprite的值为null类型则隐藏图片的显示，即: gameObject.SetActive(false);
        /// </summary>
        /// <remarks>默认为true</remarks>
        public bool WhenValueIsNullThenHide { get; set; } = true;

        /// <summary>
        /// 当更新List类型的数据时，如果当前数据量小于UI数量，则将多余的UI进行隐藏不显示
        /// </summary>
        /// <remarks>默认为true</remarks>
        public bool WhenListLessThanUIThenHide { get; set; } = true;

        /// <summary>
        /// 设置视图允许的最大历史记录，默认为10
        /// </summary>
        /// <remarks>如果你发现你的视图打开逻辑不正确,建议将此值设置大一点</remarks>
        public int MaxHistoryRecord { get; set; } = 10;
    }
}
