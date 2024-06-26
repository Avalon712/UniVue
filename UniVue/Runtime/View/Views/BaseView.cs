using System.Collections.Generic;
using UnityEngine;
using UniVue.Evt;
using UniVue.Input;
using UniVue.Model;
using UniVue.Tween;
using UniVue.Utils;
using UniVue.View.Widgets;

namespace UniVue.View.Views
{
    public class BaseView : IView
    {
        private bool _isClosing;
        private bool _isOpening;

        public int order { get; set; }

        public ViewLevel level { get; private set; }

        public string name { get; private set; }

        public bool state { get; protected set; }

        public GameObject viewObject { get; private set; }

        public bool isMaster { get; set; }

        public string root { get; set; }

        public string master { get; set; }

        public bool forbid { get; set; }

        public IView[] nestedViews { get; internal set; }

        /// <summary>
        /// ViewLevel.Transient视图显示时间
        /// </summary>
        public float transientTime { get; set; } = -1;

        /// <summary>
        /// 打开当前视图的默认缓动动画
        /// </summary>
        public DefaultTween openTween { get; set; } = DefaultTween.None;

        /// <summary>
        /// 打开动画的持续时间，小于0表示使用默认的缓动时间
        /// </summary>
        public float openDuration { get; set; } = -1;

        /// <summary>
        /// 关闭当前视图的默认缓动动画
        /// </summary>
        public DefaultTween closeTween { get; set; } = DefaultTween.None;

        /// <summary>
        /// 关闭动画的持续时间，小于0表示使用默认的缓动时间
        /// </summary>
        public float closeDuration { get; set; } = -1;


        public BaseView(GameObject viewObject, string viewName = null, ViewLevel level = ViewLevel.Common)
        {
            this.viewObject = viewObject;
            if (viewName == null) { viewName = viewObject.name; }
            name = viewName;
            this.level = level;
            state = viewObject.activeSelf || level == ViewLevel.Permanent;
            ViewUtil.SetActive(viewObject, state);

            //将当前视图对象交给ViewRouter管理
            Vue.Router.AddView(this);
        }

        /// <summary>
        /// 当视图被加载到场景中时调用
        /// </summary>
        public virtual void OnLoad()
        {
            BindEvent(null);
        }

        protected void BindEvent(params GameObject[] exclued)
        {
            //获取所有的ui组件
            var uis = ComponentFindUtil.FindAllSpecialUIComponents(viewObject, this, exclued);
            //构建UIEvent
            UIEventBuilder.Build(name, uis);
            //处理路由事件
            Vue.Router.BindRouteEvt(name, uis);
        }

        public IView BindModel<T>(T model, bool allowUIUpdateModel = true, string modelName = null, bool forceRebind = false) where T : IBindableModel
        {
            ViewUtil.BindModel(this, model, allowUIUpdateModel, modelName, forceRebind);
            return this;
        }

        public void RebindModel<T>(T newModel, T oldModel) where T : IBindableModel
        {
            Vue.Updater.Rebind(name, newModel, oldModel);
        }

        public void RebindModel<T>(T newModel) where T : IBindableModel
        {
            Vue.Updater.Rebind(name, newModel);
        }

        public IEnumerable<IView> GetNestedViews()
        {
            IView[] views = nestedViews;
            if (views != null)
            {
                for (int i = 0; i < views.Length; i++)
                {
                    if (views[i] != null) { yield return views[i]; }
                }
            }
        }

        /// <summary>
        /// 设置当前视图的拖拽配置信息
        /// </summary>
        /// <param name="configs">拖拽配置信息</param>
        public void SetDraggable(params DragInputConfig[] configs)
        {
            ViewUtil.SetDraggable(viewObject, configs);
        }

        public virtual void OnUnload()
        {
            viewObject = null;
        }


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

            if (_isOpening || _isClosing) { return; }

            state = true; //设置为打开状态

            if (openTween == DefaultTween.None)
            {
                viewObject.SetActive(true);
            }
            else
            {
                _isOpening = true;
                DefaultViewTweens.ExecuteTween(viewObject.transform, openTween, openDuration)
                    .Call(() => { _isOpening = false; }); //动画完成后才设置为开状态
            }

            if (level == ViewLevel.Transient)
            {
                TweenBehavior.Timer(() => Vue.Router.Close(name)).Delay(transientTime);
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

            if (_isClosing || _isOpening) { return; }

            state = false;

            if (closeTween == DefaultTween.None)
            {
                viewObject.SetActive(false);
            }
            else
            {
                _isClosing = true;
                DefaultViewTweens.ExecuteTween(viewObject.transform, closeTween, closeDuration)
                    .Call(() => { _isClosing = false; }); //动画完成后才设置为关状态
            }
        }

        public virtual T GetWidget<T>() where T : Widget
        {
            return default;
        }
    }
}
