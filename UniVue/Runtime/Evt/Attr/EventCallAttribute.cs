using System;

namespace UniVue.Evt.Attr
{
    /// <summary>
    /// 该注解注解的方法不能是泛型方法,同时方法参数不能有in、out修饰。
    /// 同时方法参数的类型应该与EventArg的UI返回的数据类型一致。
    /// <para>方法参数支持的类型: int、float、string、bool、enum、自定义类型(不能是结构体)、EventCall、UIEvent、EventArg[]</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method,AllowMultiple =false,Inherited =true)]
    public sealed class EventCallAttribute : Attribute
    {
        /// <summary>
        /// 事件名称
        /// </summary>
        public string EventName { get; private set; }


        public EventCallAttribute(string eventName)
        {
            EventName = eventName; 
        }
    }

}
