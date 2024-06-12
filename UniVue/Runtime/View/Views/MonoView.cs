using System.Collections.Generic;
using UnityEngine;
using UniVue.Evt;
using UniVue.Model;
using UniVue.Utils;
using UniVue.View.Widgets;

namespace UniVue.View.Views
{
    /// <summary>
    /// 此视图继承了MonoBehaviour，通过这个视图和提供的视图组件可以灵活得构建任意类型的视图
    /// 以及实现一些更复杂的视图功能
    /// </summary>
    public class MonoView : MonoBehaviour, IView
    {
        [SerializeField] private ViewLevel _level;

        private bool _state;

        /// <summary>
        /// 对于继承自MonoView的视图此属性将是无效的
        /// </summary>
        public int order => 0;

        public ViewLevel level => _level;

        public bool state => _state;

        public virtual GameObject viewObject => gameObject;

        public virtual bool isMaster => false;

        public virtual string root => null;

        public virtual string master => null;

        public virtual bool forbid => false;

        public virtual new string name => gameObject.name;


        public virtual void OnLoad()
        {
            //初始化运行时数据
            _state = gameObject.activeSelf;
            ViewUtil.SetActive(viewObject, _state || _level == ViewLevel.Permanent);
            //将当前视图对象交给ViewRouter管理
            Vue.Router.AddView(this);
            AutoBindEvent();
        }

        /// <summary>
        /// 绑定路由事件、创建UIEvent和EventArg对象
        /// </summary>
        private void AutoBindEvent()
        {
            //获取所有的ui组件
            var uis = ComponentFindUtil.FindAllSpecialUIComponents(viewObject, this);
            //构建UIEvent
            UIEventBuilder.Build(name, uis);
            //处理路由事件
            Vue.Router.BindRouteEvt(name, uis);
        }

        public virtual void OnUnload()
        {

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

        /// <summary>
        /// 获取此时图嵌套的所有的视图
        /// </summary>
        /// <remarks>如果此视图含有嵌套视图，务必重写此函数，否则可能会导致路由事件重复绑定、重复构建UIEvent等问题</remarks>
        /// <typeparam name="T">实现IView</typeparam>
        /// <returns>此视图包含的所有嵌套视图</returns>
        public virtual IEnumerable<IView> GetNestedViews()
        {
            yield return null;
        }


        public virtual void Close()
        {
            if (!Vue.Router.IsRouterCtrl)
            {
#if UNITY_EDITOR
                LogUtil.Warning("所有视图的打开关闭必须通过ViewRouter对象来控制!");
#endif
                return;
            }

            _state = false; //设置为关闭状态
            viewObject.SetActive(false);
        }


        public virtual void Open()
        {
            if (!Vue.Router.IsRouterCtrl)
            {
#if UNITY_EDITOR
                LogUtil.Warning("所有视图的打开关闭必须通过ViewRouter对象来控制!");
#endif
                return;
            }

            _state = true; //设置为打开状态
            viewObject.SetActive(true);
        }

        public virtual T GetWidget<T>() where T : Widget
        {
            return default;
        }
    }
}
