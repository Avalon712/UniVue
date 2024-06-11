
namespace UniVue.Effect
{
    public interface IUIEffect
    {
        /// <summary>
        /// 当前是否处于激活状态
        /// </summary>
        bool active { get; }

        /// <summary>
        /// 启用UI效果
        /// </summary>
        void Enable();

        /// <summary>
        /// 第一帧调用
        /// </summary>
        void OnStart();

        /// <summary>
        /// 每帧调用
        /// </summary>
        void OnUpdate(float deltaTime);

        /// <summary>
        /// Update()后
        /// </summary>
        void OnLateUpdate(float deltaTime);

        /// <summary>
        /// 禁用UI效果
        /// </summary>
        void Disable();

        /// <summary>
        /// 销毁
        /// </summary>
        void Destroy();
    }
}
