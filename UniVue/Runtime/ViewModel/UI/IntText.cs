using System.Collections.Generic;
using TMPro;

namespace UniVue.ViewModel
{
    public sealed class IntText : IntUI<TMP_Text>, II18nPropertyUI
    {
        private readonly TextTemplate[] _templates;

        /// <summary>
        /// 当前语言环境下的文本模板
        /// </summary>
        public string Template { get; set; }

        public IntText(TMP_Text ui, string propertyName) : base(ui, propertyName, false)
        {
            if (TextTemplate.TryParse(ui.text, out _templates))
            {
                Template = TextTemplate.CurrentLanguageEnvironmentTemplate(_templates).template;
            }
        }

        public override IEnumerable<T> GetUI<T>()
        {
            yield return _ui as T;
        }

        public override void UpdateUI(int propertyValue)
        {
            if (_value != propertyValue)
            {
                _value = propertyValue;
                if (!string.IsNullOrEmpty(Template))
                    _ui.text = Template.Replace(TextTemplate.SYMBOL, propertyValue.ToString());
                else
                    _ui.text = propertyValue.ToString();
            }
        }

        public void OnLanguageEnvironmentChanged()
        {
            if (_templates != null)
            {
                Template = TextTemplate.CurrentLanguageEnvironmentTemplate(_templates).template;
                int v = _value;
                _value = int.MinValue;
                UpdateUI(v);
            }
        }
    }
}
