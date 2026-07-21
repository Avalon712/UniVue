#region

using System;
using System.Collections.Generic;

#endregion

namespace UniVue.Editor
{
    public interface ITextTranslator
    {
        /// <summary>
        /// 翻译语言
        /// </summary>
        /// <param name="results">翻译结果</param>
        /// <param name="onCompleted">翻译完成时回调，参数为results</param>
        public void Translate(TranslationResults results, Action<TranslationResults> onCompleted);
    }

    public sealed class TranslationResults
    {
        private readonly Dictionary<string, string> _results;

        public TranslationResults(int token, ulong id, string defaultLanguageType, string text)
        {
            Token = token;
            Id = id;
            DefaultLanguageType = defaultLanguageType;
            Text = text;
            _results = new Dictionary<string, string>();
            _results[defaultLanguageType] = text;
        }

        /// <summary>
        /// 唯一令牌
        /// </summary>
        public int Token { get; }

        /// <summary>
        /// 文本id
        /// </summary>
        public ulong Id { get; }

        /// <summary>
        /// 默认语种
        /// </summary>
        public string DefaultLanguageType { get; }

        /// <summary>
        /// 待翻译的原始文本
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 添加翻译结果
        /// </summary>
        /// <param name="languageType">目标语言（注意不能是默认语言）</param>
        /// <param name="text">目标语言的翻译结果</param>
        public void AddTranslationResult(string languageType, string text)
        {
            if (languageType != DefaultLanguageType) _results[languageType] = text;
        }

        /// <summary>
        /// 获取目标语言的翻译结果
        /// </summary>
        /// <param name="languageType">目标语言类型</param>
        /// <returns>翻译结果</returns>
        public string GetTranslationResult(string languageType)
        {
            return _results.GetValueOrDefault(languageType, DefaultLanguageType);
        }
    }
}