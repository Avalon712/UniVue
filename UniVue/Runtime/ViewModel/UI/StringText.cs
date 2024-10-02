using TMPro;

namespace UniVue.ViewModel
{
    public sealed class StringText : StringUI<TMP_Text>, II18nPropertyUI
    {
        private readonly TextTemplate[] _templates;

        /// <summary>
        /// 当前语言环境下的文本模板
        /// </summary>
        public string Template { get; set; }

        public StringText(TMP_Text ui, string propertyName) : base(ui, propertyName, false)
        {
            if (TextTemplate.TryParse(ui.text, out _templates))
            {
                Template = TextTemplate.CurrentLanguageEnvironmentTemplate(_templates).template;
            }
        }

        public override void UpdateUI(string propertyValue)
        {
            if (_value == propertyValue) return;
            SetActive(!string.IsNullOrEmpty(propertyValue) || !Vue.Config.WhenValueIsNullThenHide);
            _value = propertyValue;
            if (!string.IsNullOrEmpty(Template))
                propertyValue = Template.Replace(TextTemplate.SYMBOL, propertyValue);
            _ui.text = propertyValue;
        }

        public void OnLanguageEnvironmentChanged()
        {
            if (_templates != null)
            {
                Template = TextTemplate.CurrentLanguageEnvironmentTemplate(_templates).template;
                string v = _value;
                _value = string.Empty;
                UpdateUI(v);
            }
        }
    }
}
