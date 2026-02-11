using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dRz.Loader.Cad._Draft

{
    /// <summary>
    /// Неиспользуется <br/>
    /// Предложил GPT кастомный логгер <br/>
    /// в принципе рабочий вариент, но <br/> 
    /// не работает обрезка файла лога
    /// </summary>
    internal class LoaderLoggerGPT
    {

        private const long MaxSizeBytes = 256 * 1024; // 256 KB

        private static readonly string LogPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SpecSPDS",
                "logs",
                "loader.log");

        private static bool _initialized;

        public static void Info(string message)
            => Write("INFO", message);

        public static void Error(string message, Exception? ex = null)
            => Write("ERROR", ex == null ? message : $"{message}\n{ex}");

        private static void Write(string level, string message)
        {
            try
            {
                EnsureInitialized();

                var line =
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{level}] {message}{Environment.NewLine}";

                File.AppendAllText(LogPath, line, Encoding.UTF8);
            }
            catch
            {
                // Логгер loader НИКОГДА не должен ронять хост
            }
        }

        private static void EnsureInitialized()
        {
            if (_initialized)
                return;

            _initialized = true;

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
                TrimIfNeeded();
            }
            catch
            {
                // игнорируем
            }
        }

        private static void TrimIfNeeded()
        {
            var fi = new FileInfo(LogPath);
            if (!fi.Exists || fi.Length <= MaxSizeBytes)
                return;

            using var fs = new FileStream(LogPath, FileMode.Open, FileAccess.Read);
            fs.Seek(-MaxSizeBytes, SeekOrigin.End);

            using var reader = new StreamReader(fs, Encoding.UTF8);
            string tail = reader.ReadToEnd();

            File.WriteAllText(LogPath, tail, Encoding.UTF8);
        }


    }
}
