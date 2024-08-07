﻿using System;
using UniVue.Evt;
using UniVue.i18n;
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
        private static RuleEngine _rule;

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
        /// 规则引擎
        /// </summary>
        public static RuleEngine Rule => _rule;

        /// <summary>
        /// 当前游戏中使用的语言标识
        /// </summary>
        public static string LanguageTag { get; private set; }

        /// <summary>
        /// 初始化Vue
        /// </summary>
        /// <param name="config">Vue的渲染配置文件</param>
        public static void Initialize(VueConfig config)
        {
            if (!_initialized)
            {
                _config = config;
                LanguageTag = config.DefaultLanguageTag;
                _rule = new RuleEngine();
                _event = new EventManager();
                _router = new ViewRouter();
                _updater = new ViewUpdater();
                _initialized = true;
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

        /// <summary>
        /// 加载视图
        /// <para>如果当前配置文件中viewObject尚未实例化则由框架进行实例化</para>
        /// <para>注：你应该只负责加载配置文件，实例化工作由框架进行，以确保视图的order顺序</para>
        /// </summary>
        /// <param name="config">当前场景下的视图配置文件</param>
        public static void LoadViews(SceneConfig config)
        {
            CheckInitialize();

            if (config == null)
            {
#if UNITY_EDITOR
                LogUtil.Warning("SceneConfig为null!");
#endif
                return;
            }
            //构建视图
            ViewBuilder.Build(config);
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
        /// 手动的方式创建AutowireInfo，以对不支持反射的时候（程序集扫描）实现自动装配与卸载的功能
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
        public static void AutowireEventCalls(string[] scanAssemblies = null)
        {
            CheckInitialize();
            if (scanAssemblies == null) { scanAssemblies = new string[] { "Assembly-CSharp" }; }
            _event.ConfigAutowireEventCalls(scanAssemblies);
        }

        /// <summary>
        /// 自动装配指定场景的EventCall同时卸载不符合条件的EventCall
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
        /// 切换语言
        /// </summary>
        /// <param name="language">游戏中要显示的语言</param>
        /// <param name="parser">I18n文件解析器，可使用内置的属性文件解析方法PropertyFileParser</param>
        /// <param name="loader">游戏中与多语言相关资产（精灵图片、字体）加载器（如果切换语言时不更换字体同时没有图片精灵的更换时传递null）</param>
        public static void SwitchLanguage(Language language, II18nResourceLoader loader)
        {
            CheckInitialize();
            LanguageTag = language.Tag;
            new I18n(language, loader).Switch(OnSwitchLanguageComplete);
        }

        private static void OnSwitchLanguageComplete()
        {
            Updater.UpdateEnumUI();
        }

        private static void CheckInitialize()
        {
            if (!_initialized) { throw new Exception("Vue尚未进行初始化操作!"); }
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

            _router.UnloadAllViews();
            _updater.ClearBundles();
            _event.SignoutAll();
            _event.UnregisterAllUIEvents();
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
