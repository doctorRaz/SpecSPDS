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
        /// Полный путь к целевому каталогу установленного аддона или модуля.
        /// Файлы обновления будут установлены непосредственно в этот каталог.
        /// </summary>
        public string PackageDirectory { get; init; } = string.Empty;

        /// <summary>
        /// Определяет структуру каталога продукта внутри архива.
        /// При <see langword="true"/> файлы находятся в каталоге продукта;
        /// при <see langword="false"/> после каталога продукта ожидается каталог,
        /// имя которого совпадает с именем каталога из <see cref="PackageDirectory"/>.
        /// </summary>
        public bool IsPackage { get; init; } = false;


    }
}
