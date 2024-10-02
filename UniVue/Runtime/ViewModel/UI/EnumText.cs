using System.Collections.Generic;
using TMPro;
using UniVue.Common;

namespace UniVue.ViewModel
{
    public sealed class EnumText : EnumUI
    {
        private readonly TMP_Text _text;
        private readonly TextTemplate[] _templates;

        /// <summary>
        /// 当前语言环境下的文本模板
        /// </summary>
        public string Template { get; set; }

        public EnumText(TMP_Text ui, string enumTypeFullName, string propertyName) : base(enumTypeFullName, propertyName, false)
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
            if (_value != propertyValue)
            {
                _value = propertyValue;
                string v = GetAliasByValue(propertyValue);
                SetActive(!string.IsNullOrEmpty(v) || !Vue.Config.WhenValueIsNullThenHide);
                if (!string.IsNullOrEmpty(Template))
                    _text.text = Template.Replace(TextTemplate.SYMBOL, v);
                else
                    _text.text = v;
            }
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
