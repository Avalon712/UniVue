namespace UniVue.Tween
{
    /// <summary>
    /// 缓动函数 see: https://spicyyoghurt.com/tools/easing-functions
    /// </summary>
    public enum TweenEase
    {
        /// <summary>
        /// 线性
        /// </summary>
        Linear,

        /// <summary>
        /// 二次缓动
        /// </summary>
        InQuad,

        /// <summary>
        /// 二次缓出
        /// </summary>
        OutQuad,

        /// <summary>
        /// 二次缓入缓出
        /// </summary>
        InOutQuad,

        /// <summary>
        /// 正弦缓动
        /// </summary>
        InSin,

        /// <summary>
        /// 正弦缓出
        /// </summary>
        OutSin,

        /// <summary>
        /// 正弦缓入和缓出
        /// </summary>
        InOutSin,

        /// <summary>
        /// 指数缓动
        /// </summary>
        InExp,

        /// <summary>
        /// 指数缓出
        /// </summary>
        OutExp,

        /// <summary>
        /// 指数缓入和缓出
        /// </summary>
        InOutExp,

        /// <summary>
        /// 弹性缓动
        /// </summary>
        InElastic,

        /// <summary>
        /// 弹性缓出
        /// </summary>
        OutElastic,

        /// <summary>
        /// 弹性缓入和缓出
        /// </summary>
        InOutElastic,

        /// <summary>
        /// 反弹
        /// </summary>
        InBounce,

        OutBounce,

        InOutBounce,

        /// <summary>
        /// 循环缓入
        /// </summary>
        InCirc,

        OutCirc,

        InOutCirc,

        /// <summary>
        /// 震荡
        /// </summary>
        Vibrate,

        OnePunch,
    }
}
