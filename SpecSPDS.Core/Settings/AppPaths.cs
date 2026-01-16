using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drz.SpecSPDS.Core.Settings
{
    public static class AppPaths
    {
        public static void Initialize(string product)
        {
            if (string.IsNullOrWhiteSpace(product))
                throw new ArgumentException(nameof(product));

            _product = product;
        }

        private static string? _product;
        private static string Product =>
                _product ?? throw new InvalidOperationException("AppPaths not initialized");
        public static string AppRoot =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Product);

        public static string ConfigDir =>
            Path.Combine(AppRoot, "config");

        public static string LogDir =>
            Path.Combine(AppRoot, "logs");

        public static string ConfigFile =>
            Path.Combine(ConfigDir, $"{Product}.config");

        public static void Ensure()
        {
            Directory.CreateDirectory(ConfigDir);
            Directory.CreateDirectory(LogDir);
        }
    }
}
