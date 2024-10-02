using System.Collections.Generic;
using System.Text;
using TMPro;
using UniVue.Common;

namespace UniVue.ViewModel
{
    public sealed class FlagsEnumText : EnumUI
    {
        private readonly TMP_Text _text;
        private readonly TextTemplate[] _templates;
        private readonly StringBuilder _builder = new StringBuilder();

        /// <summary>
        /// 当前语言环境下的文本模板
        /// </summary>
        public string Template { get; set; }

        public FlagsEnumText(TMP_Text ui, string enumTypeFullName, string propertyName) : base(enumTypeFullName, propertyName, false)
        {
            _text = ui;
            if (TextTemplate.TryParse(ui.text, out _templates))
            {
                Template = TextTemplate.CurrentLanguageEnvironmentTemplate(_templates).template;
            }
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _text as T;
        }

        public override void SetActive(bool active)
        {
            GameObjectUtil.SetActive(_text.gameObject, active);
        }

        public override void UpdateUI(int propertyValue)
        {
            if (_value == propertyValue) return;

            _value = propertyValue;
            string separator = Vue.Config.FlagsEnumSeparator;
            int count = ValueCount;
            for (int i = 0; i < count; i++)
            {
                string alias = GetAliasByIndex(i);
                int value = GetIntValueByAlias(alias);
                if ((value & propertyValue) == value)
                {
                    if (_builder.Length > 0)
                    {
                        _builder.Append(separator);
                    }
                    _builder.Append(alias);
                }
            }
            string str = _builder.ToString();
            SetActive(!string.IsNullOrEmpty(str) || !Vue.Config.WhenValueIsNullThenHide);

            if (!string.IsNullOrEmpty(Template))
                _text.text = Template.Replace(TextTemplate.SYMBOL, str);
            else
                _text.text = str;

            _builder.Clear();
        }

        public override void OnLanguageEnvironmentChanged()
        {
            if (_templates != null)
            {
                Template = TextTemplate.CurrentLanguageEnvironmentTemplate(_templates).template;
            }
            base.OnLanguageEnvironmentChanged();
        }
    }
}
