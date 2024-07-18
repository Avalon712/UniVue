using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.ViewModel
{
    public sealed class ListText : CollectionPropertyUI
    {
        private TMP_Text[] _texts;

        public ListText(string propertyName, TMP_Text[] texts) : base(propertyName)
        {
            _texts = texts;
        }

        public override IEnumerable<T> GetUI<T>()
        {
            for (int i = 0; i < _texts.Length; i++)
            {
                yield return _texts[i] as T;
            }
        }

        public override void SetActive(bool active)
        {
            for (int i = 0; i < _texts.Length; i++)
            {
                ViewUtil.SetActive(_texts[i].gameObject, active);
            }
        }

        public override void Unbind()
        {
            for (int i = 0; i < _texts.Length; i++)
            {
                _texts[i] = null;
            }
            _texts = null;
        }

        public override void UpdateUI(List<int> propertyValue)
        {
            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;
            for (int i = 0; i < _texts.Length; i++)
            {
                if (i < propertyValue.Count)
                    _texts[i].text = propertyValue[i].ToString();
                ViewUtil.SetActive(_texts[i].gameObject, i < propertyValue.Count);
            }
        }

        public override void UpdateUI(List<float> propertyValue)
        {
            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;
            string keepBit = $"F{Vue.Config.FloatKeepBit}";
            for (int i = 0; i < _texts.Length; i++)
            {
                if (i < propertyValue.Count)
                    _texts[i].text = propertyValue[i].ToString(keepBit);
                ViewUtil.SetActive(_texts[i].gameObject, i < propertyValue.Count | config);
            }
        }

        public override void UpdateUI(IList propertyValue)
        {
            object value = propertyValue.Count > 0 ? propertyValue[0] : null;
            if (value == null) return;
            Type enumType = value.GetType();
            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;

            for (int i = 0; i < _texts.Length; i++)
            {
                if (i < propertyValue.Count)
                    _texts[i].text = EnumUtil.GetEnumAlias(Vue.LanguageTag, enumType, propertyValue[i].ToString());
                ViewUtil.SetActive(_texts[i].gameObject, i < propertyValue.Count | config);
            }
        }

        public override void UpdateUI(List<string> propertyValue)
        {
            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;
            for (int i = 0; i < _texts.Length; i++)
            {
                if (i < propertyValue.Count)
                    _texts[i].text = propertyValue[i];
                ViewUtil.SetActive(_texts[i].gameObject, i < propertyValue.Count | config);
            }
        }

        public override void UpdateUI(List<bool> propertyValue)
        {
            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;
            for (int i = 0; i < _texts.Length; i++)
            {
                if (i < propertyValue.Count)
                    _texts[i].text = propertyValue[i].ToString();
                ViewUtil.SetActive(_texts[i].gameObject, i < propertyValue.Count | config);
            }
        }

        public override void UpdateUI(List<Sprite> propertyValue) { }
    }
}
