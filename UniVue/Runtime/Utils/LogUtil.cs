using UnityEngine;

namespace UniVue.Utils
{
    public sealed class LogUtil
    {
        private LogUtil() { }

        public static void Info(string message)
        {
            Log(1, message);
        }

        public static void Warning(string message)
        {
            Log(2, message);
        }

        public static void Error(string message)
        {
            Log(3, message);
        }


        /// <summary>
        /// 内部实现各种信息输出
        /// </summary>
        /// <param name="level">输出级别：1-Info、2-Warning、3-Error</param>
        /// <param name="message">输出内容</param>
        private static void Log(int level, string message)
        {
            switch (level)
            {
                case 1: Debug.Log(message); break;
                case 2: Debug.LogWarning(message); break;
                case 3: Debug.LogError(message); break;
            }
        }


    }
}
