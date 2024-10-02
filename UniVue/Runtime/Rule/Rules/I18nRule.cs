using UnityEngine;
using UniVue.Common;
using UniVue.Rule;

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
    public sealed class I18nRule : IRule
    {
        private const string RULE = @"I18n_(.+)_(Text|Txt|Image|Img)";

        //private I18n _i18n;
        //private int _index;

        //public event Action onCompleted;

        //public I18nRule(ref I18n i18n)
        //{
        //    _i18n = i18n;
        //    _index = (Vue.Config.Format & NamingFormat.UI_Suffix) == NamingFormat.UI_Suffix ? 1 : 2;
        //}

        public bool Check(string rule, UIType uiType, Component ui)
        {
            //if (uiType == UIType.TMP_Text || uiType == UIType.Image)
            //{
            //    Match match = Regex.Match(ui.name, GetRule0());
            //    if (match.Success)
            //    {
            //        ChangeContent(uiType, ui, match.Groups[_index].Value);
            //    }
            //}
            return false;
        }

        private void ChangeContent(UIType uiType, Component ui, string id)
        {
            //if (uiType == UIType.TMP_Text)
            //{
            //    TMP_Text text = ui as TMP_Text;
            //    if (!string.IsNullOrEmpty(id) && _i18n.TryGetContent(id, out string content))
            //    {
            //        text.text = content;
            //    }
            //}
            //else if (uiType == UIType.Image && _i18n.Loader != null)
            //{
            //    Image image = ui as Image;
            //    image.sprite = _i18n.Loader.LoadSprite(_i18n.language, id);
            //}
        }

        public void OnComplete()
        {
            //onCompleted?.Invoke();
        }

    }


}
