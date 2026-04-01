namespace dRz.Cad.Diagnostics.Diagnostics
{
    /// <summary>
    /// константы названий variable лога
    /// </summary>
    internal static class LogVar
    {
        #region GDC

        /// <summary>
        /// префикс имени файла лога GDC
        /// </summary>
        public const string AppTitle = "AppTitle";

        /// <summary>
        /// каталог логов GDC
        /// </summary>
        public const string LogsDir = "LogsDir";

        /// <summary>
        /// level для GDC
        /// </summary>
        public const string LevelMay = "LevelMay";

        #endregion

        #region var        

        /// <summary>
        /// level для Var
        /// </summary>
        public const string FinalLevel = "FinalLevel";

        /// <summary>
        /// каталог логов var
        /// </summary>
        public const string FinalLogsDir = "FinalLogsDir";

        /// <summary>
        /// имя лога var
        /// </summary>
        public const string FinalAppTitle = "FinalAppTitle";

        #endregion

    }

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
        public const string DiagnosticMode = "diagnostic.mode";

        /// <summary>
        /// уровень логирования основного логера <br/>
        /// в первой строке текстового файла уровень Trace...Fatal
        /// </summary>
        public const string LogLevel = "log.level";
    }
}
