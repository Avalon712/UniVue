using System;
using System.Collections.Generic;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 枚举类型可以绑定的UI组件：TMP_Dropdrown、ToggleGroup
    /// </summary>
    public abstract class EnumUI : SingleValuePropertyUI
    {
        private static WeakReference<Dictionary<int, EnumAliasInfo[]>> _enumInfoCachePool;

        /// <summary>
        /// 当前的枚举值
        /// </summary>
        protected int _value;

        private Dictionary<int, EnumAliasInfo[]> _pool;

        private EnumAliasInfo[] _enums;

        public int EnumCount => _enums.Length;

        protected EnumUI(Type enumType, string propertyName, bool allowUIUpdateModel) : base(propertyName, allowUIUpdateModel)
        {
            CheckInitPool();
            int typeId = enumType.GetHashCode();
            if (_enumInfoCachePool.TryGetTarget(out _pool))
            {
                if (!_pool.TryGetValue(typeId, out _enums))
                {
                    CreateEnumAliasInfo(enumType);
                    _pool.Add(typeId, _enums);
                }
            }
            else
            {
                CreateEnumAliasInfo(enumType);
                _pool = new Dictionary<int, EnumAliasInfo[]> { { typeId, _enums } };
                _enumInfoCachePool.SetTarget(_pool);
            }
        }

        private void CheckInitPool()
        {
            if (_enumInfoCachePool == null)
            {
                _pool = new Dictionary<int, EnumAliasInfo[]>();
                _enumInfoCachePool = new WeakReference<Dictionary<int, EnumAliasInfo[]>>(_pool);
            }
        }

        private void CreateEnumAliasInfo(Type enumType)
        {
            Array array = Enum.GetValues(enumType);
            _enums = new EnumAliasInfo[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                EnumUtil.CreateEnumAliasInfo(out _enums[i], enumType, array.GetValue(i));
            }
        }

        public override void Unbind()
        {
            _notifier = null;
            _pool = null;
            _enums = null;
        }

        /// <summary>
        /// 根据枚举值获取枚举值的别名
        /// </summary>
        protected string GetAliasByValue(int value)
        {
            for (int i = 0; i < _enums.Length; i++)
            {
                if (_enums[i].Value0 == value)
                    return _enums[i].GetAlias(Vue.LanguageTag);
            }
            return null;
        }

        protected string GetAliasByIndex(int index)
        {
            return _enums[index].GetAlias(Vue.LanguageTag);
        }

        protected string GetNewAlias(string oldAlias)
        {
            for (int i = 0; i < _enums.Length; i++)
            {
                if (_enums[i].ContainsAlias(oldAlias))
                    return _enums[i].GetAlias(Vue.LanguageTag);
            }
            return oldAlias;
        }

        /// <summary> 
        /// 根据枚举值的别名获取枚举值
        /// </summary>
        protected int GetValue(string alias)
        {
            for (int i = 0; i < _enums.Length; i++)
            {
                if (_enums[i].TryGetValue(alias, out int value))
                    return value;
            }
            return -1;
        }

        internal virtual void UpdateUI()
        {
            UpdateUI(_value);
        }

        public sealed override void UpdateUI(bool propertyValue) { }

        public sealed override void UpdateUI(float propertyValue) { }

        public sealed override void UpdateUI(string propertyValue) { }

        public sealed override void UpdateUI(Sprite propertyValue) { }
    }

    internal struct EnumAliasInfo
    {
        private List<EnumAliasAttribute> _aliases;

        /// <summary>
        /// 枚举值的int形式
        /// </summary>
        public int Value0 { get; set; }

        /// <summary>
        /// 枚举值
        /// </summary>
        public object Value1 { get; set; }

        /// <summary>
        /// 枚举值的字符串形式
        /// </summary>
        public string Value2 { get; set; }

        public EnumAliasInfo(int value0, object value1, string value2, List<EnumAliasAttribute> aliases)
        {
            _aliases = aliases;
            Value0 = value0;
            Value1 = value1;
            Value2 = value2;
        }

        public string GetAlias(string languageTag)
        {
            if (_aliases != null)
            {
                for (int i = 0; i < _aliases.Count; i++)
                {
                    if (_aliases[i].Languag == languageTag)
                    {
                        return _aliases[i].Alias;
                    }
                }
            }
            return Value2;
        }

        public bool ContainsAlias(string oldAlias)
        {
            if (_aliases != null)
            {
                for (int i = 0; i < _aliases.Count; i++)
                {
                    if (_aliases[i].Alias == oldAlias)
                    {
                        return true;
                    }
                }
            }
            return Value2 == oldAlias;
        }

        public bool TryGetValue(string alias, out int value)
        {
            value = Value0;
            if (_aliases != null)
            {
                for (int i = 0; i < _aliases.Count; i++)
                {
                    if (_aliases[i].Alias == alias)
                        return true;
                }
            }
            return Value2 == alias;
        }
    }

}
