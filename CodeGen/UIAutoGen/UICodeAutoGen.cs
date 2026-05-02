using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
using UniVue.Editor;
using UniVue.UI;
using Object = UnityEngine.Object;

namespace UniVue.CodeGen
{
    internal sealed class UICodeAutoGen : AssetPostprocessor
    {
        private const string RegionStart = "#region UniVue Auto-Generated \u2014 DO NOT MODIFY";
        private const string RegionEnd = "#endregion // UniVue Auto-Generated";

        private static List<UICodeGenRule> _rules;

        private static void OnPostprocessAllAssets(
            string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            ManifestData manifest = LoadManifest();
            bool dirty = false;

            foreach (string path in importedAssets)
            {
                if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
                dirty |= ProcessPrefab(path, manifest);
                dirty |= HandleImportedPrefabCodegenRootRemoved(path, manifest);
            }

            foreach (string path in deletedAssets)
            {
                if (!path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
                dirty |= HandleDeletedPrefab(path, manifest);
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                if (!movedAssets[i].EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) continue;
                PrefabRecord record = manifest.records.Find(r => r.prefabPath == movedFromAssetPaths[i]);
                if (record != null)
                {
                    record.prefabPath = movedAssets[i];
                    dirty = true;
                }
            }

            if (dirty) SaveManifest(manifest);
        }

        [MenuItem("UniVue/Code Gen/Generate All UI Code")]
        public static void ForceGenerateAll()
        {
            string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
            ManifestData manifest = LoadManifest();

            foreach (PrefabRecord record in manifest.records)
                StripAutoGenRegionFromFile(record.scriptPath);

            manifest.records.Clear();
            bool dirty = false;

            foreach (string guid in allPrefabs)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                dirty |= ProcessPrefab(path, manifest);
            }

            if (dirty) SaveManifest(manifest);
            AssetDatabase.Refresh();
        }

        // priority 越小越靠上；与校验项使用相同数值（Unity 要求一致）
        [MenuItem("Assets/UniVue/CodeGen/Generate UI Code", false, -50000)]
        public static void GenerateUICodeForSelectedPrefab()
        {
            ManifestData manifest = LoadManifest();
            bool dirty = false;
            foreach (Object obj in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(obj);
                if (!PrefabAssetIsCodegenTarget(path)) continue;
                dirty |= ProcessPrefab(path, manifest, true);
            }

            if (dirty) SaveManifest(manifest);
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/UniVue/CodeGen/Generate UI Code", true, -50000)]
        public static bool ValidateGenerateUICodeForSelectedPrefab()
        {
            if (Selection.objects.Length != 1) return false;
            return PrefabAssetIsCodegenTarget(AssetDatabase.GetAssetPath(Selection.activeObject));
        }

        /// <summary>与菜单 <c>UniVue/Code Gen/Generate All UI Code</c> 相同，在 Project 资源右键菜单中提供入口。</summary>
        [MenuItem("Assets/UniVue/CodeGen/Generate All UI Code", false, -49999)]
        public static void RegenerateAllUICodeFromAssetsMenu()
        {
            ForceGenerateAll();
        }

        [MenuItem("Assets/UniVue/CodeGen/Generate All UI Code", true, -49999)]
        public static bool ValidateRegenerateAllUICodeFromAssetsMenu()
        {
            return true;
        }

#region Rule Discovery

        private static List<UICodeGenRule> GetRules()
        {
            if (_rules != null) return _rules;

            TypeCache.TypeCollection allTypes = TypeCache.GetTypesDerivedFrom<UICodeGenRule>();
            List<Type> concreteTypes = new();
            foreach (Type t in allTypes)
            {
                if (!t.IsAbstract)
                    concreteTypes.Add(t);
            }

            _rules = new List<UICodeGenRule>();
            foreach (Type t in concreteTypes)
            {
                bool isLeaf = true;
                foreach (Type other in concreteTypes)
                {
                    if (other != t && t.IsAssignableFrom(other))
                    {
                        isLeaf = false;
                        break;
                    }
                }

                if (isLeaf)
                    _rules.Add((UICodeGenRule)Activator.CreateInstance(t));
            }

            _rules.Sort();
            return _rules;
        }

#endregion

#region Prefab Processing

        /// <summary>
        /// 是否为 Project 中的预制体资源，且根节点上的 <see cref="BaseUI" /> 被至少一条代码生成规则接受（如 <see cref="BaseView" /> / <see cref="BaseComponent" />
        /// ）。
        /// </summary>
        private static bool PrefabAssetIsCodegenTarget(string prefabAssetPath)
        {
            if (string.IsNullOrEmpty(prefabAssetPath)) return false;
            if (!prefabAssetPath.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)) return false;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
            if (!prefab || !prefab.TryGetComponent(out BaseUI rootUI)) return false;

            foreach (UICodeGenRule rule in GetRules())
            {
                if (rule.InvokeFilter(prefab, rootUI)) return true;
            }

            return false;
        }

        /// <summary>
        /// 将 Unity 资产路径（相对项目根，如 <c>Assets/...</c>、<c>Packages/...</c>）转为绝对磁盘路径。
        /// 不可用 <see cref="Path.GetFullPath(string)" /> 直接作用于资产路径——其相对于进程当前目录，常与项目根不一致。
        /// </summary>
        private static bool TryGetAbsolutePath(string assetRelativePath, out string absolutePath)
        {
            absolutePath = null;
            if (string.IsNullOrEmpty(assetRelativePath)) return false;

            if (Path.IsPathRooted(assetRelativePath))
            {
                absolutePath = Path.GetFullPath(assetRelativePath);
                return File.Exists(absolutePath);
            }

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrEmpty(projectRoot)) return false;

            string rel = assetRelativePath.Replace('/', Path.DirectorySeparatorChar);
            absolutePath = Path.GetFullPath(Path.Combine(projectRoot, rel));
            return File.Exists(absolutePath);
        }

        private static bool ProcessPrefab(string prefabPath, ManifestData manifest, bool forceRegenerate = false)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (!prefab || !prefab.TryGetComponent(out BaseUI rootUI)) return false;

            List<UICodeGenRule> rules = GetRules();
            Type rootType = rootUI.GetType();

            HashSet<GeneratedProperty> properties = new();
            bool anyAccepted = false;

            foreach (UICodeGenRule rule in rules)
            {
                if (!rule.InvokeFilter(prefab, rootUI)) continue;
                anyAccepted = true;
                rule.InvokeTryGenProperties(rootType, prefab, properties);
            }

            if (!anyAccepted) return false;

            string newHash = ComputeHash(properties);
            PrefabRecord record = manifest.records.Find(r => r.prefabPath == prefabPath);
            if (!forceRegenerate && record != null && record.fieldsHash == newHash)
                return false;

            MonoScript monoScript = MonoScript.FromMonoBehaviour(rootUI);
            if (!monoScript) return false;
            string scriptAssetPath = AssetDatabase.GetAssetPath(monoScript);
            if (string.IsNullOrEmpty(scriptAssetPath)) return false;

            if (!TryGetAbsolutePath(scriptAssetPath, out string fullPath)) return false;

            if (record != null && record.scriptPath != scriptAssetPath)
                StripAutoGenRegionFromFile(record.scriptPath);

            string content = File.ReadAllText(fullPath, Encoding.UTF8);
            content = StripAutoGenRegion(content);

            if (properties.Count > 0)
            {
                string code = BuildPartialClass(rootType.Namespace, rootType.Name, properties);
                content = content.TrimEnd() + "\n\n" + code + "\n";
            }

            File.WriteAllText(fullPath, content, Encoding.UTF8);

            if (record != null)
            {
                record.scriptPath = scriptAssetPath;
                record.typeFullName = rootType.FullName;
                record.fieldsHash = newHash;
            }
            else
            {
                manifest.records.Add(new PrefabRecord
                {
                    prefabPath = prefabPath,
                    scriptPath = scriptAssetPath,
                    typeFullName = rootType.FullName,
                    fieldsHash = newHash
                });
            }

            return true;
        }

        private static bool HandleDeletedPrefab(string prefabPath, ManifestData manifest)
        {
            int idx = manifest.records.FindIndex(r => r.prefabPath == prefabPath);
            if (idx < 0) return false;

            string scriptPath = manifest.records[idx].scriptPath;
            manifest.records.RemoveAt(idx);
            if (!ManifestHasRecordForScriptPath(manifest, scriptPath))
                StripAutoGenRegionFromFile(scriptPath);
            return true;
        }

        /// <summary>
        /// 预制体已导入但根节点不再挂载 <see cref="BaseUI" />（例如移除了 BaseView）时：
        /// 从清单中移除该预制体对应条目；若没有任何预制体仍关联同一脚本，则剥离该脚本中的自动生成区域。
        /// </summary>
        private static bool HandleImportedPrefabCodegenRootRemoved(string prefabPath, ManifestData manifest)
        {
            int idx = manifest.records.FindIndex(r => r.prefabPath == prefabPath);
            if (idx < 0) return false;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (!prefab || prefab.TryGetComponent(out BaseUI _))
                return false;

            string scriptPath = manifest.records[idx].scriptPath;
            manifest.records.RemoveAt(idx);
            if (!ManifestHasRecordForScriptPath(manifest, scriptPath))
                StripAutoGenRegionFromFile(scriptPath);
            return true;
        }

        private static bool ManifestHasRecordForScriptPath(ManifestData manifest, string scriptAssetPath)
        {
            if (string.IsNullOrEmpty(scriptAssetPath)) return false;
            foreach (PrefabRecord r in manifest.records)
            {
                if (string.Equals(r.scriptPath, scriptAssetPath, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

#endregion

#region Code Generation

        private static string BuildPartialClass(string ns, string className, HashSet<GeneratedProperty> properties)
        {
            List<GeneratedProperty> sorted = new(properties);
            sorted.Sort((a, b) => string.Compare(a.path, b.path, StringComparison.Ordinal));

            StringBuilder sb = new();
            sb.AppendLine(RegionStart);

            bool hasNs = !string.IsNullOrEmpty(ns);
            string ci = hasNs ? "    " : "";
            string mi = hasNs ? "        " : "    ";

            if (hasNs)
            {
                sb.AppendLine($"namespace {ns}");
                sb.AppendLine("{");
            }

            sb.AppendLine($"{ci}partial class {className}");
            sb.AppendLine($"{ci}{{");

            for (int i = 0; i < sorted.Count; i++)
            {
                GeneratedProperty p = sorted[i];
                string lazyPath = FormatLazyInitUiRelativePath(p.path);
                string esc = EscapeCSharpStringLiteral(lazyPath);
                sb.AppendLine($"{mi}[UniVue.UI.LazyInitUI(\"{esc}\")]");
                sb.AppendLine($"{mi}public {p.propertyTypeFullName} {p.propertyName} {{ get; }}");
                if (i < sorted.Count - 1) sb.AppendLine();
            }

            sb.AppendLine($"{ci}}}");
            if (hasNs) sb.AppendLine("}");
            sb.Append(RegionEnd);
            return sb.ToString();
        }

        /// <summary>
        /// 将 <see cref="GeneratedProperty.path" />（RootName/子路径…）转为不含根名的相对路径，以 <c>/</c> 开头，与 <see cref="LazyInitUIAttribute" />
        /// 一致。
        /// </summary>
        private static string FormatLazyInitUiRelativePath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return "/";
            int slash = fullPath.IndexOf('/');
            if (slash < 0) return "/" + fullPath;
            return fullPath.Substring(slash);
        }

        private static string EscapeCSharpStringLiteral(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\", StringComparison.Ordinal)
                    .Replace("\"", "\\\"", StringComparison.Ordinal)
                    .Replace("\r", "\\r", StringComparison.Ordinal)
                    .Replace("\n", "\\n", StringComparison.Ordinal);
        }

        private static void StripAutoGenRegionFromFile(string scriptAssetPath)
        {
            if (string.IsNullOrEmpty(scriptAssetPath)) return;
            if (!TryGetAbsolutePath(scriptAssetPath, out string fullPath)) return;
            string content = File.ReadAllText(fullPath, Encoding.UTF8);
            string stripped = StripAutoGenRegion(content);
            if (stripped != content)
                File.WriteAllText(fullPath, stripped.TrimEnd() + "\n", Encoding.UTF8);
        }

        private static string StripAutoGenRegion(string content)
        {
            int start = content.IndexOf(RegionStart, StringComparison.Ordinal);
            if (start < 0) return content;
            int end = content.IndexOf(RegionEnd, start, StringComparison.Ordinal);
            string before = content.Substring(0, start).TrimEnd();
            if (end < 0) return before;
            end += RegionEnd.Length;
            while (end < content.Length && (content[end] == '\r' || content[end] == '\n')) end++;
            string after = content.Substring(end).TrimEnd();
            return string.IsNullOrEmpty(after) ? before : before + "\n\n" + after;
        }

#endregion

#region Manifest

        private static ManifestData LoadManifest()
        {
            string json = UICodeAutoGenManifest.instance.manifestJson;
            if (string.IsNullOrEmpty(json)) return new ManifestData();
            try { return JsonUtility.FromJson<ManifestData>(json) ?? new ManifestData(); }
            catch { return new ManifestData(); }
        }

        private static void SaveManifest(ManifestData data)
        {
            UICodeAutoGenManifest.instance.manifestJson = JsonUtility.ToJson(data, false);
            UICodeAutoGenManifest.instance.Save();
        }

        private static string ComputeHash(HashSet<GeneratedProperty> properties)
        {
            List<GeneratedProperty> sorted = new(properties);
            sorted.Sort((a, b) => string.Compare(a.path, b.path, StringComparison.Ordinal));

            StringBuilder sb = new();
            foreach (GeneratedProperty p in sorted)
            {
                sb.Append(p.propertyTypeFullName).Append(':').Append(p.propertyName).Append(':').Append(p.path)
                  .Append(';');
            }

            using MD5 md5 = MD5.Create();
            return Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())));
        }

        [Serializable]
        private class ManifestData
        {
            public List<PrefabRecord> records = new();
        }

        [Serializable]
        private class PrefabRecord
        {
            public string prefabPath;
            public string scriptPath;
            public string typeFullName;
            public string fieldsHash;
        }

#endregion
    }
}