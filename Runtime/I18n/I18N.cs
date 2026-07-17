using System;
using TMPro;
using UniVue.Event;
using UniVue.Utils;

namespace UniVue.I18n
{
    public static class I18N
    {
        /// <summary>
        /// 语言环境发生变化时调用
        /// </summary>
        public const string EVENT_LANGUAGE_CHANGED = nameof(EVENT_LANGUAGE_CHANGED);
        
        /// <summary>
        /// 当前的语言环境
        /// </summary>
        public static string Language { get; private set; }
        
        public static string DefaultLanguage { get; private set; }
        
        public static II10NFileReader Reader { get; private set; }
        
        public static I18NConfig Config { get; private set; }

        /// <summary>
        /// 当前语言环境下使用的字体
        /// </summary>
        public static TMP_FontAsset Font => Config.GetFont(Language);
        
        public static void Initialize(II10NFileReader reader, I18NConfig config)
        {
            ExceptionUtils.ThrowIfArgNull(reader, nameof(reader));
            Reader = reader;
            Config = config;
            DefaultLanguage = config.defaultLanguage;
            SwitchLanguage(config.defaultLanguage);
        }
        
        /// <summary>
        /// 切换语言环境
        /// </summary>
        /// <param name="language">目标语言类型</param>
        /// <param name="completed">切换完成时回调</param>
        public static void SwitchLanguage(string language, Action<bool> completed = null)
        {
            if (Reader == null)
            {
                completed?.Invoke(false);
                return;
            }
            if (language == Language)
            {
                completed?.Invoke(true);
                return;
            }
            Reader.ReadLanguageFile(Language, (success) =>
            {
                if (success)
                {
                    Language = language;
                    EventMgr.Dispatch(EVENT_LANGUAGE_CHANGED);
                }
                completed?.Invoke(success);
            });
        }
        
        /// <summary>
        /// 获取当前语言环境下的文本
        /// </summary>
        /// <param name="id">文本唯一id</param>
        /// <returns>当前语言环境下的文本</returns>
        public static string I10N(long id)
        {
            return Reader.Read(id, Language);
        }

        /// <summary>
        /// 获取当前语言环境下的文本
        /// </summary>
        /// <param name="text">默认语言环境下的文本</param>
        /// <returns>当前语言环境下的文本</returns>
        public static string I10N(string text)
        {
            return I10N(CoreUtils.GenerateUniqueId(text));
        }
    }
}