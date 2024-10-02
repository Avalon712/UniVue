using System;

namespace UniVue.Event
{
    /// <summary>
    /// 该注解注解的方法不能是泛型、静态方法，同时方法参数不能有in、out、ref修饰。
    /// 同时方法参数的类型应该与<see cref="ArgumentUI"/>的UI返回的数据类型一致。
    /// <para>不指定EventName时将默认方法名为事件名称，不指定触发范围时(视图)将默认为满足条件即调用</para>
    /// <para>方法参数支持的类型:见<see cref="SupportableArgumentType"/>枚举的定义</para>
    /// <para>以下情况将不会进行事件的调用</para>
    /// <para>1、泛型方法</para>
    /// <para>2、方法参数带有<see langword="out"/>关键字修饰(支持<see langword="in"/>、<see langword="ref"/>关键字)</para>
    /// <para>3、方法参数带有<see langword="params"/>关键字</para>
    /// <para>4、含有不被支持的参数类型</para>
    /// <para>5、不支持await/async</para>
    /// <para>以下情况是被支持的</para>
    /// <para>1、支持Unity的协程调用(前提条件此所方法所在的类必须继承了MonoBehaviour)</para>
    /// </summary>
    /// <remarks>此特性只有在导入<see href="https://github.com/Avalon712/UniVue-SourceGenerator">UniVue源生成器</see>后才会生效</remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class EventCallAttribute : Attribute
    {
        public EventCallAttribute() { }

        public EventCallAttribute(string eventName) { }

        public EventCallAttribute(string[] views) { }

        public EventCallAttribute(string eventName, string[] views) { }
    }

}
