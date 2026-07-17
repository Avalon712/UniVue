using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UniVue.Common;
using UniVue.Utils;
using VC;

namespace UniVue.I18n
{
    /// <summary>
    /// 从Unity的StreamingAssets目录下读取二进制本地化语言文件
    /// </summary>
    public sealed class StreamingAssetsI10NFileReader : II10NFileReader
    {
        private const string FileExtensionName = "bytes";
        private string _languageType;
        private readonly string _relativeDirectory;
        private bool _isLoading;
        private List<string> _texts;
        private bool _disposed;
        private CoroutineID _loadingTask;
        private Action<bool> _completed;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="defaultLanguage">默认语言</param>
        /// <param name="relativeDirectory">相对<see cref="Application.streamingAssetsPath"/>的子目录，此目录存放所有的本地化语言二进制文本</param>
        public StreamingAssetsI10NFileReader(string defaultLanguage, string relativeDirectory)
        {
            _relativeDirectory = relativeDirectory;
            if (!string.IsNullOrEmpty(defaultLanguage))
            {
                _isLoading = true;
                _loadingTask = VCoroutine.Run(LoadI10NFile(defaultLanguage));
            }
        }
        
        private string GetURL(string languageType)
        {
            string fileName = $"{languageType}.{FileExtensionName}";
            string filePath = string.IsNullOrEmpty(_relativeDirectory) ? fileName : Path.Combine(_relativeDirectory, fileName);
            return CoreUtils.GetStreamingAssetsURL(filePath);
        }

        private IEnumerator LoadI10NFile(string languageType)
        {
            if(_languageType == languageType || _disposed)
            {
                _completed?.Invoke(false);
                yield break;
            } 
            using UnityWebRequest req = UnityWebRequest.Get(GetURL(languageType));
            yield return req.SendWebRequest();
            
            if (req.result == UnityWebRequest.Result.Success)
            {
                using BinaryReader reader = new BinaryReader(new MemoryStream(req.downloadHandler.data));
                if (_texts == null)
                {
                    _texts = new List<string>(Mathf.NextPowerOfTwo((int)(reader.BaseStream.Length / 64L)));
                }
                _texts.Clear();
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    _texts.Add(reader.ReadString());
                }
                _languageType = languageType;
                _completed?.Invoke(true);
            }
            else
            {
                LogUtil.Error($"Failed to load {languageType} localization file: {req.error}");
                _completed?.Invoke(false);
            }
            
            _isLoading = false;
            _loadingTask = 0;
            _completed = null;
        }
        
        public void ReadLanguageFile(string languageType, Action<bool> completed)
        {
            if (_disposed)
            {
                completed?.Invoke(false);
                return;
            }
            if(_languageType == languageType)
            {
                completed?.Invoke(true);
                return;
            }
            if (_isLoading)
            {
                VCoroutine.Kill(_loadingTask);
                _texts?.Clear();
            }
            _isLoading = true;
            _loadingTask = VCoroutine.Run(LoadI10NFile(languageType));
            _completed = completed;
        }

        public string Read(long id, string languageType)
        {
            if(_isLoading || _disposed) return string.Empty;
            
            if (languageType != _languageType)
            {
                VCoroutine.Run(LoadI10NFile(languageType));
                _isLoading = true;
                return string.Empty;
            }
            
            if(id <= 0 || id >= _texts.Count) return string.Empty;
            
            return _texts[(int)id];
        }

        public void Dispose()
        {
            if(_disposed) return;
            _disposed = true;
            _texts.Clear();
        }
    }
}