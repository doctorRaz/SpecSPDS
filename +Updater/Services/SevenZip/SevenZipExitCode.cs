namespace drz.Updater.Services.SevenZip
{

    /// <summary>
    /// Коды завершения 7-Zip.
    /// </summary>
    public enum SevenZipExitCode
    {
        /// <summary>Операция успешно завершена.</summary>
        Success = 0,

        /// <summary>Операция выполнена с предупреждениями.</summary>
        Warning = 1,

        /// <summary>Фатальная ошибка.</summary>
        Error = 2,

        /// <summary>Ошибка командной строки.</summary>
        CommandLineError = 7,

        /// <summary>Недостаточно памяти.</summary>
        OutOfMemory = 8,

        /// <summary>Операция отменена пользователем.</summary>
        UserCanceled = 255
    }
}