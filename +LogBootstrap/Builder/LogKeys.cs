namespace drz.LogBootstrap.Builder
{
    /// <summary>
    /// константы названий файлов <br/>
    /// маркер-флаг изменения уровня логирования
    /// </summary>
    internal static class LogKeys
    {
        #region Public Fields

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

        #endregion Public Fields
    }

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
        /// level для GDC
        /// </summary>
        public const string LevelMay = "LevelMay";

        /// <summary>
        /// каталог логов GDC
        /// </summary>
        public const string LogsDir = "LogsDir";
        #endregion GDC

        #region var

        /// <summary>
        /// имя лога var
        /// </summary>
        public const string FinalAppTitle = "FinalAppTitle";

        /// <summary>
        /// level для Var
        /// </summary>
        public const string FinalLevel = "FinalLevel";

        /// <summary>
        /// каталог логов var
        /// </summary>
        public const string FinalLogsDir = "FinalLogsDir";
        #endregion var
    }
}