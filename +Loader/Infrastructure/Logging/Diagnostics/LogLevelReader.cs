using dRz.Loader.Infrastructure.Info;
using NLog;
using System.IO;

namespace dRz.Loader.Infrastructure.Logging.Diagnostics
{
    internal static class LogLevelReader
    {
        /// <summary>
        /// Читает LogLevel из файла.
        /// Если файла нет — возвращает defaultLevel.
        /// Если файл пустой или текст некорректный — возвращает fallbackIfEmpty.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="defaultLevel">The default level.</param>
        /// <param name="fallbackIfEmpty">The fallback if empty.</param>
        /// <returns></returns>
        public static LogLevel GetLevelFromFile(string fileName, LogLevel defaultLevel, LogLevel fallbackIfEmpty)
        {
            try
            {
                string path = Path.Combine(InfoAdOn.AssemblyDirectory, fileName);
                if (!File.Exists(path))
                {
                    return defaultLevel;
                }

                using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                using StreamReader sr = new StreamReader(fs);

                string? text = sr.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(text))
                {
                    return fallbackIfEmpty;
                }

                return LogLevel.FromString(text) ?? fallbackIfEmpty;
            }
            catch
            {
                return defaultLevel;
            }
        }
    }
}