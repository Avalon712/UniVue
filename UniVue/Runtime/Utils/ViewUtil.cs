using UnityEngine;
using UniVue.Input;
using UniVue.Model;
using UniVue.View.Views;
using UniVue.ViewModel;

namespace UniVue.Utils
{
    /// <summary>
    /// 视图封装工具
    /// </summary>
    public sealed class ViewUtil
    {
        private ViewUtil() { }

        public static void SetActive(GameObject obj, bool state)
        {
            if (obj.activeSelf != state)
            {
                obj.SetActive(state);
            }
        }

        /// <summary>
        /// 设置当前视图的拖拽配置信息
        /// </summary>
        /// <param name="configs">拖拽配置信息</param>
        public static void SetDraggable(GameObject viewObject, params DragInputConfig[] configs)
        {
            if (configs != null)
            {
                for (int i = 0; i < configs.Length; i++)
                {
                    DragInputConfig config = configs[i];
                    if (config == null) { continue; }

                    GameObject moverObj = GameObjectFindUtil.BreadthFind(config.Mover, viewObject);
                    if (moverObj != null)
                    {
                        DragInput input = moverObj.AddComponent<DragInput>();
                        input.Draggable = config.Draggable;
                        input.RealtimeCalculateLimitArea = config.RealtimeCalculateLimitArea;
                        if (!string.IsNullOrEmpty(config.LimitArea))
                            input.LimitArea = GameObjectFindUtil.BreadthFind(config.LimitArea, viewObject)?.GetComponent<RectTransform>();
                    }
                }
            }
        }

        public static void BindModel<T>(IView view, T model, bool allowUIUpdateModel = true, string modelName = null, bool forceRebind = false) where T : IBindableModel
        {
            //先查询之前是否生成过相同模型类型的UIBundle对象
            UIBundle bundle = UIQuerier.Query(view.name, model);
            //当前尚未为此模型生成任何UIBundle对象
            if (bundle == null)
            {
                //获取所有的ui组件
                var uis = ComponentFindUtil.FindAllSpecialUIComponents(view.viewObject, view);
                //模型到视图的绑定
                Vue.Updater.BindViewAndModel(view.name, model, uis, modelName, allowUIUpdateModel);
                model.NotifyAll();
            }
            else if (!bundle.active || forceRebind)
            {
                bundle.Rebind(model);
            }
#if UNITY_EDITOR
            else
            {
                LogUtil.Warning($"名称为{view.name}的视图已经绑定了模型{model.GetType().Name}!");
            }
#endif
        }
    }
}
