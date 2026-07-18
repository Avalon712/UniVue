#region

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

#endregion

namespace UniVue.I18n
{
    public interface II10NFileReader : IDisposable
    {
        /// <summary>
        /// 读取指定的语言文本集
        /// </summary>
        /// <param name="languageType">语言类型</param>
        /// <param name="token">取消令牌</param>
        /// <returns>成功返回文本表；失败返回 null</returns>
        Task<IReadOnlyDictionary<long, string>> ReadFileAsync(string languageType, CancellationToken token = default);
    }
}