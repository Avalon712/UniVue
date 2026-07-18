#region

using System;

#endregion

namespace UniVue.Editor
{
    public interface II10NFileExporter : IDisposable
    {
        /// <summary>
        /// 导出语言表
        /// </summary>
        /// <param name="table">语言表</param>
        public void Export(LanguageTable table);
    }
}