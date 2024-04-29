
namespace UniVue.Evt.Evts
{
    public abstract class UIEvent
    {
        /// <summary>
        /// 当前UIEvent所处的视图
        /// </summary>
        public string ViewName { get; private set; }

        /// <summary>
        /// 事件名称
        /// </summary>
        public string EventName { get; private set; }

        /// <summary>
        /// 事件参数
        /// </summary>
        public EventArg[] EventArgs { get; internal set; }

        public UIEvent(string viewName,string eventName)
        {
            ViewName = viewName; EventName = eventName;
            //交给EventManger管理
            Vue.Event.AddUIEvent(this);
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        public virtual void Unregister()
        {
            for (int i = 0; i < EventArgs.Length; i++)
            {
                EventArgs[i] = default;
            }
            EventArgs = null;
        }

        /// <summary>
        /// 事件触发
        /// </summary>
        public void Trigger() { Vue.Event.ExecuteEvent(this); }

    }


}
