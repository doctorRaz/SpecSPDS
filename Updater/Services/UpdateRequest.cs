using System;

namespace dRz.Updater.Services
{
    /// <summary>Входные параметры для проверки и установки обновления.</summary>
    public sealed class UpdateRequest
    {
        /// <summary>Текущая установленная версия аддона.</summary>
        public Version CurrentVersion { get; init; } = new Version();

        /// <summary>URL источника файла update.json.</summary>
        public string UpdateUrl { get; init; } = string.Empty;

        /// <summary>Способ запуска проверки. При ручном запуске используется CheckAndNotify.</summary>
        public UpdateLaunchMode LaunchMode { get; init; } = UpdateLaunchMode.Automatic;

        /// <summary>Полный путь к целевому каталогу установленного аддона или модуля.</summary>
        public string PackageDirectory { get; init; } = string.Empty;

        /// <summary>Определяет структуру каталога продукта внутри архива.</summary>
        public bool IsPackage { get; init; }

        /// <summary>Признак загрузки основной сборки аддона с сетевого расположения.</summary>
        public bool IsNetworkPath { get; init; }
    }
}