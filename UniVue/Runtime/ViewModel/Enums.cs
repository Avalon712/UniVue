using System.Collections.Generic;
using UniVue.i18n;

namespace UniVue.ViewModel
{
    public static class Enums
    {
        private static readonly Dictionary<string, EnumInfo> _enums = new Dictionary<string, EnumInfo>();

        public static bool TryGetEnumInfo(string typeFullName, out EnumInfo enumInfo)
        {
            return _enums.TryGetValue(typeFullName, out enumInfo);
        }
        public static void AddEnumInfo(EnumInfo enumInfo)
        {
            _enums.TryAdd(enumInfo.typeFullName, enumInfo);
        }
    }

    public sealed class EnumInfo
    {
        public readonly string typeFullName;

        internal readonly EnumValueInfo[] valueInfos;

        public EnumInfo(string typeFullName, EnumValueInfo[] valueInfos)
        {
            this.typeFullName = typeFullName;
            this.valueInfos = valueInfos;
        }

        public bool TryGetValue(int intValue, out EnumValueInfo valueInfo)
        {
            valueInfo = null;
            for (int i = 0; i < valueInfos.Length; i++)
            {
                if (valueInfos[i].intValue == intValue)
                {
                    valueInfo = valueInfos[i];
                    break;
                }
            }
            return valueInfo != null;
        }

        public bool TryGetValue(string strValue, out EnumValueInfo valueInfo)
        {
            valueInfo = null;
            for (int i = 0; i < valueInfos.Length; i++)
            {
                if (valueInfos[i].stringValue == strValue)
                {
                    valueInfo = valueInfos[i];
                    break;
                }
            }
            return valueInfo != null;
        }
    }

    public sealed class EnumValueInfo
    {
        private readonly AliasInfo[] _aliases;

        /// <summary>
        /// 枚举值的int形式
        /// </summary>
        public readonly int intValue;

        /// <summary>
        /// 枚举值的字符串形式
        /// </summary>
        public readonly string stringValue;

        /// <summary>
        /// 枚举值
        /// </summary>
        public readonly object enumValue;

        public EnumValueInfo(int intValue, string stringValue, object enumValue, AliasInfo[] aliases)
        {
            _aliases = aliases;
            this.enumValue = enumValue;
            this.intValue = intValue;
            this.stringValue = stringValue;
        }

        public string GetAlias(Language language)
        {
            if (_aliases != null)
            {
                for (int i = 0; i < _aliases.Length; i++)
                {
                    if (_aliases[i].language == language)
                    {
                        return _aliases[i].alias;
                    }
                }
            }
            return stringValue;
        }

        public bool ContainsAlias(string oldAlias)
        {
            if (_aliases != null)
            {
                for (int i = 0; i < _aliases.Length; i++)
                {
                    if (_aliases[i].alias == oldAlias)
                    {
                        return true;
                    }
                }
            }
            return stringValue == oldAlias;
        }

        public bool TryGetIntValueByAlias(string alias, out int value)
        {
            value = intValue;
            if (_aliases != null)
            {
                for (int i = 0; i < _aliases.Length; i++)
                {
                    if (_aliases[i].alias == alias)
                        return true;
                }
            }
            return stringValue == alias;
        }
    }

    public sealed class AliasInfo
    {
        public readonly Language language;
        public readonly string alias;

        public AliasInfo(Language language, string alias)
        {
            this.language = language;
            this.alias = alias;
        }
    }
}
