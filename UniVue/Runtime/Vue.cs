using System;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public sealed class Vue
    {
        private static Vue _instanced;
        private NamingFormat _format;
        private ViewRouter _router;
        private ViewUpdater _updater;
        private EventManager _event;

        private Vue()
        {
            _router = new();
            _updater = new();
            _event = new();
            _format = NamingFormat.UI_Suffix | NamingFormat.UnderlineUpper;

            Init();
        }

        private void Init()
        {
            //注册事件
            SceneManager.activeSceneChanged += OnSceneChanged;

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


        public static Vue Instance
        {
            get
            {
                if (_instanced == null) { _instanced = new Vue(); }
                return _instanced;
            }
        }

        /// <summary>
        /// 获取当前UI命名格式
        /// 默认值为： NamedFormat.UI_Suffix | NamedFormat.UnderlineUpper
        /// </summary>
        public static NamingFormat Format
        {
            get => Instance._format;
            set
            {
                if ((value & NamingFormat.UI_Prefix) != NamingFormat.UI_Prefix && (value & NamingFormat.UI_Suffix) != NamingFormat.UI_Suffix)
                {
                    throw new ArgumentException("命名格式必须指定UI名称的格式，指定NamedFormat.UI_Prefix或NamedFormat.UI_Suffix");
                }
                Instance._format = value;
            }
        }

        public static EventManager Event { get => Instance._event; }

        /// <summary>
        /// 获取视图路由
        /// </summary>
        public static ViewRouter Router { get => Instance._router; }

        /// <summary>
        /// 获取视图更新器
        /// </summary>
        public static ViewUpdater Updater { get => Instance._updater; }


        /// <summary>
        /// 加载视图
        /// <para>如果当前配置文件中viewObject尚未实例化则由框架进行实例化</para>
        /// <para>注：你应该只负责加载配置文件，实例化工作由框架进行，以确保视图的order顺序</para>
        /// </summary>
        /// <param name="config">当前场景下的视图配置文件</param>
        public void LoadViews(SceneConfig config)
        {
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
        /// 卸载当前场景下的所有视图
        /// 注：当场景切换时会自动调用此函数，你无需调用它。
        /// </summary>
        public void UnloadCurrentSceneResources()
        {
            _updater.ClearBundles();
            _router.UnloadViews();
            _event.UnregisterUIEvents();
        }

        /// <summary>
        /// 卸载指定名称的视图
        /// </summary>
        /// <param name="viewName">要卸载的视图的名称</param>
        public void UnloadView(string viewName)
        {
            _updater.UnloadBundle(viewName);
            _router.UnloadView(viewName);
            _event.UnregisterUIEvents(viewName);
        }

        /// <summary>
        /// 将数据绑定到视图上 (前提是已经将当前场景下的视图已经加载)
        /// <para>等价于 : Vue.Instance.Router.GetView().BindModel()</para>
        /// </summary>
        /// <param name="viewName">视图名称</param>
        /// <param name="model">模型数据</param>
        /// <param name="allowUIUpdateModel">是否允许通过UI来修改模型数据</param>
        /// <param name="modelName">模型名称，若为null则将默认为模型TypeName</param>
        public void BindModel<T>(string viewName, T model, bool allowUIUpdateModel = true, string modelName = null) where T : IBindableModel
        {
            Router.GetView(viewName)?.BindModel(model, allowUIUpdateModel, modelName);
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
        public UIBundle BuildUIBundle<T>(GameObject uiBundle, T model, bool allowUIUpdateModel = true, string modelName = null) where T : IBindableModel
        {
            var uis = ComponentFindUtil.FindAllSpecialUIComponents(uiBundle);
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
        /// 生成UIEvent
        /// </summary>
        /// <param name="uiBundle">一些UI的集合的根对象，这个GameObject不是一个ViewObject的</param>
        public void BuildUIEvents(GameObject uiBundle)
        {
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
        public UIBundle BuildUIBundleAndUIEvents<T>(GameObject uiBundle, T model, bool allowUIUpdateModel = true, string modelName = null) where T : IBindableModel
        {
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

        private void OnSceneChanged(Scene current, Scene next)
        {
            UnloadCurrentSceneResources();
        }

        public string GetCurrentSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        public void Dispose()
        {
            if (_instanced == null) { return; }

            SceneManager.activeSceneChanged -= OnSceneChanged;

            _router.UnloadViews();
            _updater.ClearBundles();
            _event.SignoutAll();
            _event.UnregisterUIEvents();

            _router = null;
            _updater = null;
            _instanced = null;
            _event = null;
        }

    }


}
 