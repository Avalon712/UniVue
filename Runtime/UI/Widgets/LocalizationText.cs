using TMPro;
using UnityEngine;
using UniVue.I18n;

namespace UniVue.UI.Widgets
{
    /// <summary>
    /// 本地多语言文本组件
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizationText : BaseUI
    {
        private TMP_Text _uiTxt;
        private object[] _args;
        
        [field: SerializeField]
        public long TextId { get; private set; }
        
        public TMP_Text TextUI => _uiTxt;
        
        protected override void OnCreate()
        {
            _uiTxt = GetComponent<TMP_Text>();
            SetText(TextId);
            On(I18N.EVENT_LANGUAGE_CHANGED, OnLanguageChanged);
        }

        protected override void OnActive()
        {
            if (_uiTxt.font != I18N.Font)
            {
                _uiTxt.font = I18N.Font;
            }
        }

        private void OnLanguageChanged()
        {
            if(_args == null)
                SetText(TextId);
            else
                SetText(TextId, _args);
            _uiTxt.font = I18N.Font;
        }
        
        public void SetText(long id)
        {
            TextId = id;
            _uiTxt.text = I18N.I10N(id);
        }
        
        public void SetText(long id, params object[] args)
        {
            TextId = id;
            _args = args;
            string text = I18N.I10N(id);
            _uiTxt.text = string.Format(text, args);
        }
    }
}