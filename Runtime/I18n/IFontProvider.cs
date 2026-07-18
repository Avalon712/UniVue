#region

using System.Threading;
using System.Threading.Tasks;
using TMPro;

#endregion

namespace UniVue.I18n
{
    /// <summary>
    /// 字体通常都比较大，不建议使用 ScriptableObject 引用导致游戏启动就加载所有被引用的字体资源，
    /// 占用大量内存。更高效的做法应该是按需加载，只加载当前语言环境的字体。
    /// </summary>
    public interface IFontProvider
    {
        /// <summary>
        /// 加载指定语言的字体
        /// </summary>
        /// <param name="languageType">语言类型</param>
        /// <param name="token">取消令牌</param>
        /// <returns>成功返回字体；未找到或失败返回 null</returns>
        Task<TMP_FontAsset> GetFontAsync(string languageType, CancellationToken token = default);
    }
}