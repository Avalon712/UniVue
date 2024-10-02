using TMPro;

namespace UniVue.ViewModel
{
    public sealed class FloatText : FloatUI<TMP_Text>, II18nPropertyUI
    {
        private readonly TextTemplate[] _templates;

        /// <summary>
        /// 当前语言环境下的文本模板
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// 指定浮点数保留的位数
        /// </summary>
        public int KeepBit { get; set; }

        public FloatText(TMP_Text ui, string propertyName) : base(ui, propertyName, false)
        {
            KeepBit = Vue.Config.FloatKeepBit;
            if (TextTemplate.TryParse(ui.text, out _templates))
            {
                Template = TextTemplate.CurrentLanguageEnvironmentTemplate(_templates).template;
            }
        }

        public override void UpdateUI(float propertyValue)
        {
            if (_value != propertyValue)
            {
                _value = propertyValue;
                if (!string.IsNullOrEmpty(Template))
                    _ui.text = Template.Replace(TextTemplate.SYMBOL, propertyValue.ToString("F" + KeepBit));
                else
                    _ui.text = propertyValue.ToString("F" + KeepBit);
            }
        }

        public void OnLanguageEnvironmentChanged()
        {
            if (_templates != null)
            {
                Template = TextTemplate.CurrentLanguageEnvironmentTemplate(_templates).template;
                float v = _value;
                _value = float.MinValue;
                UpdateUI(v);
            }
        }
    }
}
