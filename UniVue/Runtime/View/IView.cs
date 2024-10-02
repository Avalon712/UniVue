
namespace UniVue.View
{
    public interface IView
    {
        /// <summary>
        /// 当前视图的状态
        /// </summary>
        public bool state { get; }

        /// <summary>
        /// 当前视图的级别
        /// </summary>
        public ViewLevel Level { get; }

        /// <summary>
        /// 当前视图的名称
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 视图卸载时调用
        /// </summary>
        public void OnUnload();

        /// <summary>
        /// 打开视图
        /// </summary>
        /// <remarks>你应该在这个函数的实现里面将state标记为true</remarks>
        public void Open();

        /// <summary>
        /// 关闭视图
        /// </summary>
        /// <remarks>你应该在这个函数的实现里面将state标记为false</remarks>
        public void Close();
    }
}
