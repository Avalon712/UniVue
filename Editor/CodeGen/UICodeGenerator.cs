#region

using System;
using System.IO;
using UnityEngine;
using UniVue.UI;

#endregion

namespace UniVue.Editor.CodeGen
{
    public abstract class UICodeGenerator
    {
        public string PrefabPath { get; internal set; }

        public UIGeneratedFile File { get; private set; }

        /// <summary>
        /// 文件扩展名称，默认为 .g.cs
        /// </summary>
        public virtual string FileExtension => ".g.cs";

        /// <summary>
        /// 预制体身上挂载的BaseUI脚本
        /// </summary>
        public BaseUI Script { get; private set; }

        internal void ExportInternal(GameObject uiPrefab, string sourceFilePath, Type sourceType, string prefabPath)
        {
            Script = uiPrefab.GetComponent<BaseUI>();
            UIGeneratedFile file = new();
            if (!Script)
            {
                Debug.LogWarning(
                    $"The UI prefab({prefabPath}) does not have a BaseUI component attached. Code generation will be skipped.",
                    uiPrefab);
                return;
            }

            File = file;
            file.AssetPath = prefabPath;
            file.SourceType = sourceType;
            file.SourceFilePath = sourceFilePath;
            file.FileName = GetGeneratedFileNameWithoutExtension(Path.GetFileNameWithoutExtension(sourceFilePath));
            file.FilePath = GetGeneratedFileAbsolutPath(sourceFilePath, $"{file.FileName}{FileExtension}");
            OnFileInitialized(file);
            TraverseUIHierarchy(uiPrefab, uiPrefab.name);
            Generate(File);
        }

        private void TraverseUIHierarchy(GameObject ui, string uiPath)
        {
            if (!IfIgnoreCurrent(ui, uiPath)) OnVisitUIHierarchy(ui, uiPath);

            if (IfSkipDescendant(ui, uiPath)) return;

            foreach (Transform child in ui.transform)
            {
                string childPath = $"{uiPath}/{child.name}";
                TraverseUIHierarchy(child.gameObject, childPath);
            }
        }

        /// <summary>
        /// 获取生成的代码文件名（不包含扩展名）
        /// </summary>
        /// <param name="sourceFileName">原始文件的名称（不含扩展名称）</param>
        /// <returns></returns>
        protected virtual string GetGeneratedFileNameWithoutExtension(string sourceFileName)
        {
            return sourceFileName;
        }

        /// <summary>
        /// 获取生成的代码文件路径
        /// </summary>
        /// <param name="sourceFilePath">源文件的绝对路径</param>
        /// <param name="generatedFileNameWithExtension">生成的文件的名称（含扩展名）</param>
        /// <returns>生成的文件的绝对路径</returns>
        protected virtual string GetGeneratedFileAbsolutPath(string sourceFilePath,
            string generatedFileNameWithExtension)
        {
            return Path.Combine(Path.GetDirectoryName(sourceFilePath) ?? string.Empty, generatedFileNameWithExtension);
        }

        /// <summary>
        /// 是否跳过当前节点的所有后继节点
        /// <remarks>在执行导出逻辑时，预制体的每个GameObject都会被按照深度优先的方式进行递归，已决定为当前UI类自动导出生成哪些UI对象</remarks>
        /// </summary>
        /// <param name="ui">当前被检查的UI对象</param>
        /// <param name="uiPath">从预制体节点到当前节点的路径</param>
        /// <returns>true-当前节点的所有后继节点都不会再被遍历</returns>
        protected virtual bool IfSkipDescendant(GameObject ui, string uiPath)
        {
            return false;
        }

        /// <summary>
        /// 是否忽略对当前节点的导出逻辑（其后代节点仍然会被执行导出逻辑）
        /// </summary>
        /// <param name="ui">当前被检查的UI对象</param>
        /// <param name="uiPath">从预制体节点到当前节点的路径</param>
        /// <returns>true-当前节点不会被执行导出逻辑</returns>
        protected virtual bool IfIgnoreCurrent(GameObject ui, string uiPath)
        {
            return false;
        }

        /// <summary>
        /// 生成文件信息初始化完成后调用
        /// </summary>
        /// <param name="file"></param>
        protected abstract void OnFileInitialized(UIGeneratedFile file);

        /// <summary>
        /// 为当前节点生成导出属性
        /// </summary>
        /// <param name="ui">当前被检查的UI对象</param>
        /// <param name="uiPath">从预制体节点到当前节点的路径</param>
        protected abstract void OnVisitUIHierarchy(GameObject ui, string uiPath);

        /// <summary>
        /// 生成完整的源代码
        /// </summary>
        /// <param name="file">生成的文件信息</param>
        /// <returns>完整的文件内容</returns>
        protected abstract void Generate(UIGeneratedFile file);
    }

    public sealed class UIGeneratedFile
    {
        /// <summary>
        /// 自定义的额外数据
        /// </summary>
        public object customExtraData;

        /// <summary>
        /// 预制体路径
        /// </summary>
        public string AssetPath { get; internal set; }

        /// <summary>
        /// 生成的文件名称（不包含扩展名）
        /// </summary>
        public string FileName { get; internal set; }

        /// <summary>
        /// 生成的文件的绝对路径
        /// </summary>
        public string FilePath { get; internal set; }

        /// <summary>
        /// 源文件类型
        /// </summary>
        public Type SourceType { get; internal set; }

        /// <summary>
        /// 源文件路径
        /// </summary>
        public string SourceFilePath { get; internal set; }
    }
}