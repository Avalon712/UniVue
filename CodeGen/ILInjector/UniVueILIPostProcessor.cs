using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Unity.CompilationPipeline.Common.Diagnostics;
using Unity.CompilationPipeline.Common.ILPostProcessing;

namespace UniVue.CodeGen
{
    public sealed class UniVueILIPostProcessor : ILPostProcessor
    {
        private const string UniVueRuntimeAssemblyName = "UniVue.Runtime";

        public override ILPostProcessor GetInstance()
        {
            return this;
        }

        public override bool WillProcess(ICompiledAssembly compiledAssembly)
        {
            return ReferencesUniVueRuntime(compiledAssembly);
        }

        public override ILPostProcessResult Process(ICompiledAssembly compiledAssembly)
        {
            List<DiagnosticMessage> diagnostics = new();
            if (!WillProcess(compiledAssembly))
                return new ILPostProcessResult(compiledAssembly.InMemoryAssembly, diagnostics);

            try
            {
                using MemoryStream peStream = new(compiledAssembly.InMemoryAssembly.PeData);
                using MemoryStream pdbInputStream = HasPdb(compiledAssembly)
                    ? new MemoryStream(compiledAssembly.InMemoryAssembly.PdbData)
                    : null;
                using PostProcessorAssemblyResolver resolver = new(compiledAssembly);

                ReaderParameters readerParameters = new()
                {
                    AssemblyResolver = resolver,
                    ReadingMode = ReadingMode.Immediate
                };

                if (pdbInputStream != null)
                {
                    readerParameters.ReadSymbols = true;
                    readerParameters.SymbolReaderProvider = new PortablePdbReaderProvider();
                    readerParameters.SymbolStream = pdbInputStream;
                }

                AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(peStream, readerParameters);
                bool modified = false;
                modified |= ParamsGCOptimizationInjector.Inject(assemblyDefinition, diagnostics);
                modified |= UILazyInitUIInjector.Inject(assemblyDefinition, diagnostics);
                modified |= NotifyPropertyChangedInjector.Inject(assemblyDefinition);
                if (!modified) return new ILPostProcessResult(compiledAssembly.InMemoryAssembly, diagnostics);

                using MemoryStream peOutputStream = new();
                using MemoryStream pdbOutputStream = pdbInputStream != null ? new MemoryStream() : null;

                WriterParameters writerParameters = new();
                if (pdbOutputStream != null)
                {
                    writerParameters.WriteSymbols = true;
                    writerParameters.SymbolWriterProvider = new PortablePdbWriterProvider();
                    writerParameters.SymbolStream = pdbOutputStream;
                }

                assemblyDefinition.Write(peOutputStream, writerParameters);
                InMemoryAssembly inMemoryAssembly = new(
                                                        peOutputStream.ToArray(),
                                                        pdbOutputStream != null
                                                            ? pdbOutputStream.ToArray()
                                                            : compiledAssembly.InMemoryAssembly.PdbData);

                return new ILPostProcessResult(inMemoryAssembly, diagnostics);
            }
            catch (Exception exception)
            {
                diagnostics.Add(new DiagnosticMessage
                {
                    DiagnosticType = DiagnosticType.Error,
                    MessageData = $"UniVue IL injection failed: {exception}"
                });
                return new ILPostProcessResult(compiledAssembly.InMemoryAssembly, diagnostics);
            }
        }

        private static bool HasPdb(ICompiledAssembly compiledAssembly)
        {
            return compiledAssembly?.InMemoryAssembly.PdbData != null &&
                   compiledAssembly.InMemoryAssembly.PdbData.Length > 0;
        }

        private static bool ReferencesUniVueRuntime(ICompiledAssembly compiledAssembly)
        {
            if (compiledAssembly?.References == null || IgnoreILInjectAssembly.Ignore(compiledAssembly.Name))
                return false;

            foreach (string reference in compiledAssembly.References)
            {
                string assemblyName = Path.GetFileNameWithoutExtension(reference);
                if (string.Equals(assemblyName, UniVueRuntimeAssemblyName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Unity 编译管线传入的 <see cref="ICompiledAssembly.References" /> 未必包含拆分模块（如 <c>UnityEngine.CoreModule</c>）的显式项，
    /// 但解析 <c>MonoBehaviour</c> 等类型时会向 Cecil 请求该程序集。除名称映射外，在所有引用所在目录中按 <c>{Name}.dll</c> 与大小写不敏感回退查找。
    /// </summary>
    internal sealed class PostProcessorAssemblyResolver : DefaultAssemblyResolver
    {
        private readonly Dictionary<string, string> _assemblyPaths = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _referenceDirectories = new(StringComparer.OrdinalIgnoreCase);

        public PostProcessorAssemblyResolver(ICompiledAssembly compiledAssembly)
        {
            foreach (string reference in compiledAssembly.References)
            {
                if (string.IsNullOrEmpty(reference)) continue;

                string assemblyName = Path.GetFileNameWithoutExtension(reference);
                if (!string.IsNullOrEmpty(assemblyName) && !_assemblyPaths.ContainsKey(assemblyName))
                    _assemblyPaths[assemblyName] = reference;

                string searchDirectory = Path.GetDirectoryName(reference);
                if (!string.IsNullOrEmpty(searchDirectory))
                {
                    _referenceDirectories.Add(searchDirectory);
                    AddSearchDirectory(searchDirectory);
                }
            }
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            ReaderParameters readParams = AttachResolver(parameters);

            if (name?.Name != null &&
                _assemblyPaths.TryGetValue(name.Name, out string mappedPath) &&
                File.Exists(mappedPath))
                return AssemblyDefinition.ReadAssembly(mappedPath, readParams);

            if (name?.Name != null)
            {
                foreach (string dir in _referenceDirectories)
                {
                    string candidate = Path.Combine(dir, name.Name + ".dll");
                    if (File.Exists(candidate))
                        return AssemblyDefinition.ReadAssembly(candidate, readParams);
                }
            }

            try
            {
                return base.Resolve(name, readParams);
            }
            catch (AssemblyResolutionException)
            {
                if (name?.Name == null)
                    throw;

                foreach (string dir in _referenceDirectories)
                {
                    try
                    {
                        foreach (string dll in Directory.GetFiles(dir, "*.dll", SearchOption.TopDirectoryOnly))
                        {
                            if (!string.Equals(Path.GetFileNameWithoutExtension(dll), name.Name,
                                               StringComparison.OrdinalIgnoreCase))
                                continue;
                            if (File.Exists(dll))
                                return AssemblyDefinition.ReadAssembly(dll, readParams);
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }

                throw;
            }
        }

        private ReaderParameters AttachResolver(ReaderParameters parameters)
        {
            if (parameters == null)
                return new ReaderParameters { AssemblyResolver = this };

            if (parameters.AssemblyResolver == this)
                return parameters;

            return new ReaderParameters
            {
                AssemblyResolver = this,
                ReadingMode = parameters.ReadingMode,
                ReadSymbols = parameters.ReadSymbols,
                SymbolReaderProvider = parameters.SymbolReaderProvider,
                SymbolStream = parameters.SymbolStream
            };
        }
    }
}