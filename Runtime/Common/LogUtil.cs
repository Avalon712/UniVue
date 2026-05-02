using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace UniVue.Common
{
    public static class LogUtil
    {
        [Conditional("UNITY_EDITOR")]
        public static void Info(object msg)
        {
            Debug.Log(msg);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Warn(object msg)
        {
            Debug.LogWarning(msg);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Error(object msg)
        {
            Debug.LogError(msg);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Exception(Exception e)
        {
            Debug.LogException(e);
        }

        [Conditional("UNITY_EDITOR")]
        public static void Assert(bool condition, object message)
        {
            Debug.Assert(condition, message);
        }
    }
}