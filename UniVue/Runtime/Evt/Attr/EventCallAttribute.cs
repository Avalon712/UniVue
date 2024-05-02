using System;

namespace UniVue.Evt.Attr
{
    /// <summary>
    /// 该注解注解的方法不能是泛型方法,同时方法参数不能有in、out修饰。
    /// 同时方法参数的类型应该与EventArg的UI返回的数据类型一致。
    /// <para>方法参数支持的类型: int、float、string、bool、enum、自定义类型(不能是结构体)、EventCall、Sprite、UIEvent、EventArg[]</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method,AllowMultiple =false,Inherited =true)]
    public sealed class EventCallAttribute : Attribute
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        public string EventName { get; private set; }

        /// <summary>
        /// 只有在这些视图下的事件触发时才调用此函数
        /// 注：如果此函数为null，则表示无论在哪个视图，只要触发此事件都将进行回调
        /// </summary>
        public string[] Views { get; private set; }

        /// <summary>
        /// 注解此特性的方法中方法参数支持的类型: int、float、string、bool、enum、自定义类型(不能是结构体)、EventCall、Sprite、UIEvent、EventArg[]
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <param name="views">
        /// 只有在这些视图下的事件触发时才调用此函数
        /// 注：如果此函数为null，则表示无论在哪个视图，只要触发此事件都将进行回调
        /// </param>
        public EventCallAttribute(string eventName,params string[] views)
        {
            EventName = eventName; 
        }
    }

}
