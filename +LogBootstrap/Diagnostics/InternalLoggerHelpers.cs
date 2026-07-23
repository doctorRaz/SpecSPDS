using NLog;
using NLog.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace drz.LogBootstrap.Diagnostics
{
    /// <summary>
    ///
    /// </summary>
    public static class InternalLoggerHelpers
    {
        #region Private Fields

        //private const string logName = "nlog-drzTools-internal";
        private const int MaxArchiveFiles = 5;

        private const int MaxFileSizeBytes = 10 * 1024 * 1024;
        private static readonly object _lock = new();
        private static bool _initialized = false;
        private static string _logDir;

        #endregion Private Fields

        #region Public Methods

        /// <summary>Gets the log dir.</summary>
        /// <value>The log dir.</value>
        internal static string LogDir => _logDir;

        /// <summary>Gets a value indicating whether this <see cref="InternalLoggerHelpers"/> is initialized.</summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        internal static bool Initialized => _initialized;

        // 10 MB
        /// <summary>
        /// Internal logger → Output Window (DEBUG) <br/>
        /// Internal logger → Output file.log<br/>
        /// Internal logger → Output Console<br/>
        /// </summary>
        public static void ConfigureInternalLogger(string typeCaller, LogLevel requestedLevel, string logName, string logDir = null)
        {
            lock (_lock)
            {
                /* Получаем уровень из файла Diagnostic.Mode
                    Если файла нет — Off (ничего не делаем).
                        Если файл создан, но пустой — Trace (максимум инфы)
                        иначе уровень из файла Diagnostic.Mode.
                */

                // 1. Первая инициализация — настраиваем инфраструктуру, уровень из DiagnosticMode даже если OFF
                if (!_initialized)
                {
                    logDir ??= Path.Combine(Path.GetTempPath(), "dRzTools", "logs");

                    _logDir = logDir;

                    string currentLogPath = GetInternalLogPath(logName);

                    // 1. Сначала удаляем 6-й и далее файлы
                    RotateOldLogs(logName);

                    // 2. Проверяем размер текущего. Если > 10МБ — переименовываем в архивный
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

                    return; // уровень уже установлен, дальше ничего не меняем
                }

                // Runtime — понижение уровня для увеличения детализации
                if (requestedLevel != LogLevel.Off && requestedLevel < InternalLogger.LogLevel)
                {
                    InternalLogger.LogLevel = requestedLevel;

                    InternalLogger.Info($"{typeCaller} InternalLogger Level changed {InternalLogger.LogLevel} → {requestedLevel}");
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Checks the size of the current file.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        private static void CheckCurrentFileSize(string filePath)
        {
            try
            {
                FileInfo file = new FileInfo(filePath);
                if (file.Exists && file.Length > MaxFileSizeBytes)
                {
                    string dir = Path.GetDirectoryName(filePath);
                    string newName = $"{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.Now:HHmmss}.log";
                    file.MoveTo(Path.Combine(dir, newName));
                }
            }
            catch { }
        }

        private static string GetInternalLogPath(string logName) =>
                   Path.Combine(LogDir, $"{DateTime.Now:yyyy-MM-dd}_{logName}_internal.log");

        /// <summary>Rotates the old logs.</summary>
        /// <param name="logName">Name of the log.</param>
        private static void RotateOldLogs(string logName)
        {
            try
            {
                if (!Directory.Exists(LogDir))
                {
                    return;
                }

                // Получаем все internal-логи именно для этого приложения
                // Сортируем по дате создания (от новых к старым)
                List<FileInfo> files = new DirectoryInfo(LogDir)
                                         .GetFiles($"*_{logName}*.log")
                                         .OrderByDescending(f => f.LastWriteTime)
                                         .ToList();

                // Если файлов больше 5, удаляем лишние
                //if (files.Count > arhivedFilesCount)
                //{
                foreach (FileInfo file in files.Skip(MaxArchiveFiles))
                {
                    file.Delete();
                }
                //}
            }
            catch { }
        }

        #endregion Private Methods
    }
}