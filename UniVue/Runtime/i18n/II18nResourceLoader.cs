using System.Collections.Generic;
using UnityEngine;
using UniVue.Utils;

namespace UniVue.i18n
{
    public interface II18nResourceLoader
    {
        /// <summary>
        /// 根据id加载指定的精灵图片
        /// </summary>
        /// <param name="languageTag">当前的语言标识</param>
        /// <param name="id">内容ID标识</param>
        /// <returns>Sprite</returns>
        public Sprite LoadSprite(string languageTag, string id);

        /// <summary>
        /// 加载语言文件内容
        /// </summary>
        /// <remarks>内部默认实现方式为读取属性文件</remarks>
        /// <param name="language">语言信息</param>
        /// <returns>Dictionary<string,string></returns>
        public Dictionary<string, string> LoadContents(Language language)
        {
            return FileUtil.ReadPropertyFile(language.Path, language.Encode);
        }
    }
}
