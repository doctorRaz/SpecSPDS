namespace dRz.LogBootstrap.Builder
{
    /// <summary>
    /// Содержит имена файлов, через которые можно переопределить уровни логирования.
    /// </summary>
    internal static class LogKeys
    {
        /// <summary>
        /// Имя файла с уровнем внутренней диагностики NLog.
        /// </summary>
        /// <remarks>
        /// Уровень указывается в первой строке файла. Поддерживаемые значения: Trace, Debug,
        /// Info, Warn, Error, Fatal и Off.
        /// </remarks>
        public const string DiagnosticMode = "diagnostic.mode";

        /// <summary>
        /// Имя файла с уровнем основного логгера.
        /// </summary>
        /// <remarks>
        /// Уровень указывается в первой строке файла. Поддерживаемые значения: Trace, Debug,
        /// Info, Warn, Error, Fatal и Off.
        /// </remarks>
        public const string LogLevel = "log.level";
    }
}