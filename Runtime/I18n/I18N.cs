#region

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UniVue.Common;
using UniVue.Event;
using UniVue.Utils;
using Object = UnityEngine.Object;

#endregion

namespace UniVue.I18n
{
    public ref struct I18NInitParameters
    {
        public IFontProvider FontProvider { get; set; }
        public II10NFileReader Reader { get; set; }
        public List<string> SupportedLanguages { get; set; }
        public string DefaultLanguage { get; set; }
    }

    public static class I18N
    {
        /// <summary>
        /// 语言环境切换完成时调用，无参
        /// </summary>
        public const string EVENT_LANGUAGE_CHANGED_COMPLETED = nameof(EVENT_LANGUAGE_CHANGED_COMPLETED);

        private static bool _initialized;
        private static IReadOnlyDictionary<long, string> _textMap;
        private static CancellationTokenSource _cts;

        /// <summary>
        /// 当前的语言环境
        /// </summary>
        public static string Language { get; private set; }

        public static string DefaultLanguage { get; private set; }

        public static II10NFileReader Reader { get; private set; }

        public static IFontProvider FontProvider { get; private set; }

        public static IReadOnlyList<string> SupportedLanguages { get; private set; }

        /// <summary>
        /// 当前语言环境使用的字体
        /// </summary>
        public static TMP_FontAsset Font { get; private set; }

        /// <summary>
        /// 初始化多语言环境
        /// </summary>
        /// <param name="parameters">初始化参数</param>
        /// <param name="openDebugUI">是否在运行时打开调试器UI（仅在编辑器模式下有效）</param>
        public static void Initialize(in I18NInitParameters parameters, bool openDebugUI = true)
        {
            if (_initialized) return;
            ExceptionUtils.ThrowIfArgNull(parameters.Reader, nameof(parameters.Reader));
            ExceptionUtils.ThrowIfArgNull(parameters.SupportedLanguages, nameof(parameters.SupportedLanguages));
            _initialized = true;
            Reader = parameters.Reader;
            FontProvider = parameters.FontProvider;
            DefaultLanguage = parameters.DefaultLanguage;
            SupportedLanguages = parameters.SupportedLanguages;
            _ = SwitchLanguageAsync(DefaultLanguage);

#if UNITY_EDITOR
            if (openDebugUI)
            {
                GameObject go = new("I18N DebugUI", typeof(I18NSwitcherUI));
                Object.DontDestroyOnLoad(go);
            }
#endif
        }

        /// <summary>
        /// 切换语言环境（fire-and-forget，内部走 <see cref="SwitchLanguageAsync" />）
        /// </summary>
        public static void SwitchLanguage(string language, AsyncResultCallback<string> completed = null)
        {
            _ = SwitchLanguageAsync(language, completed);
        }

        /// <summary>
        /// 异步切换语言环境。多次调用采用 last-win：仅最后一次请求会写入状态并视为成功。
        /// </summary>
        /// <param name="language">目标语言</param>
        /// <param name="completed">完成回调：(是否成功, 当前/目标语言)</param>
        /// <returns>是否成功应用为当前语言</returns>
        public static async Task<bool> SwitchLanguageAsync(string language,
            AsyncResultCallback<string> completed = null)
        {
            if (Reader == null || string.IsNullOrEmpty(language))
            {
                completed?.Invoke(false, Language);
                return false;
            }

            if (language == Language && _textMap != null)
            {
                completed?.Invoke(true, Language);
                return true;
            }

            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            CancellationTokenSource cts = new();
            _cts = cts;
            CancellationToken token = cts.Token;

            try
            {
                Task<IReadOnlyDictionary<long, string>> readTask = Reader.ReadFileAsync(language, token);
                Task<TMP_FontAsset> fontTask = FontProvider != null
                    ? FontProvider.GetFontAsync(language, token)
                    : Task.FromResult<TMP_FontAsset>(null);

                await Task.WhenAll(readTask, fontTask);

                // last-win：已被更新的请求取代，则丢弃结果
                if (_cts != cts)
                {
                    completed?.Invoke(false, language);
                    return false;
                }

                IReadOnlyDictionary<long, string> textMap = readTask.Result;
                if (textMap == null)
                {
                    completed?.Invoke(false, language);
                    return false;
                }

                _textMap = textMap;
                Language = language;
                Font = fontTask.Result;

                _cts = null;
                cts.Dispose();

                EventMgr.Dispatch(EVENT_LANGUAGE_CHANGED_COMPLETED);
                completed?.Invoke(true, Language);
                return true;
            }
            catch (OperationCanceledException)
            {
                if (_cts == cts)
                {
                    _cts = null;
                    cts.Dispose();
                }

                completed?.Invoke(false, language);
                return false;
            }
            catch (Exception e)
            {
                if (_cts == cts)
                {
                    _cts = null;
                    cts.Dispose();
                }

                LogUtil.Exception(e);
                completed?.Invoke(false, language);
                return false;
            }
        }

        /// <summary>
        /// 获取当前语言环境下的文本
        /// </summary>
        public static string LocalText(long id)
        {
            if (_textMap == null)
            {
                LogUtil.Warn($"未加载当前语言环境{Language}下的文本集!");
                return string.Empty;
            }

            if (_textMap.TryGetValue(id, out string text)) return text;

            LogUtil.Warn($"当前语言({Language})的文本集中不存在id={id}的文本!");
            return string.Empty;
        }

        /// <summary>
        /// 获取当前语言环境下的文本
        /// </summary>
        public static string LocalText(string text)
        {
            return LocalText(CoreUtils.GenerateUniqueId(text));
        }

        /// <summary>
        /// 获取文本唯一Id
        /// </summary>
        public static long LocalTextId(string text)
        {
            return CoreUtils.GenerateUniqueId(text);
        }
    }
}