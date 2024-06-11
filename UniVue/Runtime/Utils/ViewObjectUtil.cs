using UnityEngine;
using UniVue.Input;
using UniVue.Model;
using UniVue.View.Views;

namespace UniVue.Utils
{
    public sealed class ViewObjectUtil
    {
        private ViewObjectUtil() { }

        public static void SetActive(GameObject obj,bool state)
        {
            if(obj.activeSelf != state)
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

        public static void BindModel<T>(IView view,T model, bool allowUIUpdateModel = true, string modelName = null, bool forceRebind = false) where T : IBindableModel
        {
            //如果没有绑定过相同类型的模型则需要进行生成相应的UIBundle
            if (!Vue.Updater.HadBindedTheSameModelType(view.name,model))
            {
                //获取所有的ui组件
                var uis = ComponentFindUtil.FindAllSpecialUIComponents(view.viewObject, view);
                //模型到视图的绑定
                Vue.Updater.BindViewAndModel(view.name, model, uis, modelName, allowUIUpdateModel);
                model.NotifyAll();
            }
            else if (forceRebind)
            {
                Vue.Updater.Rebind(view.name, model);
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
