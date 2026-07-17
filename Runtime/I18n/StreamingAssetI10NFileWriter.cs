#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UniVue.I18n
{
    /// <summary>
    /// 从Unity的StreamingAssets目录下写入二进制本地化语言文件，尽在Editor模式下可用
    /// </summary>
    public sealed class StreamingAssetI10NFileWriter : II10NFileWriter
    {
        private const string FileExtensionName = "bytes";
        private readonly Dictionary<string, BinaryWriter> _writers = new();
        private readonly string _relativeDirectory;
        private bool _disposed;
        private readonly Dictionary<BinaryWriter, int> _indexes = new();
        
        public StreamingAssetI10NFileWriter():this(string.Empty){}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relativeDirectory">相对<see cref="Application.streamingAssetsPath"/>的子目录，此目录存放所有的本地化语言二进制文本</param>
        public StreamingAssetI10NFileWriter(string relativeDirectory)
        {
            _relativeDirectory = relativeDirectory;
        }
        
        private string GetPath(string languageType)
        {
            string fileName = $"{languageType}.{FileExtensionName}";
            string filePath = string.IsNullOrEmpty(_relativeDirectory) ? fileName : Path.Combine(_relativeDirectory, fileName);
            return Path.Combine(Application.streamingAssetsPath, filePath);
        }
        
        public long Write(string text, string languageType)
        {
            if (!_writers.TryGetValue(languageType, out BinaryWriter writer))
            {
                string path = GetPath(languageType);
                writer = new BinaryWriter(File.Exists(path) ? File.OpenWrite(path) : File.Create(path));
                _writers.Add(languageType, writer);
                _indexes.Add(writer, 1);
            }
            int index = _indexes[writer];
            writer.Write(text);
            _indexes[writer] = index + 1;
            return index;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            foreach (BinaryWriter writer in _writers.Values)
            {
                writer.Dispose();
            }
            _writers.Clear();
        }
    }
}

#endif