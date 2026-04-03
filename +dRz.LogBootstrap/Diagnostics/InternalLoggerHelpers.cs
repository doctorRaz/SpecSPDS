using NLog;
using NLog.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace dRz.LogServices.Diagnostics
{
    public static class InternalLoggerHelpers
    {

        private static bool _initialized = false;
        private static readonly object _lock = new();

        /// <summary>
        /// Internal logger → Output Window (DEBUG) <br/>
        /// Internal logger → Output file.log<br/>
        /// Internal logger → Output Console<br/>
        /// </summary>        
        public static void ConfigureInternalLogger(string typeCaller, string? logDir = null)
        {
            lock (_lock)
            {
                // 1. Всегда читаем желаемый уровень
                // Если файла нет — Off (ничего не делаем). 
                // Если файл создан, но пустой — Trace (максимум инфы)
                // иначе уровень из файла.
                LogLevel requestedLevel = LogLevelReader.GetLevelFromFile(LogKeys.DiagnosticMode);

                // 2. Первая инициализация — настраиваем инфраструктуру, уровень из DiagnosticMode даже если OFF
                if (!_initialized)
                {

                    //think может лучше в appdata/dRzTools/logs?? если общий лог для всех аддонов пойдет туда
                    logDir ??= Path.Combine(Path.GetTempPath(), "dRzTools");

                    // 1. Сначала удаляем 6-й и далее файлы
                    RotateOldLogs(logDir);

                    // 2. Проверяем размер текущего. Если > 10МБ — переименовываем в архивный
                    string currentLogPath = GetInternalLogPath(logDir);
                    CheckCurrentFileSize(currentLogPath);

                    //пишем в файл
                    InternalLogger.LogFile = currentLogPath;

                    // пишем в output debuger
                    InternalLogger.LogWriter = new OutputDebugTextWriter();

                    InternalLogger.LogToConsole = true;

                    // Стартовый уровень — ставим как в DiagnosticMode
                    InternalLogger.LogLevel = requestedLevel;

                    //все исключения
                    LogManager.ThrowExceptions = false;

                    //ошибки конфига
                    LogManager.ThrowConfigExceptions = true;

                    InternalLogger.Info($"{typeCaller} InternalLogger Initialized, Level={requestedLevel}");

                    _initialized = true;
                }

                // 3. Управление уровнем (runtime)
                if (requestedLevel == LogLevel.Off)
                {
                    // принципиально: НЕ понижаем и НЕ выключаем
                    return;
                }

                if (requestedLevel < InternalLogger.LogLevel)
                {
                    InternalLogger.Info($"{typeCaller} InternalLogger Level changed {InternalLogger.LogLevel} → {requestedLevel}");

                    InternalLogger.LogLevel = requestedLevel;
                }

            }
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
                    string dir = Path.GetDirectoryName(filePath);
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
                if (!Directory.Exists(logDir))
                {
                    return;
                }

                // Получаем все internal-логи именно для этого приложения
                // Сортируем по дате создания (от новых к старым)
                List<FileInfo> files = new DirectoryInfo(logDir)
                 .GetFiles($"*_nlog-drzTools-internal*.log")
                 .OrderByDescending(f => f.LastWriteTime)
                 .ToList();

                int arhivedFilesCount = 5;

                // Если файлов больше 5, удаляем лишние
                if (files.Count > arhivedFilesCount)
                {

                    foreach (FileInfo file in files.Skip(arhivedFilesCount))
                    {
                        file.Delete();
                    }
                }
            }
            catch (Exception ex)
            {
                // Если не удалось почистить, просто выводим в трассировку
                Trace.WriteLine($"Failed to rotate internal logs: {ex.Message}");
            }
        }

        private static string GetInternalLogPath(string logDir) =>
           Path.Combine(logDir, $"{DateTime.Now:yyyy-MM-dd}_nlog-drzTools-internal.log");



        private const string logName = "nlog-drzTools-internal";
    }
}