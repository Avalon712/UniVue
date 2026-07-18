#region

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UniVue.Common;
using UniVue.Utils;

#endregion

namespace UniVue.I18n
{
    /// <summary>
    /// 从 Unity 的 StreamingAssets 目录下读取二进制本地化语言文件
    /// </summary>
    public sealed class StreamingAssetsI10NFileReader : II10NFileReader
    {
        private const string FileExtensionName = "bytes";
        private readonly string _relativeDirectory;
        private string _cachedLanguageType;
        private Dictionary<long, string> _cachedTexts;
        private bool _disposed;

        public StreamingAssetsI10NFileReader()
        {
        }

        /// <param name="relativeDirectory">相对 <see cref="Application.streamingAssetsPath" /> 的子目录</param>
        public StreamingAssetsI10NFileReader(string relativeDirectory)
        {
            _relativeDirectory = relativeDirectory;
        }

        public Task<IReadOnlyDictionary<long, string>> ReadFileAsync(string languageType,
            CancellationToken token = default)
        {
            if (_disposed)
            {
                LogUtil.Error("StreamingAssetsI10NFileReader is disposed!");
                return Task.FromResult<IReadOnlyDictionary<long, string>>(null);
            }

            if (string.IsNullOrEmpty(languageType)) return Task.FromResult<IReadOnlyDictionary<long, string>>(null);

            if (_cachedLanguageType == languageType && _cachedTexts != null)
                return Task.FromResult<IReadOnlyDictionary<long, string>>(_cachedTexts);

            if (token.IsCancellationRequested) return Task.FromCanceled<IReadOnlyDictionary<long, string>>(token);

            UnityWebRequest req = UnityWebRequest.Get(GetURL(languageType));
            UnityWebRequestAsyncOperation operation = req.SendWebRequest();
            TaskCompletionSource<IReadOnlyDictionary<long, string>> tcs = new();

            CancellationTokenRegistration ctr = default;
            if (token.CanBeCanceled)
                ctr = token.Register(() =>
                {
                    req.Abort();
                    tcs.TrySetCanceled(token);
                });

            operation.completed += _ =>
            {
                try
                {
                    ctr.Dispose();

                    if (token.IsCancellationRequested)
                    {
                        tcs.TrySetCanceled(token);
                        return;
                    }

                    if (req.result != UnityWebRequest.Result.Success)
                    {
                        LogUtil.Error($"Failed to load {languageType} localization file: {req.error}");
                        tcs.TrySetResult(null);
                        return;
                    }

                    using BinaryReader reader = new(new MemoryStream(req.downloadHandler.data));
                    int totalCount = reader.ReadInt32();
                    Dictionary<long, string> texts = new(totalCount);
                    for (int i = 0; i < totalCount; i++) texts.Add(reader.ReadInt64(), reader.ReadString());

                    _cachedTexts = texts;
                    _cachedLanguageType = languageType;
                    tcs.TrySetResult(texts);
                }
                finally
                {
                    req.Dispose();
                }
            };

            return tcs.Task;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _cachedTexts = null;
            _cachedLanguageType = null;
        }

        private string GetURL(string languageType)
        {
            string fileName = $"{languageType}.{FileExtensionName}";
            string filePath = string.IsNullOrEmpty(_relativeDirectory)
                ? fileName
                : Path.Combine(_relativeDirectory, fileName);
            return CoreUtils.GetStreamingAssetsURL(filePath);
        }
    }
}