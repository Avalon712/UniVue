using System;
using System.Diagnostics;

namespace UniVue.Common
{
    public static class ThrowUtil
    {
        public static void ThrowExceptionIfNull(object obj, string message)
        {
            if (Equals(obj, null))
                throw new NullReferenceException(message);
        }

        public static void ThrowExceptionIfTrue(bool flag, string message)
        {
            if (flag)
                throw new Exception(message);
        }

        public static void ThrowException(string message)
        {
            throw new Exception(message);
        }

        [Conditional("UNITY_EDITOR")]
        public static void ThrowWarn(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogWarning(message);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void ThrowWarnIfTrue(bool flag, string message)
        {
#if UNITY_EDITOR
            if (flag)
                UnityEngine.Debug.LogWarning(message);
#endif
        }

        [Conditional("UNITY_EDITOR")]
        public static void ThrowLog(string message)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log(message);
#endif
        }
    }
}
