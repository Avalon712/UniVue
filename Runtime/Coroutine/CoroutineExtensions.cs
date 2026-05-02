using System.Collections;

namespace UniVue.Coroutine
{
    public static class CoroutineExtensions
    {
        /// <summary>
        /// 作为一个协程进行运行
        /// </summary>
        /// <param name="coroutine">协程</param>
        /// <returns>协程唯一id</returns>
        public static CoroutineID RunAsCoroutine(this IEnumerator coroutine)
        {
            return CoroutineMgr.Run(coroutine);
        }

        /// <summary>
        /// 作为一个协程进行运行
        /// </summary>
        /// <param name="coroutine">协程</param>
        /// <param name="context">协程运行时对yield结果处理的上下文对象</param>
        /// <returns>协程唯一id</returns>
        public static CoroutineID RunAsCoroutine(this IEnumerator coroutine, CoroutineYieldHandleContext context)
        {
            return CoroutineMgr.Run(coroutine, context);
        }

        /// <summary>
        /// 作为一个协程进行运行
        /// </summary>
        /// <param name="coroutine">协程</param>
        /// <param name="coroutineName">协程别名（取别名方便调试）</param>
        /// <returns>协程唯一id</returns>
        public static CoroutineID RunAsCoroutine(this IEnumerator coroutine, string coroutineName)
        {
            return CoroutineMgr.Run(coroutine, coroutineName);
        }

        /// <summary>
        /// 作为一个协程进行运行
        /// </summary>
        /// <param name="coroutine">协程</param>
        /// <param name="coroutineName">协程别名（取别名方便调试）</param>
        /// <param name="context">协程运行时对yield结果处理的上下文对象</param>
        /// <returns>协程唯一id</returns>
        public static CoroutineID RunAsCoroutine(this IEnumerator coroutine, string coroutineName,
                                                 CoroutineYieldHandleContext context)
        {
            return CoroutineMgr.Run(coroutine, coroutineName, context);
        }
    }
}