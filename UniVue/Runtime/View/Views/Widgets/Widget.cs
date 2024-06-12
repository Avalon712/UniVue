
namespace UniVue.View.Widgets
{
    /// <summary>
    /// 组件标识
    /// </summary>
    public abstract class Widget
    {
        /// <summary>
        /// Unity序列化接口
        /// </summary>
        /// <remarks>请不要使用此构造函数</remarks>
        public Widget() { }

        /// <summary>
        /// 销毁组件
        /// </summary>
        public abstract void Destroy();
    }
}
