using UnityEngine;
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

        public ViewLevel Level => _level;

        public bool State => _state;

        public string Name => gameObject.name;

        public GameObject ViewObject => gameObject;

        public string Parent { get; set; }

        private void Start()
        {
            //初始化运行时数据
            _state = gameObject.activeSelf;
            ViewUtil.SetActive(ViewObject, _state || _level == ViewLevel.Permanent);
            //将当前视图对象交给ViewRouter管理
            Vue.Router.AddView(this);
        }

        public virtual void OnLoad()
        {
            ViewUtil.Patch2Pass(ViewObject);
        }

        public virtual void OnUnload()
        {

        }

        public IView BindModel(IBindableModel model, bool allowUIUpdateModel = true, string modelName = null, bool forceRebind = false)
        {
            ViewUtil.BindModel(this, model, allowUIUpdateModel, modelName, forceRebind);
            return this;
        }


        public void RebindModel(IBindableModel newModel, IBindableModel oldModel)
        {
            Vue.Updater.Rebind(Name, newModel, oldModel);
        }


        public void RebindModel(IBindableModel newModel)
        {
            Vue.Updater.Rebind(Name, newModel);
        }

        public virtual void Close()
        {
            if (!Vue.Router.IsRouterCtrl)
            {
                Vue.Router.Close(Name);
                return;
            }

            _state = false; //设置为关闭状态
            ViewObject.SetActive(false);
        }


        public virtual void Open()
        {
            if (!Vue.Router.IsRouterCtrl)
            {
                Vue.Router.Open(Name);
                return;
            }

            _state = true; //设置为打开状态
            ViewObject.SetActive(true);
        }

        public virtual T GetWidget<T>() where T : Widget
        {
            return default;
        }
    }
}
