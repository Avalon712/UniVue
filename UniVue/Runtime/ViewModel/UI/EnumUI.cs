using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 枚举类型可以绑定的UI组件：TMP_Dropdrown、ToggleGroup
    /// </summary>
    public abstract class EnumUI : SingleValuePropertyUI, II18nPropertyUI
    {
        /// <summary>
        /// 当前的枚举值
        /// </summary>
        protected int _value = int.MinValue;

        private EnumValueInfo[] _enums;

        public int ValueCount => _enums.Length;

        protected EnumUI(string enumTypeFullName, string propertyName, bool allowUIUpdateModel) : base(propertyName, allowUIUpdateModel)
        {
            if (Enums.TryGetEnumInfo(enumTypeFullName, out EnumInfo enumInfo))
            {
                _enums = enumInfo.valueInfos;
            }
        }

        /// <summary>
        /// 根据枚举值获取枚举值的别名
        /// </summary>
        protected string GetAliasByValue(int value)
        {
            for (int i = 0; i < _enums.Length; i++)
            {
                if (_enums[i].intValue == value)
                    return _enums[i].GetAlias(Vue.language);
            }
            return string.Empty;
        }

        protected string GetAliasByIndex(int index)
        {
            return _enums[index].GetAlias(Vue.language);
        }

        protected string GetNewAlias(string oldAlias)
        {
            for (int i = 0; i < _enums.Length; i++)
            {
                if (_enums[i].ContainsAlias(oldAlias))
                    return _enums[i].GetAlias(Vue.language);
            }
            return oldAlias;
        }

        /// <summary> 
        /// 根据枚举值的别名获取枚举值
        /// </summary>
        protected int GetIntValueByAlias(string alias)
        {
            for (int i = 0; i < _enums.Length; i++)
            {
                if (_enums[i].TryGetIntValueByAlias(alias, out int value))
                    return value;
            }
            return -1;
        }

        protected string GetStringValueByIntValue(int value)
        {
            for (int i = 0; i < _enums.Length; i++)
            {
                if (_enums[i].intValue == value)
                    return _enums[i].stringValue;
            }
            return null;
        }

        protected int GetIntValueByStringValue(string value)
        {
            for (int i = 0; i < _enums.Length; i++)
            {
                if (_enums[i].stringValue == value)
                    return _enums[i].intValue;
            }
            return int.MinValue;
        }

        public virtual void OnLanguageEnvironmentChanged()
        {
            int v = _value;
            _value = int.MinValue; //让后面可以进行更新
            UpdateUI(v);
        }

        public sealed override void UpdateUI(bool propertyValue) { }

        public sealed override void UpdateUI(float propertyValue) { }

        public sealed override void UpdateUI(string propertyValue) { }

        public sealed override void UpdateUI(Sprite propertyValue) { }

    }

    public sealed class EnumToggleInfo
    {
        public Toggle toggle { get; }

        public TMP_Text text { get; }

        public string value
        {
            get => text.text;
            set => text.text = value;
        }

        public EnumToggleInfo(Toggle toggle, TMP_Text text)
        {
            this.toggle = toggle;
            this.text = text;
        }
    }
}
