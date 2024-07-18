using System;
using System.Collections.Generic;
using UniVue.Filters;

namespace UniVue.i18n
{
    internal struct I18n
    {
        private II18nResourceLoader _loader;
        private Language _language;
        private Dictionary<string, string> _contents;

        /// <summary>
        /// 当前游戏中的语言标识
        /// </summary>
        public string LanguageTag { get; }

        public II18nResourceLoader Loader { get { return _loader; } }

        /// <summary>
        /// 创建多语言对象
        /// </summary>
        /// <param name="language">游戏中要显示的语言</param>
        /// <param name="parser">I18n文件解析器，如果为null则将使用内置的属性文件解析方法</param>
        /// <param name="loader">游戏中与多语言相关资产（精灵图片、字体）加载器</param>
        public I18n(Language language, II18nResourceLoader loader)
        {
            _loader = loader;
            _language = language;
            LanguageTag = language.Tag;
            _contents = null;
        }

        /// <summary>
        /// 切换到目标语言
        /// </summary>
        /// <remarks>同步方式</remarks>
        public void Switch(Action onComplete)
        {
            _contents = _loader.LoadContents(_language);
            I18nFilter filter = new I18nFilter(ref this);
            Vue.Rule.AsyncFilterAll(filter, onComplete);
        }

        public bool TryGetContent(string contentID, out string content)
        {
            return _contents.TryGetValue(contentID, out content);
        }

    }
}
