using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace UniVue.Editor
{
    internal static class ReflectionUtils
    {
        private const BindingFlags ACCESS = BindingFlags.Static |
                BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static bool IsListFlagsEnumType(Type type)
        {
            if (type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type[] types = type.GenericTypeArguments;
                if (types != null && types.Length == 1)
                {
                    if (types[0].IsEnum && types[0].GetCustomAttribute<FlagsAttribute>() != null) { return true; }
                }
            }
            return false;
        }

        public static bool IsFlagsEnumType(Type type)
        {
            return type.GetCustomAttribute<FlagsAttribute>() != null;
        }

        public static T GetFieldValue<T>(object obj, string fieldName)
        {
            return (T)obj.GetType().GetField(fieldName, ACCESS).GetValue(obj);
        }

        public static T GetPropertyValue<T>(object obj, string propertyName)
        {
            return (T)obj.GetType().GetProperty(propertyName, ACCESS).GetValue(obj);
        }

        public static void SetFieldValue(object obj, string fieldName, object value)
        {
            obj.GetType().GetField(fieldName, ACCESS).SetValue(obj, value);
        }

        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            obj.GetType().GetProperty(propertyName, ACCESS).SetValue(obj, value);
        }

        public static void InvokeMethod(object obj, string methodName, params object[] arguments)
        {
            obj.GetType().GetMethod(methodName, ACCESS).Invoke(obj, arguments);
        }

        /// <summary>
        /// 深拷贝一个对象
        /// </summary>
        /// <param name="obj">要拷贝的对象</param>
        /// <returns>拷贝后的对象</returns>
        public static object DeepCopy(object obj)
        {
            object copy = null;
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                copy = formatter.Deserialize(ms);
            }
            return copy;
        }
    }

}
