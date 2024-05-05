using System.Collections.Generic;
using UnityEngine;
using UniVue.Model;
using UniVue.Tween;
using UniVue.Utils;

namespace UniVue.View.Views
{
    public class DynamicView : IView
    {
        public ViewLevel level { get;private set; }

        public string name { get;private set; }

        public bool state { get;protected set; }

        public GameObject viewObject { get;private set; }

        public bool isMaster { get; set; }

        public string root { get; set; }

        public string master { get; set; }

        public bool forbid { get; set; }

        public IView[] nestedViews { get; set; }

        public DynamicView(GameObject viewObject,string viewName=null,ViewLevel level = ViewLevel.Common)
        {
            this.viewObject = viewObject; 
            if(viewName == null) { viewName = viewObject.name; }
            name = viewName;
            this.level = level;

            //获取所有的ui组件
            var uis = ComponentFindUtil.FindAllSpecialUIComponents(viewObject, this);
            //将当前视图对象交给ViewRouter管理
            Vue.Router.AddView(this, uis);

            //调用函数
            OnLoad();
        }

        public IView BindModel<T>(T model, bool allowUIUpdateModel = true, string modelName = null) where T : IBindableModel
        {
            if (!Vue.Updater.HadBinded(name, model))
            {
                //获取所有的ui组件
                var uis = ComponentFindUtil.FindAllSpecialUIComponents(viewObject, this);
                //模型到视图的绑定
                Vue.Updater.BindViewAndModel(name, model, uis, modelName, allowUIUpdateModel);
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
                    yield return views[i];
                }
            }
            yield return null;
        }

        public virtual void OnLoad(){ }

        public virtual void OnUnload()
        {
            viewObject = null;
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

            viewObject.SetActive(false);

            state = false; //设置为关闭状态
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

            viewObject.SetActive(true);
            
            if (level == ViewLevel.Transient)
            {
                TweenBehavior.Timer(Close).Delay(5);
            }

            state = true; //设置为打开状态
        }
    }
}
