using System;
using System.Text.RegularExpressions;
using UniVue.Common;
using UniVue.i18n;

namespace UniVue.ViewModel
{
    /// <summary>
    /// 文本模板
    /// <para>模板中的所有字符'$'将被替换为属性值</para>
    /// </summary>
    /// <remarks>
    /// UI组件(TMP_Text)的初始文本的内容中使用&lt;template&gt;标签可以指定模板，同时可以指定不同语言环境下使用不同的模板，下面举例标签的使用
    /// <para>&lt;template lang=zh_CN&gt;名称: $&lt;/template&gt; : 标识当前语言环境为zh_CN(简体中文)时使用模板"名称: $"</para>
    /// <para>&lt;template&gt;名称: $&lt;/template&gt; : 当前没有任何语言环境(Language.None)，为模板"名称: $"</para>
    /// <para>注意，&lt;template&gt;标签中的lang属性必须能够被解析为枚举<see cref="i18n.Language"/>对应的一个值</para>
    /// </remarks>
    public readonly struct TextTemplate
    {
        //.*?  ---> 非贪婪匹配
        private static readonly Regex TEMPLATE = new Regex(@"<template(\slang=(\w+))?>(.*?)</template>", RegexOptions.Compiled);

        /// <summary>
        /// 模板中要替换的符号
        /// </summary>
        public const string SYMBOL = "$";

        public readonly Language language;
        public readonly string template;

        public TextTemplate(Language language, string template)
        {
            this.language = language;
            this.template = template;
        }

        public override bool Equals(object obj)
        {
            return obj is TextTemplate template &&
                   language == template.language &&
                   this.template == template.template;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(language, template);
        }

        public override string ToString()
        {
            return $"TextTemplate{{Language={language} Template={template}}}";
        }

        /// <summary>
        /// 尝试将指定输入文本中的所有的&lt;template&gt;标签进行解析
        /// </summary>
        /// <param name="input">&lt;template&gt;标签</param>
        /// <returns>true-解析成功</returns>
        public static bool TryParse(string input, out TextTemplate[] templates)
        {
            templates = null;
            MatchCollection matches = TEMPLATE.Matches(input);
            if (matches.Count > 0)
            {
                templates = new TextTemplate[matches.Count];
            }
            for (int i = 0; i < matches.Count; i++)
            {
                Match match = matches[i];
                if (match.Success)
                {
                    GroupCollection groups = match.Groups;
                    string lang = groups[2].Value;
                    if (string.IsNullOrEmpty(lang))
                        templates[i] = new TextTemplate(Language.None, groups[3].Value);
                    else if (Lang.TryParse(lang, out Language language))
                        templates[i] = new TextTemplate(language, groups[3].Value);
                    else
                        ThrowUtil.ThrowWarn($"未能将{lang}正确解析为UniVue.i18n.Language的枚举值");
                }
            }
            return templates != null;
        }

        /// <summary>
        /// 获取当前语言环境下的模板
        /// </summary>
        public static TextTemplate CurrentLanguageEnvironmentTemplate(TextTemplate[] templates)
        {
            for (int i = 0; i < templates.Length; i++)
            {
                TextTemplate template = templates[i];
                if (template.language == Vue.language) return template;
            }
            return templates[0];
        }
    }

}
