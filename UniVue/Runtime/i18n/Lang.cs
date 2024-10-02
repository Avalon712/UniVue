
namespace UniVue.i18n
{
    /// <summary>
    /// <see cref="Language"/>的字符串形式
    /// </summary>
    public static class Lang
    {
        public const string None = nameof(Language.None);

        #region 中文  

        public const string zh = nameof(Language.zh);
        public const string zh_CN = nameof(Language.zh_CN);
        public const string zh_HK = nameof(Language.zh_HK);
        public const string zh_MO = nameof(Language.zh_MO);
        public const string zh_TW = nameof(Language.zh_TW);

        #endregion

        #region 英语  

        public const string en = nameof(Language.en);
        public const string en_US = nameof(Language.en_US);
        public const string en_GB = nameof(Language.en_GB);
        public const string en_CA = nameof(Language.en_CA);

        #endregion

        #region 日语  

        public const string ja_JP = nameof(Language.ja_JP);

        #endregion

        #region 德语  

        public const string de = nameof(Language.de);
        public const string de_DE = nameof(Language.de_DE);

        #endregion

        #region 西班牙语  

        public const string es = nameof(Language.es);
        public const string es_ES = nameof(Language.es_ES);
        public const string es_AR = nameof(Language.es_AR);
        public const string es_MX = nameof(Language.es_MX);

        #endregion

        #region 葡萄牙语  

        public const string pt = nameof(Language.pt);
        public const string pt_BR = nameof(Language.pt_BR);
        public const string pt_PT = nameof(Language.pt_PT);

        #endregion

        #region 法语  

        public const string fr = nameof(Language.fr);
        public const string fr_FR = nameof(Language.fr_FR);

        #endregion

        #region 意大利语  

        public const string it_IT = nameof(Language.it_IT);

        #endregion

        #region 朝鲜语  

        public const string ko_KR = nameof(Language.ko_KR);

        #endregion

        #region 俄语  

        public const string ru_RU = nameof(Language.ru_RU);

        #endregion

        #region 波兰语  

        public const string pl_PL = nameof(Language.pl_PL);

        #endregion

        #region 土耳其语  

        public const string tr_TR = nameof(Language.tr_TR);

        #endregion

        #region 阿拉伯语  

        public const string ar = nameof(Language.ar);
        public const string ar_AE = nameof(Language.ar_AE);
        public const string ar_EG = nameof(Language.ar_EG);

        #endregion

        #region 泰语  

        public const string th_TH = nameof(Language.th_TH);

        #endregion

        #region 荷兰语  

        public const string nl_NL = nameof(Language.nl_NL);

        #endregion

        #region 越南语  

        public const string vi_VN = nameof(Language.vi_VN);

        #endregion

        public static bool TryParse(string lang, out Language language)
        {
            language = Language.None;
            switch (lang)
            {
                case None:
                    return true;
                case zh:
                    language = Language.zh;
                    return true;
                case zh_CN:
                    language = Language.zh_CN;
                    return true;
                case zh_HK:
                    language = Language.zh_HK;
                    return true;
                case zh_MO:
                    language = Language.zh_MO;
                    return true;
                case zh_TW:
                    language = Language.zh_TW;
                    return true;
                case en:
                    language = Language.en;
                    return true;
                case en_US:
                    language = Language.en_US;
                    return true;
                case en_GB:
                    language = Language.en_GB;
                    return true;
                case en_CA:
                    language = Language.en_CA;
                    return true;
                case ja_JP:
                    language = Language.ja_JP;
                    return true;
                case de:
                    language = Language.de;
                    return true;
                case es:
                    language = Language.es;
                    return true;
                case pt:
                    language = Language.pt;
                    return true;
                case fr:
                    language = Language.fr;
                    return true;
                case it_IT:
                    language = Language.it_IT;
                    return true;
                case ko_KR:
                    language = Language.ko_KR;
                    return true;
                case ru_RU:
                    language = Language.ru_RU;
                    return true;
                case pl_PL:
                    language = Language.pl_PL;
                    return true;
                case tr_TR:
                    language = Language.tr_TR;
                    return true;
                case ar:
                    language = Language.ar;
                    return true;
                case th_TH:
                    language = Language.th_TH;
                    return true;
                case nl_NL:
                    language = Language.nl_NL;
                    return true;
                case vi_VN:
                    language = Language.vi_VN;
                    return true;
                default:
                    return false;
            }
        }

    }
}
