﻿using System.Collections.Generic;
using UnityEngine;

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
        public EventArg[] EventArgs { get; private set; }

        public UIEvent(string viewName, string eventName, EventArg[] eventArgs)
        {
            ViewName = viewName;
            EventName = eventName;
            EventArgs = eventArgs;
            //交给EventManger管理
            Vue.Event.AddUIEvent(this);
        }

        /// <summary>
        /// 获取触发当前事件的UI组件
        /// </summary>
        /// <typeparam name="T">UI组件类型</typeparam>
        /// <returns>组件类型</returns>
        public abstract T GetEventUI<T>() where T : Component;

        /// <summary>
        /// 为当前事件设置事件参数
        /// </summary>
        /// <param name="values">key=参数名，value=参数值</param>
        public void SetEventArgs(Dictionary<string, object> values)
        {
            for (int i = 0; i < EventArgs.Length; i++)
            {
                if (values.ContainsKey(EventArgs[i].ArgumentName))
                {
                    EventArgs[i].SetArgumentValue(values[EventArgs[i].ArgumentName]);
                }
            }
        }

        /// <summary>
        /// 为当前事件指定参数设置参数值
        /// </summary>
        /// <param name="argName">参数名称</param>
        /// <param name="argValue">参数值</param>
        /// <returns>UIEvent</returns>
        public void SetEventArg(string argName, object argValue)
        {
            for (int i = 0; i < EventArgs.Length; i++)
            {
                if (argName == EventArgs[i].ArgumentName)
                {
                    EventArgs[i].SetArgumentValue(argValue);
                }
            }
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        public virtual void Unregister()
        {
            if (EventArgs != null)
            {
                for (int i = 0; i < EventArgs.Length; i++)
                {
                    EventArgs[i] = default;
                }
                EventArgs = null;
            }
        }

        /// <summary>
        /// 事件触发
        /// </summary>
        public void Trigger() { Vue.Event.ExecuteEvent(this); }

    }


}
