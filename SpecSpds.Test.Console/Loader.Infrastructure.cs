using System;
using System.IO;
using System.Reflection;

namespace Loader.Infrastructure
{
    /// <summary>
    /// Immutable runtime environment for loader.
    /// All values are computed once on first access.
    /// </summary>
    public static class LoaderEnvironment
    {
        // -------------------------
        // Assembly
        // -------------------------

        private static readonly Assembly Assembly =
            typeof(LoaderEnvironment).Assembly;

        /// <summary>
        /// Full path to loader assembly (.dll).
        /// Fallback-safe for single-file.
        /// </summary>
        public static readonly string AssemblyPath =
            ResolveAssemblyPath();

        /// <summary>
        /// Directory containing loader assembly.
        /// </summary>
        public static readonly string AssemblyDirectory =
            Path.GetDirectoryName(AssemblyPath)!;

        // -------------------------
        // Product
        // -------------------------

        private static readonly string FileName =
            Path.GetFileNameWithoutExtension(AssemblyPath);

        /// <summary>
        /// Product name derived from assembly file name.
        /// Example: Specspds.ncad → Specspds
        /// </summary>
        public static readonly string ProductName =
            ExtractProductPrefix(FileName);

        // -------------------------
        // AppData
        // -------------------------

        /// <summary>
        /// %AppData%/ProductName (cross-platform)
        /// </summary>
        public static readonly string AppDataProductPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                ProductName);

        // =====================================================================

        private static string ResolveAssemblyPath()
        {
            // Assembly.Location may be empty in single-file publish
            string location = Assembly.Location;

            if (!string.IsNullOrEmpty(location))
                return location;

            // Guaranteed fallback
            return AppContext.BaseDirectory.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
        }

        private static string ExtractProductPrefix(string fileName)
        {
            int dotIndex = fileName.IndexOf('.');
            return dotIndex > 0
                ? fileName.Substring(0, dotIndex)
                : fileName;
        }
    }
}
