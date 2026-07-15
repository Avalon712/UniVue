namespace UniVue.CodeGen
{
    internal static class IgnoreILInjectAssembly
    {
        public static bool Ignore(string assemblyName)
        {
            return string.IsNullOrEmpty(assemblyName) ||
                   IsUniVueInternalIgnoreDll(assemblyName) ||
                   assemblyName.StartsWith("Unity") ||
                   assemblyName.StartsWith("System") ||
                   assemblyName.StartsWith("mscorlib") ||
                   assemblyName.StartsWith("netstandard") ||
                   assemblyName.StartsWith("Mono") ||
                   assemblyName.StartsWith("NUnit") ||
                   assemblyName.StartsWith("Mono.Cecil");
        }

        private static bool IsUniVueInternalIgnoreDll(string assemblyName)
        {
            //测试集UniVue.Tests不能忽略
            return assemblyName.StartsWith("UniVue.Runtime") ||
                   assemblyName.StartsWith("UniVue.CodeGen") ||
                   assemblyName.StartsWith("UniVue.Editor") ||
                   assemblyName.StartsWith("VCoroutine");
        }
    }
}