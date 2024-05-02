using System;

namespace UniVue.Evt.Attr
{
    /// <summary>
    /// EventCall的自动装配
    /// </summary>
    [AttributeUsage(AttributeTargets.Class,AllowMultiple=false,Inherited=false)]
    public sealed class EventCallAutowireAttribute : Attribute
    {
        /// <summary>
        /// 指示在哪些视图下需要装配
        /// 注：如果为null，则表示每个场景都需要自动装配此对象
        /// </summary>
        public string[] Scenes { get; private set; }

        /// <summary>
        /// 是否为单例对象。如果为单例对象，在注册时由EventMangager进行保证其单例性
        /// </summary>
        public bool singleton { get; private set; }

        /// <summary>
        /// EventCall的自动装配
        /// </summary>
        /// <param name="scenes">
        /// 指示在哪些视图下需要装配
        /// 注：如果为null，则表示每个场景都需要自动装配此对象
        /// </param>
        /// <param name="singleton">是否为单例对象。如果为单例对象，在注册时由EventMangager进行保证其单例性</param>
        public EventCallAutowireAttribute(bool singleton=true,params string[] scenes)
        {
            this.singleton = singleton; Scenes = scenes;
        }
    }
}
