using System;
using TMPro;
using UnityEngine;

namespace UniVue.I18n
{
    public sealed class I18NConfig : ScriptableObject
    {
        [Serializable]
        public sealed class I18NFontConfig
        {
            public string language;
            public TMP_FontAsset font;
        }
        
        public string defaultLanguage;
        
        public I18NFontConfig[] fonts;

        public TMP_FontAsset GetFont(string language)
        {
            foreach (I18NFontConfig f in fonts)
            {
                if (f.language == language)
                {
                    return f.font;
                }
            }
            return null;
        }
    }

    
}