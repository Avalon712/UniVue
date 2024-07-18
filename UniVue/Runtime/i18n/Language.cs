using System;
using System.Text;
using UnityEngine;

namespace UniVue.i18n
{
    [Serializable]
    public sealed class Language
    {
        [SerializeField] private string _tag;
        [SerializeField] private string _path;
        [SerializeField] private EncodeFormat _format;

        /// <summary>
        /// 语言标签
        /// </summary>
        /// <remarks>建议使用国际化标准组织中规定的语言标识，内置有LangTag枚举列举了游戏多语言最常用的18种语言</remarks>
        public string Tag => _tag;

        /// <summary>
        /// 语言文件所在的绝对路径
        /// </summary>
        public string Path => _path;

        public Encoding Encode
        {
            get
            {
                switch (_format)
                {
                    case EncodeFormat.UTF_8:
                        return Encoding.UTF8;
                    case EncodeFormat.UTF_32:
                        return Encoding.UTF32;
                    case EncodeFormat.Unicode:
                        return Encoding.Unicode;
                    case EncodeFormat.ASCII:
                        return Encoding.ASCII;
                }

                throw new Exception("不支持的编码格式");
            }
        }

        public Language(string tag, string path) : this(tag, path, EncodeFormat.UTF_8) { }

        public Language(string tag, string path, EncodeFormat format)
        {
            _format = format;
            _tag = tag;
            _path = path;
        }

    }
}
