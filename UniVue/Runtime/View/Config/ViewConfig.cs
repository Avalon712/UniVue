using UnityEngine;
using UniVue.Input;
using UniVue.Tween;
using UniVue.Utils;
using UniVue.View.Views;

namespace UniVue.View.Config
{
    /// <summary>
    /// 视图配置
    /// </summary>
    public class ViewConfig : ScriptableObject
    {
        [Tooltip("当前视图在Canvas下的排序,序号越小越先被渲染")]
        [Range(0, 20)]
        public int order = 0;

        public string viewName;

        public ViewLevel level = ViewLevel.Common;

        [Tooltip("视图对象预制体")]
        public GameObject viewObjectPrefab;

        [Tooltip("ViewLevel.Transient视图显示时间")]
        public float transientTime = -1;

        [Tooltip("设置当前视图的初始状态,true:打开状态")]
        public bool initState;

        [Tooltip("设置当前View被打开时是否禁止再打开其它视图")]
        public bool forbid;

        [Tooltip("视图的拖拽配置信息")]
        public DragInputConfig[] _dragInputConfigs;

        [Header("打开视图动画")]
        public DefaultTween openTween = DefaultTween.None;

        [Range(-1, 10)]
        [Tooltip("仅在非DefaultTween.None时生效。打开动画的持续时间，小于0表示使用默认的缓动时间")]
        public float openDuration = -1;

        [Header("关闭视图动画")]
        public DefaultTween closeTween = DefaultTween.None;

        [Range(-1, 10)]
        [Tooltip("仅在非DefaultTween.None时生效。关闭动画的持续时间，小于0表示使用默认的缓动时间")]
        public float closeDuration = -1;

        [Header("当前视图是否是属主视图")]
        public bool isMaster;

        [Tooltip("为当前视图设置控制对象，只有当master视图处于打开状态则允许当前视图的打开、关闭；如果master视图被关闭则当前视图也会被关闭")]
        [Header("该视图的属主视图")]
        public string master;

        [Header("当前视图的根视图名称")]
        public string root;

        [Header("当前视图的嵌套的所有视图")]
        public ViewConfig[] nestedViews;


        internal virtual IView CreateView(GameObject viewObject)
        {
            ViewUtil.SetActive(viewObject, initState);

            var view = new BaseView(viewObject, viewName, level);
            BaseSettings(view);

            return view;
        }

        protected void BaseSettings<T>(T view) where T : BaseView
        {
            //视图序号设置
            view.order = order;
            //视图打开关闭模块
            view.isMaster = isMaster;
            view.root = root;
            view.master = master;
            view.forbid = forbid;
            view.transientTime = transientTime;
            //动画模块设置
            view.openTween = openTween;
            view.openDuration = openDuration;
            view.closeTween = closeTween;
            view.closeDuration = closeDuration;
            //拖拽设置
            view.SetDraggable(_dragInputConfigs);
        }
    }
}
