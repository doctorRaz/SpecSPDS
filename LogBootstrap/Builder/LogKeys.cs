namespace dRz.LogBootstrap.Builder
{
    /// <summary>
    /// константы названий файлов <br/>
    /// маркер-флаг изменения уровня логирования
    /// </summary>
    internal static class LogKeys
    {
        /// <summary>
        /// уровень логирования интернал логера <br/>
        /// в первой строке текстового файла уровень Trace...Fatal
        /// </summary>
        public const string DiagnosticMode = "diagnostic.level";

        /// <summary>
        /// уровень логирования основного логера <br/>
        /// в первой строке текстового файла уровень Trace...Fatal
        /// </summary>
        public const string LogLevel = "logger.level";
    }
}