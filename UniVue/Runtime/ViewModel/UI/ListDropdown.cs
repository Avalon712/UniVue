using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniVue.Common;
using UniVue.i18n;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 将一个List类型的数据绑定到一个Dropdown的UI组件上
    /// </summary>
    public sealed class ListDropdown : CollectionPropertyUI
    {
        private TMP_Dropdown _dropdown;

        public ListDropdown(string propertyName, TMP_Dropdown dropdown) : base(propertyName)
        {
            _dropdown = dropdown;
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _dropdown as T;
        }

        public override void SetActive(bool active)
        {
            GameObjectUtil.SetActive(_dropdown.gameObject, active);
        }

        public override void UpdateUI(List<int> propertyValue)
        {
            for (int i = 0; i < propertyValue.Count; i++)
            {
                ShowItem(i, propertyValue[i].ToString());
            }
            TrimExcess(propertyValue.Count);
        }

        public override void UpdateUI(List<float> propertyValue)
        {
            string keepBit = $"F{Vue.Config.FloatKeepBit}";
            for (int i = 0; i < propertyValue.Count; i++)
            {
                ShowItem(i, propertyValue[i].ToString(keepBit));
            }
            TrimExcess(propertyValue.Count);
        }

        public override void UpdateUI(IList propertyValue)
        {
            object value = propertyValue.Count > 0 ? propertyValue[0] : null;
            if (value == null) return;

            EnumValueInfo[] valueInfos = Enums.TryGetEnumInfo(value.GetType().FullName, out var enumInfo) ? enumInfo.valueInfos : null;

            if (valueInfos == null) return;

            Language language = Vue.language;

            for (int i = 0; i < propertyValue.Count; i++)
            {
                int v = (int)propertyValue[i];
                for (int j = 0; j < valueInfos.Length; j++)
                {
                    if (valueInfos[j].intValue == v)
                    {
                        ShowItem(i, valueInfos[j].GetAlias(language));
                    }
                }
            }
            TrimExcess(propertyValue.Count);
        }

        public override void UpdateUI(List<string> propertyValue)
        {
            for (int i = 0; i < propertyValue.Count; i++)
            {
                ShowItem(i, propertyValue[i]);
            }
            TrimExcess(propertyValue.Count);
        }

        public override void UpdateUI(List<bool> propertyValue)
        {
            for (int i = 0; i < propertyValue.Count; i++)
            {
                ShowItem(i, propertyValue[i].ToString());
            }
            TrimExcess(propertyValue.Count);
        }

        public override void UpdateUI(List<Sprite> propertyValue)
        {
            for (int i = 0; i < propertyValue.Count; i++)
            {
                ShowItem(i, propertyValue[i]);
            }
            TrimExcess(propertyValue.Count);
        }

        private void ShowItem(int index, string item)
        {
            if (string.IsNullOrEmpty(_dropdown.captionText.text))
                _dropdown.captionText.text = item;

            if (_dropdown.options.Count > index)
                _dropdown.options[index].text = item;
            else
                _dropdown.options.Add(new TMP_Dropdown.OptionData(item));
        }

        private void ShowItem(int index, Sprite item)
        {
            if (_dropdown.captionImage != null && _dropdown.captionImage.sprite == null)
                _dropdown.captionImage.sprite = item;

            if (_dropdown.options.Count > index)
                _dropdown.options[index].image = item;
            else
                _dropdown.options.Add(new TMP_Dropdown.OptionData(item));
        }

        private void TrimExcess(int startIndex)
        {
            if (_dropdown.options.Count > startIndex)
                _dropdown.options.RemoveRange(startIndex, (_dropdown.options.Count - startIndex));
        }
    }
}
