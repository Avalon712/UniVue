using System;

namespace UniVue.I18n
{
    public interface II10NFileReader : IDisposable
    {
        /// <summary>
        /// 加载指定的语言
        /// </summary>
        /// <param name="languageType">语言类型</param>
        /// <param name="completed">读取完成时回调，参数为是否读取成功(true-成功)</param>
        public void ReadLanguageFile(string languageType, Action<bool> completed);
        
        /// <summary>
        /// 根据唯一id读取一个当前语言环境下的文本
        /// </summary>
        /// <param name="id">文本唯一id</param>
        /// <param name="languageType">语言类型</param>
        /// <returns>文本</returns>
        public string Read(long id, string languageType);
    }
}