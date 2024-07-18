using System;
using System.Collections.Generic;
using System.Reflection;
using UniVue.Model;
using UniVue.ViewModel;

namespace UniVue.Utils
{
    public static class EnumUtil
    {
        /// <summary>
        /// 获取一个枚举类上是否有[Flags]特性
        /// </summary>
        public static bool HasFlags(Type type)
        {
            return type.GetCustomAttribute<FlagsAttribute>(false) != null;
        }

        /// <summary>
        /// 获取枚举值的别名
        /// </summary>
        /// <param name="enumType">枚举类型</param>
        /// <param name="enumValueStr">枚举值的字符串</param>
        /// <returns>如果有别名则返回别名，没有则返回enumValueStr</returns>
        public static string GetEnumAlias(string languageTag, Type enumType, string enumValueStr)
        {
            FieldInfo fieldInfo = enumType.GetField(enumValueStr);
            if (fieldInfo != null)
            {
                using (var it = fieldInfo.GetCustomAttributes<EnumAliasAttribute>().GetEnumerator())
                {
                    while (it.MoveNext())
                    {
                        if (string.IsNullOrEmpty(languageTag))
                        {
                            return it.Current.Alias;
                        }
                        else if (it.Current.Languag == languageTag)
                        {
                            return it.Current.Alias;
                        }
                    }
                }
            }
            return enumValueStr;
        }

        internal static void CreateEnumAliasInfo(out EnumAliasInfo info, Type enumType, object enumValue)
        {
            string enumValueStr = enumValue.ToString();
            FieldInfo fieldInfo = enumType.GetField(enumValueStr);
            List<EnumAliasAttribute> attributes = null;
            using (var it = fieldInfo.GetCustomAttributes<EnumAliasAttribute>().GetEnumerator())
            {
                while (it.MoveNext())
                {
                    if (attributes == null) { attributes = new List<EnumAliasAttribute>(); }
                    attributes.Add(it.Current);
                }
            }
            info = new EnumAliasInfo(Convert.ToInt32(enumValue), enumValue, enumValueStr, attributes);
        }

        public static bool IsEnum(BindableType bindType, Type type, out bool isFlagsEnum)
        {
            isFlagsEnum = false;
            if (bindType == BindableType.Enum)
            {
                isFlagsEnum = HasFlags(type);
                return true;
            }
            return false;
        }
    }
}
