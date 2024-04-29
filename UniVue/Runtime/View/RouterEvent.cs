
namespace UniVue.View
{
    /// <summary>
    /// 路由事件
    /// </summary>
    public enum RouterEvent
    {
        /// <summary>
        /// 打开视图事件
        /// </summary>
        Open,

        /// <summary>
        /// 关闭视图事件
        /// </summary>
        Close,

        /// <summary>
        /// 视图跳转事件
        /// </summary>
        Skip,

        /// <summary>
        /// 返回上一个打开的视图事件
        /// </summary>
        Return
    }
}
