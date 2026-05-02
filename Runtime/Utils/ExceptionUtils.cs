using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace UniVue.Utils
{
    public static class ExceptionUtils
    {
        public static void ThrowIfArgNull(object obj, string paramName)
        {
            if (obj == null) Debug.LogException(new ArgumentNullException(paramName));
        }

        public static void ThrowIfNull(object obj, string message)
        {
            if (obj == null) Debug.LogException(new NullReferenceException(message));
        }

        public static void ThrowIfTrue(bool flag, string message)
        {
            if (flag) Debug.LogException(new AssertionException(message, string.Empty));
        }

        public static void ThrowIfFalse(bool flag, string message)
        {
            if (!flag) Debug.LogException(new AssertionException(message, string.Empty));
        }
    }
}