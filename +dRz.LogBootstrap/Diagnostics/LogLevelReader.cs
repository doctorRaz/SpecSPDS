using NLog;
using System.IO;

namespace dRz.LogServices.Diagnostics
{
    internal static class LogLevelReader
    {
        /// <summary>
        /// Читает LogLevel из файла.
        /// Если файла нет — возвращает defaultLevel.
        /// Если файл пустой или текст некорректный — возвращает fallbackIfEmpty.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>`
        /// <param name="fallbackLevelName">The default level.</param>
        /// <returns></returns>
        public static LogLevel GetLevelFromFile(string path /*fileName*/, string fallbackLevelName = "Trace")
        {
            //передавать каталог ассембле сборки
            //получать уровни для диагностики и для логера

            //если файла настроек нет
            LogLevel defaultLevel = LogLevel.Off;

            //файл настроек есть, но уровень определить не удалось
            LogLevel fallbackIfEmpty = TryFromString(fallbackLevelName, LogLevel.Trace);

            try
            {
                //путь к файлу настроек

                //string pp=Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                //string path = Path.Combine(pp /*InfoDll.AssemblyDirectory*/, fileName);
                
                if (!File.Exists(path))
                {
                    //файла нет
                    return defaultLevel;
                }

                using FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                using StreamReader sr = new StreamReader(fs);

                string text = sr.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(text))
                {
                    //файл есть, но пустой
                    return fallbackIfEmpty;
                }

                // пытаемся конвертнуть в LogLevel, если не вышло вернем умолчание
                return TryFromString(text!, fallbackIfEmpty);
            }
            catch
            {
                //хз что за ошибка , на всякий случай офф
                return defaultLevel;
            }
        }

        private static LogLevel TryFromString(string levelName, LogLevel defaultLevel)
        {
            try
            {
                return LogLevel.FromString(levelName);
            }
            catch
            {
                return defaultLevel;
            }
        }
    }
}