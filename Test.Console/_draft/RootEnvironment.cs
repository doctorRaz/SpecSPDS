using System;
using System.IO;
using System.Reflection;

namespace dRz.SpecSpds.Test._draft
{
    public   class RootEnvironment
    {

        private static readonly Lazy<Assembly> _assembly =
            new Lazy<Assembly>(ResolveAdapterAssembly);

        private static readonly Lazy<string> _assemblyPath =
            new Lazy<string>(() => _assembly.Value.Location);

        private static readonly Lazy<string> _assemblyDirectory =
            new Lazy<string>(() => Path.GetDirectoryName(_assemblyPath.Value)!);

        private static readonly Lazy<string> _productName =
            new Lazy<string>(ResolveProductName);

        private static readonly Lazy<string> _appDataPath =
            new Lazy<string>(() =>
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    _productName.Value));

        public static Assembly Assembly => _assembly.Value;

        public static string AssemblyPath => _assemblyPath.Value;

        public static string AssemblyDirectory => _assemblyDirectory.Value;

        public static string ProductName => _productName.Value;

        public static string AppDataProductPath => _appDataPath.Value;

        // -------------------------------

        private static Assembly ResolveAdapterAssembly()
        {
            // Предпочтительно — сборка, где определён этот класс
            return typeof(RootEnvironment).Assembly;
        }

        private static string ResolveProductName()
        {
            var asm = _assembly.Value;

            var product =
                asm.GetCustomAttribute<AssemblyProductAttribute>()?.Product
                ?? asm.GetName().Name
                ?? "Adapter";

            return product.Trim();
        }
    }

}
