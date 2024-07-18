using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.ViewModel
{
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
            ViewUtil.SetActive(_dropdown.gameObject, active);
        }

        public override void Unbind()
        {
            _dropdown = null;
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
            Type enumType = value.GetType();

            for (int i = 0; i < propertyValue.Count; i++)
            {
                ShowItem(i, EnumUtil.GetEnumAlias(Vue.LanguageTag, enumType, propertyValue[i].ToString()));
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
            if (_dropdown.options.Count > index)
                _dropdown.options[index].text = item;
            else
                _dropdown.options.Add(new TMP_Dropdown.OptionData(item));
        }

        private void ShowItem(int index, Sprite item)
        {
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
