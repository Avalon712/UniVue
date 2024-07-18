using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UniVue.i18n;
using UniVue.Rule;
using UniVue.Utils;

namespace UniVue.Filters
{
    /// <summary>
    /// 多语言化规则：
    /// <para>1. 只对TMP_Text和Image支持多语言化;</para>
    /// <para>2. 命名规则：I18n + ID + UI组件名称(可简写，只支持TMP_Text和Image组件) </para>
    /// <para>3. 举例说明（NamingFormat.UI_Suffix | NamingFormat.UnderlineUpper）：</para>
    /// <para>① I18n_Buy_Text：ID为"Buy"的UI组件当进行语言切换时其内容也会改变；</para>
    /// <para>② I18n_Buy[zh-CN=12,en-US=10]_Text：当语言为zh-CN时使用字体编号为12的字体资产、当语言为en-US时使用字体编号为10的字体资产</para>
    /// </summary>
    internal sealed class I18nFilter : IRuleFilter
    {
        private I18n _i18n;
        private int _index;

        public I18nFilter(ref I18n i18n)
        {
            _i18n = i18n;
            _index = (Vue.Config.Format & NamingFormat.UI_Suffix) == NamingFormat.UI_Suffix ? 1 : 2;
        }

        public bool Check(ref (Component, UIType) component, List<object> results)
        {
            if (component.Item2 == UIType.TMP_Text || component.Item2 == UIType.Image)
            {
                Match match = Regex.Match(component.Item1.name, GetRule0());
                if (match.Success)
                {
                    SwitchContent(ref component, match.Groups[_index].Value);
                }
            }
            return false;
        }

        private void SwitchContent(ref (Component, UIType) component, string id)
        {
            if (component.Item2 == UIType.TMP_Text)
            {
                TMP_Text text = component.Item1 as TMP_Text;
                if (!string.IsNullOrEmpty(id) && _i18n.TryGetContent(id, out string content))
                {
                    text.text = content;
                }
            }
            else if (component.Item2 == UIType.Image && _i18n.Loader != null)
            {
                Image image = component.Item1 as Image;
                image.sprite = _i18n.Loader.LoadSprite(_i18n.LanguageTag, id);
            }
        }

        public void OnComplete(List<object> results) { }

        #region 规则定义
        private string GetRule0()
        {
            switch (Vue.Config.Format)
            {
                case NamingFormat.CamelCase | NamingFormat.UI_Suffix:
                    return @"I18n(.+)(Text|Txt|Image)";
                case NamingFormat.CamelCase | NamingFormat.UI_Prefix:
                    return @"(Text|Txt|Image)I18n(.+)";

                case NamingFormat.UnderlineLower | NamingFormat.UI_Suffix:
                    return @"I18n_.+_(text|txt|image)";
                case NamingFormat.UnderlineLower | NamingFormat.UI_Prefix:
                    return @"(text|txt|image)_I18n_(.+)";

                case NamingFormat.SpaceLower | NamingFormat.UI_Suffix:
                    return @"I18n (.+) (text|txt|image)";
                case NamingFormat.SpaceLower | NamingFormat.UI_Prefix:
                    return @"(text|txt|image) I18n (.+)";

                case NamingFormat.SpaceUpper | NamingFormat.UI_Suffix:
                    return @"I18n (.+) (Text|Txt|Image)";
                case NamingFormat.SpaceUpper | NamingFormat.UI_Prefix:
                    return @"(Txt|Text|Image) I18n (.+)";

                case NamingFormat.UnderlineUpper | NamingFormat.UI_Suffix:
                    return @"I18n_(.+)_(Text|Txt|Image)";
                case NamingFormat.UnderlineUpper | NamingFormat.UI_Prefix:
                    return @"(Txt|Text|Image)_I18n_(.+)";
            }

            throw new NotSupportedException("非法的命名格式!");
        }

        #endregion
    }


}
