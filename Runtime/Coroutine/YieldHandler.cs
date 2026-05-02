using System;

namespace UniVue.Coroutine
{
    public abstract class YieldHandler
    {
        /// <summary>
        /// 当前处理器处理的Yield类型
        /// </summary>
        public abstract Type YieldType { get; }

        internal bool HandleYieldInternal(CoroutineMgr.CoroutineRecorder recorder)
        {
            return HandleYield(recorder);
        }

        /// <summary>
        /// 处理协程的yield return的结果
        /// </summary>
        /// <remarks>只有对结果处理完成后才会继续执行协程</remarks>
        /// <param name="recorder">协程执行栈帧</param>
        /// <returns>处理是否已经完成 true-完成  false-处理中</returns>
        protected abstract bool HandleYield(CoroutineMgr.CoroutineRecorder recorder);
    }
}