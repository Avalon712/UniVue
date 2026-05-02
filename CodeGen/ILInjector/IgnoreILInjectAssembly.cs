namespace UniVue.CodeGen
{
    internal static class IgnoreILInjectAssembly
    {
        public static bool Ignore(string assemblyName)
        {
            return string.IsNullOrEmpty(assemblyName) ||
                   assemblyName.StartsWith("UniVue") ||
                   assemblyName.StartsWith("Unity") ||
                   assemblyName.StartsWith("System") ||
                   assemblyName.StartsWith("mscorlib") ||
                   assemblyName.StartsWith("netstandard") ||
                   assemblyName.StartsWith("Mono") ||
                   assemblyName.StartsWith("NUnit") ||
                   assemblyName.StartsWith("Mono.Cecil");
        }
    }
}