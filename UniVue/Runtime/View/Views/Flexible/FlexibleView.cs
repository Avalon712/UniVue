using System.Collections.Generic;
using UnityEngine;
using UniVue.Evt;
using UniVue.Model;
using UniVue.Tween;
using UniVue.Utils;

namespace UniVue.View.Views.Flexible
{
    public class FlexibleView : IView
    {
        private bool _isClosing;
        private bool _isOpening;

        public ViewLevel level { get;private set; }

        public string name { get;private set; }

        public bool state { get;protected set; }

        public GameObject viewObject { get;private set; }

        public bool isMaster { get; set; }

        public string root { get; set; }

        public string master { get; set; }

        public bool forbid { get; set; }

        public IView[] nestedViews { get; set; }

        /// <summary>
        /// 当前视图在Canvas下的排序,序号越小越先被渲染
        /// 注：此值只有当前视图不是嵌套视图时才生效
        /// </summary>
        public int order { get; set; }

        /// <summary>
        /// ViewLevel.Transient视图显示时间
        /// </summary>
        public float transientTime { get; set; } = -1;

        /// <summary>
        /// 当前视图是否可以拖动
        /// </summary>
        public bool draggable { get; set; }

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


        public FlexibleView(GameObject viewObject,string viewName=null,ViewLevel level = ViewLevel.Common)
        {
            this.viewObject = viewObject; 
            if(viewName == null) { viewName = viewObject.name; }
            name = viewName;
            this.level = level;
            state = viewObject.activeSelf || level == ViewLevel.Permanent;
            viewObject.SetActive(state);

            //将当前视图对象交给ViewRouter管理
            Vue.Router.AddView(this);
            //获取所有的ui组件
            var uis = ComponentFindUtil.FindAllSpecialUIComponents(viewObject, this);
            //构建UIEvent
            UIEventBuilder.Build(viewName, uis);
            //处理路由事件
            Vue.Router.BindRouteEvt(viewName, uis);
            //调用函数
            OnLoad();
        }

        public IView BindModel<T>(T model, bool allowUIUpdateModel = true, string modelName = null, bool forceRebind = false) where T : IBindableModel
        {
            if (!Vue.Updater.HadBinded(name, model))
            {
                //获取所有的ui组件
                var uis = ComponentFindUtil.FindAllSpecialUIComponents(viewObject, this);
                //模型到视图的绑定
                Vue.Updater.BindViewAndModel(name, model, uis, modelName, allowUIUpdateModel);
                model.NotifyAll();
            }
            else if (forceRebind)
            {
                Vue.Updater.Rebind(name, model);
            }
#if UNITY_EDITOR
            else
            {
                LogUtil.Warning($"名称为{name}的视图已经绑定了模型{model.GetType().Name}[hashCode={model.GetHashCode()}]!");
            }
#endif
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

        public virtual void OnLoad(){ }

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

    }
}
