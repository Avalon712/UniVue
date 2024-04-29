
namespace UniVue.Tween
{
    public interface ITweenTask
    {
        /// <summary>
        /// 执行缓动
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns>动画是否结束</returns>
        public bool Execute(float deltaTime);

        /// <summary>
        /// 重置当前缓动
        /// </summary>
        public void Reset();
    }
}
