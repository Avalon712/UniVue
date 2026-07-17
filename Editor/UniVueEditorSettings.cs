using System.Collections.Generic;
using UnityEditor;
using UniVue.I18n;

namespace UniVue.Editor
{
    [FilePath("ProjectSettings/UniVueEditorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class UniVueEditorSettings : ScriptableSingleton<UniVueEditorSettings>
    {
        /// <summary>
        /// RedPointKey.g.cs存放的目录（必须在Assets目录下）
        /// </summary>
        public string redPointKeyExportDirectory;

        /// <summary>
        /// RedPointKey.g.cs的命名空间
        /// </summary>
        public string redPointKeyNamespace;

        /// <summary>
        /// 多语言配置
        /// </summary>
        public I18NConfig languageConfig;

        public void SaveSettings()
        {
            Save(true);
        }
    }


}