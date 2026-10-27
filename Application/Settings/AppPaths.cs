using System;
using System.IO;

namespace dRz.Application.Settings
{
    /// <summary>
    /// AppPaths
    /// </summary>
    public static class AppPaths
    {
        /// <summary>
        /// Initializes the specified product.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <exception cref="ArgumentException">product</exception>
        public static void Initialize(string product)
        {
            if (string.IsNullOrWhiteSpace(product))
            {
                throw new ArgumentException(nameof(product));
            }

            _product = product;
        }

        private static string _product;

        private static string Product =>
                _product ?? throw new InvalidOperationException("AppPaths not initialized");

        /// <summary>
        /// Gets the application root.
        /// </summary>
        /// <value>
        /// The application root.
        /// </value>
        public static string AppRoot =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Product);

        /// <summary>Каталог файлов конфигурации приложения.</summary>
        public static string ConfigDir =>
            Path.Combine(AppRoot, "config");

        /// <summary>Каталог файлов журналов приложения.</summary>
        public static string LogDir =>
            Path.Combine(AppRoot, "logs");

        /// <summary>Полный путь к файлу конфигурации приложения.</summary>
        public static string ConfigFile =>
            Path.Combine(ConfigDir, $"{Product}.config");

        /// <summary>Создаёт каталоги конфигурации и журналов, если они отсутствуют.</summary>
        public static void Ensure()
        {
            Directory.CreateDirectory(ConfigDir);
            Directory.CreateDirectory(LogDir);
        }
    }
}