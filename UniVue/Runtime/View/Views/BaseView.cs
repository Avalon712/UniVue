using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Evt;
using UniVue.Input;
using UniVue.Model;
using UniVue.Tween;
using UniVue.Utils;

namespace UniVue.View.Views
{
    /// <summary>
    /// BaseView
    /// </summary>
    public class BaseView : ScriptableObject, IView
    {
        private struct RuntimeData
        {
            /// <summary>
            /// 实例化ViewObject预制体的视图对象
            /// </summary>
            public GameObject viewObject;
            /// <summary>
            /// 当前视图状态
            /// </summary>
            public bool state;
            public bool isOpening;
            public bool isClosing;
        }

        private RuntimeData _runtime;

        #region 配置信息
        /// <summary>
        /// 当前视图在Canvas下的排序,序号越小越先被渲染
        /// 注：此值只有当前视图不是嵌套视图时才生效
        /// </summary>
        [Tooltip("当前视图在Canvas下的排序,序号越小越先被渲染")]
        [Range(0,20)]
        [SerializeField] internal int order = 0;

        /// <summary>
        /// 视图名称
        /// </summary>
        [SerializeField] private string viewName;

        /// <summary>
        /// 视图等级
        /// </summary>
        [SerializeField] private ViewLevel viewLevel = ViewLevel.Common;

        /// <summary>
        /// 视图游戏对象
        /// </summary>
        [Tooltip("视图对象预制体")]
        [SerializeField] internal GameObject viewObjectPrefab;

        /// <summary>
        /// ViewLevel.Transient视图显示时间
        /// </summary>
        [Tooltip("ViewLevel.Transient视图显示时间")]
        [SerializeField] private float transientTime = -1;

        /// <summary>
        /// 视图的初始状态 true=打开
        /// </summary>
        [Tooltip("设置当前视图的初始状态,true:打开状态")]
        [SerializeField] private bool initState;

        /// <summary>
        /// 设置当前ViewPanel被打开时是否禁止再打开其它ViewPanel
        /// </summary>
        [Tooltip("设置当前ViewPanel被打开时是否禁止再打开其它视图")]
        [SerializeField] private bool forbidOpenOther;

        /// <summary>
        /// 当前视图是否可以拖动
        /// </summary>
        [Tooltip("视图是否可以拖动")]
        [SerializeField] private bool draggable;

        /// <summary>
        /// 注意接受输入信号区域
        /// </summary>
        [Tooltip("可以接受拖拽信息的GameObject名称")]
        [SerializeField] private string receiveDragInput;

        /// <summary>
        /// 打开当前视图的默认缓动动画
        /// </summary>
        [Header("打开视图动画")]
        [SerializeField] private DefaultTween openTween = DefaultTween.None;

        /// <summary>
        /// 打开动画的持续时间，小于0表示使用默认的缓动时间
        /// </summary>
        [Range(-1, 10)]
        [Tooltip("仅在非DefaultTween.None时生效。打开动画的持续时间，小于0表示使用默认的缓动时间")]
        [SerializeField] private float openDuration = -1;

        /// <summary>
        /// 关闭当前视图的默认缓动动画
        /// </summary>
        [Header("关闭视图动画")]
        [SerializeField] private DefaultTween closeTween = DefaultTween.None;

        /// <summary>
        /// 关闭动画的持续时间，小于0表示使用默认的缓动时间
        /// </summary>
        [Range(-1, 10)]
        [Tooltip("仅在非DefaultTween.None时生效。关闭动画的持续时间，小于0表示使用默认的缓动时间")]
        [SerializeField] private float closeDuration = -1;

        /// <summary>
        /// 当前视图是否是属主视图
        /// </summary>
        [Header("当前视图是否是属主视图")]
        [SerializeField] private bool isMasterView;

        /// <summary>
        /// 为当前视图设置控制对象
        /// 只有当master视图处于打开状态则允许当前视图的打开、关闭
        /// 如果master视图被关闭则当前视图也会被关闭
        /// </summary>
        [Tooltip("为当前视图设置控制对象，只有当master视图处于打开状态则允许当前视图的打开、关闭；如果master视图被关闭则当前视图也会被关闭")]
        [Header("该视图的属主视图")]
        [SerializeField] private string masterViewName;

        /// <summary>
        /// 当前视图的根视图的viewName
        /// </summary>
        [Header("当前视图的根视图名称")]
        [SerializeField] private string rootViewName;

        /// <summary>
        /// 当前视图的嵌套的所有视图
        /// 注：无需为嵌套视图指定视图预制体viewObjectPrefab
        /// </summary>
        [Header("当前视图的嵌套的所有视图")]
        [SerializeField] internal BaseView[] nestedViews;

        #endregion

        #region 获取配置信息
        public GameObject viewObject { get => _runtime.viewObject;internal set => _runtime.viewObject = value; }

        public bool state { get => _runtime.state;protected set => _runtime.state = value; }

        public ViewLevel level { get => viewLevel; }

        public new string name { get => viewName; }

        public bool isMaster => isMasterView;

        public string root => rootViewName;

        public string master => masterViewName;

        public bool forbid => forbidOpenOther;

        #endregion

        /// <summary>
        /// 当视图被加载到场景中时调用
        /// </summary>
        public virtual void OnLoad()
        {
            if(viewLevel == ViewLevel.Transient)
            {
                transientTime = transientTime <= 0 ? 2 : transientTime;
            }
            //初始化运行时数据
            _runtime.state = initState;
            SetDraggable(draggable);
            viewObject.SetActive(initState || viewLevel == ViewLevel.Permanent);

            //将当前视图对象交给ViewRouter管理
            Vue.Router.AddView(this);

            AutoBindEvent();
        }

        protected virtual void AutoBindEvent()
        {
            //获取所有的ui组件
            var uis = ComponentFindUtil.FindAllSpecialUIComponents(viewObject, this);
            //构建UIEvent
            UIEventBuilder.Build(viewName, uis);
            //处理路由事件
            Vue.Router.BindRouteEvt(viewName, uis);
        }

        /// <summary>
        /// 当视图从场景中移除时调用
        /// </summary>
        public virtual void OnUnload()
        {
            _runtime = default;
        }

        #region 数据绑定

        
        public IView BindModel<T>(T model, bool allowUIUpdateModel = true, string modelName = null, bool forceRebind=false) where T : IBindableModel
        {
            if (!Vue.Updater.HadBinded(viewName, model))
            {
                //获取所有的ui组件
                var uis = ComponentFindUtil.FindAllSpecialUIComponents(viewObject, this);
                //模型到视图的绑定
                Vue.Updater.BindViewAndModel(viewName, model, uis, modelName, allowUIUpdateModel);
                model.NotifyAll();
            }
            else if (forceRebind)
            {
                Vue.Updater.Rebind(name, model);
            }
#if UNITY_EDITOR
            else
            {
                LogUtil.Warning($"名称为{viewName}的视图已经绑定了模型{model.GetType().Name}[hashCode={model.GetHashCode()}]!");
            }
#endif
            return this;
        }

        public void RebindModel<T>(T newModel,T oldModel) where T : IBindableModel
        {
            Vue.Updater.Rebind(viewName, newModel,oldModel);
        }

        public void RebindModel<T>(T newModel) where T : IBindableModel
        {
            Vue.Updater.Rebind(viewName, newModel);
        }

        public IEnumerable<IView> GetNestedViews()
        {
            if (nestedViews != null)
            {
                for (int i = 0; i < nestedViews.Length; i++)
                {
                    //Unity序列化的原因会导致虽然有数据但全是null值
                    if (nestedViews[i] != null) { yield return nestedViews[i]; }
                }
            }
        }


        #endregion

        #region 打开\关闭视图

        /// <summary>
        /// 打开当前面板
        /// </summary>
        public virtual void Open()
        {
            if (!Vue.Router.IsRouterCtrl)
            {
#if UNITY_EDITOR
                LogUtil.Warning("所有视图的打开关闭必须通过ViewRouter对象来控制!");
#endif
                return;
            }

            if (_runtime.isOpening || _runtime.isClosing) { return; }

            state = true; //设置为打开状态

            if (openTween == DefaultTween.None)
            {
                viewObject.SetActive(true);
            }
            else
            {
                _runtime.isOpening = true;
                DefaultViewTweens.ExecuteTween(viewObject.transform, openTween, openDuration)
                    .Call(() => { _runtime.isOpening = false; }); //动画完成后才设置为开状态
            }

            if (viewLevel == ViewLevel.Transient)
            {
                TweenBehavior.Timer(Close).Delay(transientTime);
            }
        }

        /// <summary>
        /// 关闭当前面板
        /// </summary>
        public virtual void Close()
        {
            if (!Vue.Router.IsRouterCtrl)
            {
#if UNITY_EDITOR
                LogUtil.Warning("所有视图的打开关闭必须通过ViewRouter对象来控制!");
#endif
                return;
            }

            if (_runtime.isClosing || _runtime.isOpening) { return; }

            state = false;

            if (closeTween == DefaultTween.None)
            {
                viewObject.SetActive(false);
            }
            else
            {
                _runtime.isClosing = true;
                DefaultViewTweens.ExecuteTween(viewObject.transform, closeTween, closeDuration)
                    .Call(() => { _runtime.isClosing = false; }); //动画完成后才设置为关状态
            }
        }

        #endregion

        /// <summary>
        /// 设置当前ViewPanel是否可以进行拖拽，默认false
        /// </summary>
        private void SetDraggable(bool draggable)
        {
            if (!draggable || string.IsNullOrEmpty(receiveDragInput)) { return; }

            GameObject receiver = GameObjectFindUtil.BreadthFind(receiveDragInput, viewObject);
            if (receiver == null) { receiver = viewObject; }
            DragInput.ReceiveInput(receiver, viewObject);
        }
    }
}

