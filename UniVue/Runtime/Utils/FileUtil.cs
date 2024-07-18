using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UniVue.Utils
{
    public static class FileUtil
    {
        /// <summary>
        /// 读取属性文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="encoding">编码格式，如果为null则默认使用UTF-8格式</param>
        /// <returns>Dictionary<string, string></returns>
        public static Dictionary<string, string> ReadPropertyFile(string filePath, Encoding encoding = null)
        {
            encoding = encoding == null ? Encoding.UTF8 : encoding;
            Dictionary<string, string> contents = new Dictionary<string, string>();
            using (StreamReader reader = new StreamReader(filePath, encoding))
            {
                while (!reader.EndOfStream)
                {
                    string line;
                    while (!string.IsNullOrEmpty(line = reader.ReadLine()) && !line.StartsWith("#"))
                    {
                        string[] strings = line.Split('=');
                        contents.TryAdd(strings[0], strings[1]);
                    }
                }
            }
            return contents;
        }
    }
}
