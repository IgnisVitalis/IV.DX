using System.Reflection;

namespace IV.DX.Kernel.Helpers
{
    internal static class ResourceReader
    {
        public static string ReadEmbeddedText(Assembly assembly, string pathInAssembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));
            if (string.IsNullOrWhiteSpace(pathInAssembly))
                throw new ArgumentNullException(nameof(pathInAssembly));

            string rootNamespace = assembly.GetName().Name!;
            string resourceName = $"{rootNamespace}.{pathInAssembly.Replace('/', '.').Replace('\\', '.')}";

            using Stream? stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"Resource '{resourceName}' not found in assembly '{assembly.FullName}'.");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
