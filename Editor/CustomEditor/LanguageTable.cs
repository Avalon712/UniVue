#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UniVue.Utils;

#endregion

namespace UniVue.Editor
{
    [Serializable]
    public sealed class LanguageTable : IEnumerable<Languages>
    {
        /// <summary>
        /// 所有可用的翻译服务
        /// </summary>
        public static readonly List<ITextTranslator> Translators;

        /// <summary>
        /// 所有可用于导出语言文件的服务
        /// </summary>
        public static readonly List<II10NFileExporter> Exporters;

        private static LanguageTable _table;

        public string defaultLanguage;
        public List<string> languageTypes;
        public List<Languages> languages;

        private HashSet<long> _waitTranslationResults = new();

        static LanguageTable()
        {
            Translators = new List<ITextTranslator>();
            TypeCache.TypeCollection collection = TypeCache.GetTypesDerivedFrom<ITextTranslator>();
            foreach (Type type in collection) Translators.Add((ITextTranslator)Activator.CreateInstance(type));

            Exporters = new List<II10NFileExporter>();
            TypeCache.TypeCollection collection2 = TypeCache.GetTypesDerivedFrom<II10NFileExporter>();
            foreach (Type type2 in collection2) Exporters.Add((II10NFileExporter)Activator.CreateInstance(type2));
        }

        /// <summary>
        /// 语言表件保存位置
        /// </summary>
        public static string FilePath => Path.Combine(Directory, "languages.json");

        public static string Directory => Path.Combine(EditorCoreUtils.GetProjectRootDirectory(), "I18n");

        public IEnumerator<Languages> GetEnumerator()
        {
            return languages.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static LanguageTable GetOrCreate()
        {
            if (_table != null) return _table;

            static LanguageTable CreateNew()
            {
                LanguageTable table = new();
                table.languages = new List<Languages>();
                table.languageTypes = new List<string>();
                table.defaultLanguage = string.Empty;
                return table;
            }

            if (!File.Exists(FilePath))
            {
                string directory = Directory;
                if (!System.IO.Directory.Exists(directory)) System.IO.Directory.CreateDirectory(directory);
                _table = CreateNew();
            }
            else
            {
                _table = JsonUtility.FromJson<LanguageTable>(File.ReadAllText(FilePath)) ?? CreateNew();
            }

            return _table;
        }

        public static void Save()
        {
            if (_table != null) File.WriteAllText(FilePath, JsonUtility.ToJson(_table, true));
        }

        /// <summary>
        /// 判断某个Id是否正在翻译中（正在翻译中的默认文本不允许被进行修改）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsWaitTranslationResult(long id)
        {
            return _waitTranslationResults.Contains(id);
        }

        /// <summary>
        /// 是否存在文本正在等待翻译结果
        /// </summary>
        /// <returns></returns>
        public bool HasAnyTextIsWaitTranslationResult()
        {
            return _waitTranslationResults.Count > 0;
        }

        /// <summary>
        /// 重命名语种
        /// </summary>
        /// <param name="oldLanguageTypeName">旧语种名称</param>
        /// <param name="newLanguageTypeName">新语种名称</param>
        /// <returns>true-重命名成功</returns>
        public bool TryRenameLanguage(string oldLanguageTypeName, string newLanguageTypeName)
        {
            if (HasAnyTextIsWaitTranslationResult())
            {
                Debug.LogWarning("有语言正在等待翻译，不允许进行重命名！");
                return false;
            }

            int index = languageTypes.IndexOf(oldLanguageTypeName);
            if (index < 0 || languageTypes.Contains(newLanguageTypeName)) return false;

            if (oldLanguageTypeName == defaultLanguage) defaultLanguage = newLanguageTypeName;

            languageTypes[index] = newLanguageTypeName;
            return true;
        }

        public string GetText(long id, string language)
        {
            int index = languageTypes.IndexOf(language);
            if (index < 0) return string.Empty;
            Languages lang = languages.Find(x => x.id == id);
            if (lang == null) return string.Empty;
            return lang.languages[index];
        }

        /// <summary>
        /// 设置某个语言的文本
        /// </summary>
        /// <param name="id">唯一Id</param>
        /// <param name="text">文本内容</param>
        /// <param name="language">语种（不能是默认语种）</param>
        public void SetText(long id, string text, string language)
        {
            if (language == defaultLanguage) return;
            Languages lang = languages.Find(x => x.id == id);
            if (lang == null) return;
            int index = languageTypes.IndexOf(language);
            if (index < 0) return;
            lang.languages[index] = text;
        }

        /// <summary>
        /// 翻译默认文本到所有语言
        /// </summary>
        /// <param name="id"></param>
        /// <param name="translator">翻译器</param>
        /// <param name="onCompleted">翻译完成时回调</param>
        public void Translate(long id, ITextTranslator translator, Action<TranslationResults> onCompleted)
        {
            if (translator == null || IsWaitTranslationResult(id)) return;
            Languages lang = languages.Find(x => x.id == id);
            if (lang == null) return;
            int index = languageTypes.IndexOf(defaultLanguage);
            TranslationResults results = new(lang.GetHashCode(), lang.id, defaultLanguage, lang.languages[index]);
            _waitTranslationResults.Add(id);
            translator.Translate(results, r =>
            {
                _waitTranslationResults.Remove(id);
                if (r != results)
                {
                    Debug.LogError("Translate()方法回调函数没有回传正确的翻译结果参数！");
                    onCompleted?.Invoke(r);
                    return;
                }

                Languages lang2 = languages.Find(x => x.id == id);
                //两次检查保证在翻译期间默认文本没有被进行修改
                if (lang2 != null && lang2.GetHashCode() == results.Token)
                    for (int i = 0; i < languageTypes.Count; i++)
                    {
                        string languageType = languageTypes[i];
                        string resultText = results.GetTranslationResult(languageType);
                        if (!string.IsNullOrEmpty(resultText)) lang2.languages[i] = resultText;
                    }

                onCompleted?.Invoke(r);
            });
        }

        /// <summary>
        /// 更新默认文本
        /// </summary>
        /// <param name="oldId">旧的语言文本</param>
        /// <param name="newDefaultText">新的默认文本</param>
        /// <param name="translator">翻译器自动翻译其他语言</param>
        /// <returns>新的id</returns>
        public long UpdateText(long oldId, string newDefaultText, ITextTranslator translator = null)
        {
            if (IsWaitTranslationResult(oldId))
            {
                Debug.LogWarning($"当前文本(id={oldId})正在等待翻译结果，不允许进行更新！");
                return -1;
            }

            int index = languages.FindIndex(x => x.id == oldId);
            if (index < 0) return oldId;
            Languages lang = languages[index];
            long newId = CoreUtils.GenerateUniqueId(newDefaultText);
            Languages langNew = languages.Find(x => x.id == newId);
            if (langNew != null)
            {
                languages.RemoveAt(index);
                return langNew.id;
            }

            int langIndex = languageTypes.IndexOf(defaultLanguage);
            lang.id = newId;
            lang.languages[langIndex] = newDefaultText;
            if (translator != null) Translate(newId, translator, null);
            return lang.id;
        }

        /// <summary>
        /// 添加文本
        /// </summary>
        /// <param name="texts">其他语言的文本（必须包含默认语言环境下的文本, key=languageType, value=该语言下的文本）</param>
        /// <returns>唯一id</returns>
        public long AddText(Dictionary<string, string> texts)
        {
            string defaultText = texts[defaultLanguage];
            long id = CoreUtils.GenerateUniqueId(defaultText);
            Languages lang = languages.Find(x => x.id == id);
            if (lang != null)
            {
                Debug.LogError($"Language table already contains same text: {defaultText}.");
                return -1;
            }

            lang = new Languages();
            lang.id = id;
            lang.languages = new List<string>(languageTypes.Count);
            foreach (string languageType in languageTypes)
                lang.languages.Add(texts.TryGetValue(languageType, out string text) ? text : string.Empty);
            languages.Add(lang);
            languages.Sort(); //按id升序
            return id;
        }

        /// <summary>
        /// 添加新语言
        /// </summary>
        /// <param name="newLanguageType"></param>
        public void AddNewLanguage(string newLanguageType)
        {
            if (languageTypes.Contains(newLanguageType)) return;
            languageTypes.Add(newLanguageType);
            foreach (Languages lang in languages) lang.languages.Add(string.Empty);
            if (string.IsNullOrEmpty(defaultLanguage)) defaultLanguage = newLanguageType;
        }

        /// <summary>
        /// 删除一个语种（同时移除所有文本中该语种对应的列-不允许删除默认语种）
        /// </summary>
        public bool RemoveLanguage(string language)
        {
            if (language == defaultLanguage) return false;

            int index = languageTypes.IndexOf(language);
            if (index < 0) return false;

            languageTypes.RemoveAt(index);
            foreach (Languages lang in languages)
                if (index < lang.languages.Count)
                    lang.languages.RemoveAt(index);

            return true;
        }

        /// <summary>
        /// 删除一条文本
        /// </summary>
        public bool RemoveText(long id)
        {
            if (IsWaitTranslationResult(id))
            {
                Debug.LogWarning($"当前文本(id={id})正在等待翻译结果，不允许删除！");
                return false;
            }

            int index = languages.FindIndex(x => x.id == id);
            if (index < 0) return false;
            languages.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 设置默认语种（必须是已定义的语种）。
        /// 由于每条文本的 id 是由“默认语种文本”经 FNV-1a 生成的，
        /// 切换默认语种后必须使用新默认语种下的文本重新计算所有 id。
        /// </summary>
        /// <returns>旧 id 到新 id 的映射（可用于同步引用了这些 id 的组件）</returns>
        public Dictionary<long, long> SetDefaultLanguage(string language)
        {
            Dictionary<long, long> idRemap = new();
            if (HasAnyTextIsWaitTranslationResult())
            {
                Debug.LogWarning("当前有文本正在等待翻译结果，不允许切换默认文本");
                return idRemap;
            }

            if (!languageTypes.Contains(language) || defaultLanguage == language) return idRemap;

            int index = languageTypes.IndexOf(language);
            HashSet<long> newIds = new();

            foreach (Languages lang in languages)
            {
                string newDefaultText = index < lang.languages.Count ? lang.languages[index] : string.Empty;
                long newId = CoreUtils.GenerateUniqueId(newDefaultText);

                if (!newIds.Add(newId))
                    Debug.LogWarning(
                        $"切换默认语种到 \"{language}\" 时，文本 \"{newDefaultText}\" 生成了重复 id，" +
                        "请确保该语种下不存在空文本或重复文本。");

                if (lang.id != newId) idRemap[lang.id] = newId;
                lang.id = newId;
            }

            defaultLanguage = language;
            languages.Sort(); //按id升序
            return idRemap;
        }
    }

    [Serializable]
    public sealed class Languages : IComparable<Languages>
    {
        public long id;
        public List<string> languages;

        public int CompareTo(Languages obj)
        {
            return id.CompareTo(obj.id);
        }
    }
}