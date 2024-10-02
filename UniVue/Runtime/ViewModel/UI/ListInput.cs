using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UniVue.Common;
using UniVue.i18n;

namespace UniVue.ViewModel
{
    public sealed class ListInput : CollectionPropertyUI
    {
        private TMP_InputField[] _texts;

        public ListInput(string propertyName, TMP_InputField[] inputFields) : base(propertyName)
        {
            _texts = inputFields;
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
                GameObjectUtil.SetActive(_texts[i].gameObject, active);
            }
        }

        public override void UpdateUI(List<int> propertyValue)
        {
            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;
            for (int i = 0; i < _texts.Length; i++)
            {
                if (i < propertyValue.Count)
                    _texts[i].text = propertyValue[i].ToString();
                GameObjectUtil.SetActive(_texts[i].gameObject, i < propertyValue.Count | config);
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
                GameObjectUtil.SetActive(_texts[i].gameObject, i < propertyValue.Count | config);
            }
        }

        public override void UpdateUI(IList propertyValue)
        {
            int count = propertyValue.Count;
            object value = count > 0 ? propertyValue[0] : null;
            if (value == null) return;

            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;

            EnumValueInfo[] valueInfos = Enums.TryGetEnumInfo(value.GetType().FullName, out var enumInfo) ? enumInfo.valueInfos : null;

            if (valueInfos == null) return;

            Language language = Vue.language;

            for (int i = 0; i < _texts.Length; i++)
            {
                if (i < count)
                {
                    int v = (int)propertyValue[i];
                    for (int j = 0; j < valueInfos.Length; j++)
                    {
                        if (valueInfos[j].intValue == v)
                        {
                            _texts[i].text = valueInfos[j].GetAlias(language);
                        }
                    }
                }
                GameObjectUtil.SetActive(_texts[i].gameObject, i < propertyValue.Count | config);
            }
        }

        public override void UpdateUI(List<string> propertyValue)
        {
            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;
            for (int i = 0; i < _texts.Length; i++)
            {
                if (i < propertyValue.Count)
                    _texts[i].text = propertyValue[i];
                GameObjectUtil.SetActive(_texts[i].gameObject, i < propertyValue.Count | config);
            }
        }

        public override void UpdateUI(List<bool> propertyValue)
        {
            bool config = !Vue.Config.WhenListLessUICountThenHideSurplus;
            for (int i = 0; i < _texts.Length; i++)
            {
                if (i < propertyValue.Count)
                    _texts[i].text = propertyValue[i].ToString();
                GameObjectUtil.SetActive(_texts[i].gameObject, i < propertyValue.Count | config);
            }
        }

        public override void UpdateUI(List<Sprite> propertyValue) { }
    }
}
