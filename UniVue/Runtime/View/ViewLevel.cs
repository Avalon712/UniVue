
namespace UniVue.View
{
    public enum ViewLevel
    {
        /// <summary>
        /// 瞬态，打开指定时间后将自动关闭
        /// </summary>
        Transient,

        /// <summary>
        /// 普通，可被任意打开、关闭
        /// </summary>
        Common,

        /// <summary>
        /// 系统级，只允许存在一个System级的视图被打开
        /// </summary>
        System,

        /// <summary>
        /// 持久级，不会被关闭的ViewPanel
        /// </summary>
        Permanent,

    }
}
