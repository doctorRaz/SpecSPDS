using System;

namespace dRz.Updater.Services
{
    /// <summary>
    /// Входные параметры для проверки и установки обновления.
    /// Значения формируются основным аддоном из его настроек.
    /// </summary>
    public sealed class UpdateRequest
    {
        /// <summary>
        /// Текущая установленная версия аддона.
        /// </summary>
        public Version CurrentVersion { get; init; } = new Version();

        /// <summary>
        /// URL источника файла update.json.
        /// </summary>
        public string UpdateUrl { get; init; } = string.Empty;

        /// <summary>
        /// Режим работы с обновлениями.
        /// </summary>
        public UpdateMode Mode { get; init; }

        /// <summary>
        /// Полный путь к каталогу установленного аддона.<br/>
        /// Может быть на уровень выше аддона, включая файл paсkage
        /// </summary>
        public string AddOnDirectory { get; init; } = string.Empty;


    }
}
