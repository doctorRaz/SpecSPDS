using NLog;
using NLog.Common;
using System;
using dRz.Loader.Cad.Infrastructure.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace dRz.Loader.Cad.Infrastructure.Logging.Diagnostics
{
    internal static class InternalLoggerHelpers
    {
        /// <summary>
        /// Internal logger → Output Window (DEBUG) <br/>
        /// Internal logger → Output file.log<br/>
        /// Internal logger → Output Console<br/>
        /// </summary>
        internal static void ConfigureInternalLogger()
        {
            // Если файла нет — Off (ничего не делаем). 
            // Если файл создан, но пустой — Trace (максимум инфы).
            LogLevel level = LogLevelReader.GetLevelFromFile("diagnostic.mode", LogLevel.Off, LogLevel.Trace);

            if (level == LogLevel.Off) return;

            string logDir = LoaderEnvironment.AppDataProductLogPath;

            // 1. Сначала удаляем 6-й и далее файлы
            RotateOldLogs(logDir);

            // 2. Проверяем размер текущего. Если > 10МБ — переименовываем в архивный
            string currentLogPath = GetInternalLogPath(logDir);
            CheckCurrentFileSize(currentLogPath);

            InternalLogger.LogLevel = level;

            //пишем в файл
            InternalLogger.LogFile = currentLogPath;

            // пишем в output debuger
            InternalLogger.LogWriter = new OutputDebugTextWriter();

            InternalLogger.LogToConsole = true;

            //все исключения
            LogManager.ThrowExceptions = false;

            //ошибки конфига
            LogManager.ThrowConfigExceptions = true;

            InternalLogger.Info($"{LoaderEnvironment.FileName}: InternalLogger Initialize Level={level}");

        }

        /// <summary>
        /// Checks the size of the current file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private static void CheckCurrentFileSize(string filePath)
        {
            try
            {

                FileInfo file = new FileInfo(filePath);
                if (file.Exists && file.Length > 10 * 1024 * 1024)
                {
                    string? dir = Path.GetDirectoryName(filePath);
                    string newName = $"{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.Now:HHmmss}.log";
                    file.MoveTo(Path.Combine(dir, newName));
                }
            }
            catch { }
        }


        /// <summary>
        /// Rotates the internal logs.
        /// </summary>
        /// <param name="logDir">The log dir.</param>
        private static void RotateOldLogs(string logDir)
        {
            try
            {
                if (!Directory.Exists(logDir)) return;

                // Получаем все internal-логи именно для этого приложения
                // Сортируем по дате создания (от новых к старым)
                List<FileInfo> files = new DirectoryInfo(logDir)
                 .GetFiles($"*_{LoaderEnvironment.FileName}_internal*.log")
                 .OrderByDescending(f => f.LastWriteTime)
                 .ToList();

                int arhivedFilesCount = 5;

                // Если файлов больше 5, удаляем лишние
                if (files.Count > arhivedFilesCount)
                {

                    foreach (FileInfo? file in files.Skip(arhivedFilesCount)) file.Delete();
                }
            }
            catch (Exception ex)
            {
                // Если не удалось почистить, просто выводим в трассировку
                Trace.WriteLine($"Failed to rotate internal logs: {ex.Message}");
            }
        }

        private static string GetInternalLogPath(string logDir) =>
           Path.Combine(logDir, $"{DateTime.Now:yyyy-MM-dd}_{LoaderEnvironment.FileName}_internal.log");


    }
}