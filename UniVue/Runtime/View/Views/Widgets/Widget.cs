
namespace UniVue.View.Widgets
{
    /// <summary>
    /// 组件标识
    /// </summary>
    public abstract class Widget
    {
        protected Widget() { }

        /// <summary>
        /// 销毁组件
        /// </summary>
        public abstract void Destroy();
    }
}
