using System.Collections.Generic;

namespace UniVue.Event
{
    public interface IEventRegister
    {
        /// <summary>
        /// 注册事件
        /// </summary>
        /// <remarks>此函数由EventManager负责调用</remarks>
        /// <param name="calls">所有注册的事件调用</param>
        void OnRegisterEventCall(List<EventCall> calls) { }

        /// <summary>
        /// 执行指定事件的回调函数
        /// </summary>
        /// <remarks>此函数由EventManager负责调用</remarks>
        /// <param name="call">方法调用信息</param>
        /// <returns>方法返回结果</returns>
        object Invoke(EventCall call) { return null; }
    }
}
