#region

using TMPro;
using UnityEngine;
using UniVue.I18n;

#endregion

namespace UniVue.UI.Widgets
{
    /// <summary>
    /// 本地多语言文本组件
    /// </summary>
    [DisableUICodeGenerated]
    [RequireComponent(typeof(TMP_Text))]
    public sealed class LocalizationText : BaseUI
    {
        [field: SerializeField] public ulong TextId { get; private set; }

        private object[] _args;

        public TMP_Text TextUI { get; private set; }

        protected override void OnCreate()
        {
            TextUI = GetComponent<TMP_Text>();
            SetText(TextId);
            On(I18N.EVENT_LANGUAGE_CHANGED_COMPLETED, OnLanguageChanged);
        }

        protected override void OnActive()
        {
            if (TextUI.font != I18N.Font) TextUI.font = I18N.Font;
        }

        private void OnLanguageChanged()
        {
            if (_args == null)
                SetText(TextId);
            else
                SetText(TextId, _args);
            if (I18N.Font != null)
                TextUI.font = I18N.Font;
        }

        /// <summary>
        /// 直接设置默认语言下的文本（会自动翻译为当前语种下的文本）
        /// </summary>
        /// <param name="text">默认语言下的文本</param>
        public void SetText(string text)
        {
            SetText(I18N.LocalTextId(text));
        }

        /// <summary>
        /// 直接设置默认语言下的文本（会自动翻译为当前语种下的文本）
        /// </summary>
        /// <param name="text">默认语言下的文本</param>
        /// <param name="args">填充占位符的参数</param>
        public void SetText(string text, params object[] args)
        {
            SetText(I18N.LocalTextId(text), args);
        }

        /// <summary>
        /// 通过唯一文本Id设置文本
        /// </summary>
        /// <param name="id">唯一Id</param>
        public void SetText(ulong id)
        {
            TextId = id;
            TextUI.text = I18N.LocalText(id);
        }

        /// <summary>
        /// 通过唯一文本Id设置文本
        /// </summary>
        /// <param name="id">唯一Id</param>
        /// <param name="args">填充占位符的参数</param>
        public void SetText(ulong id, params object[] args)
        {
            TextId = id;
            _args = args;
            string text = I18N.LocalText(id);
            TextUI.text = string.Format(text, args);
        }
    }
}