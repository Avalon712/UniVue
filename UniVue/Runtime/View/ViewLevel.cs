
namespace UniVue.View
{
    public enum ViewLevel
    {
        /// <summary>
        /// 普通，可被任意打开、关闭
        /// </summary>
        Common,

        /// <summary>
        /// 瞬态，打开指定时间后将自动关闭
        /// </summary>
        Transient,

        /// <summary>
        /// 系统级，同级视图中只允许存在一个System级的视图被打开
        /// </summary>
        /// <remarks>同级指拥有相同父视图的视图(所有根视图属于同一级)</remarks>
        System,

        /// <summary>
        /// 模态级，不关闭此视图无法打开任何视图
        /// </summary>
        Modal,

        /// <summary>
        /// 持久级，不会被"关闭"的视图
        /// </summary>
        Permanent,

        /// <summary>
        /// 非托管的，此类视图的打开和关闭不会经过ViewRouter的检查，会被执行Open/Close
        /// </summary>
        Unmanaged,
    }
}
