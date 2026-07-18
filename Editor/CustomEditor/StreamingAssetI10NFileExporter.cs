#region

using System.IO;
using UnityEngine;

#endregion

namespace UniVue.Editor
{
    /// <summary>
    /// 从Unity的StreamingAssets目录下写入二进制本地化语言文件，尽在Editor模式下可用
    /// </summary>
    public sealed class StreamingAssetI10NFileExporter : II10NFileExporter
    {
        private const string FileExtensionName = "bytes";
        private readonly string _relativeDirectory;
        private bool _disposed;

        public StreamingAssetI10NFileExporter() : this(string.Empty)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="relativeDirectory">相对<see cref="Application.streamingAssetsPath" />的子目录，此目录存放所有的本地化语言二进制文本</param>
        public StreamingAssetI10NFileExporter(string relativeDirectory)
        {
            _relativeDirectory = relativeDirectory;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }

        public void Export(LanguageTable table)
        {
            int totalCount = table.languages.Count;
            for (int i = 0; i < table.languageTypes.Count; i++)
            {
                string languageType = table.languageTypes[i];
                string fullPath = GetPath(languageType);
                using BinaryWriter writer =
                    new(File.Exists(fullPath) ? File.OpenWrite(fullPath) : File.Create(fullPath));
                writer.Write(totalCount); //写入总条数 -> 防止扩容
                foreach (Languages language in table.languages)
                {
                    writer.Write(language.id);
                    writer.Write(language.languages[i]);
                }
            }
        }

        private string GetPath(string languageType)
        {
            string fileName = $"{languageType}.{FileExtensionName}";
            string filePath = string.IsNullOrEmpty(_relativeDirectory)
                ? fileName
                : Path.Combine(_relativeDirectory, fileName);
            string fullPath = Path.Combine(Application.streamingAssetsPath, filePath);
            string directoryName = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
                Directory.CreateDirectory(directoryName);
            return fullPath;
        }
    }
}