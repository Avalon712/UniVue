using System;

namespace UniVue.I18n
{
    public interface II10NFileWriter : IDisposable
    {
        /// <summary>
        /// 写入一个指定语言类型的文本
        /// </summary>
        /// <param name="text">文本</param>
        /// <param name="languageType">语言类型</param>
        /// <returns>唯一id</returns>
        public long Write(string text, string languageType);
    }
}