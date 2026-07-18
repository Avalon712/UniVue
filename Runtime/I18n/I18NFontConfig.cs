#region

using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

#endregion

namespace UniVue.I18n
{
    /// <summary>
    /// 只推荐在编辑器模式下使用
    /// </summary>
    public sealed class I18NFontConfig : ScriptableObject, IFontProvider
    {
        public string defaultLanguage;

        public CfgLanguageFont[] fonts;

        public Task<TMP_FontAsset> GetFontAsync(string languageType, CancellationToken token = default)
        {
            if (token.IsCancellationRequested) return Task.FromCanceled<TMP_FontAsset>(token);

            if (fonts != null)
                for (int i = 0; i < fonts.Length; i++)
                {
                    CfgLanguageFont f = fonts[i];
                    if (f != null && f.language == languageType) return Task.FromResult(f.font);
                }

            return Task.FromResult<TMP_FontAsset>(null);
        }

        [Serializable]
        public sealed class CfgLanguageFont
        {
            public string language;
            public TMP_FontAsset font;
        }
    }
}