using System;
using dRz.Abstractions.Updater;

namespace dRz.Abstractions.Settings
{
    /// <summary>
    /// Настройки и состояние обновления аддона.
    /// </summary>
    public interface IUpdateSettings
    {
        /// <summary>
        /// Режим проверки и установки необязательных обновлений.
        /// </summary>
        UpdateMode Mode { get; set; }

        /// <summary>
        /// Дата и время последней завершённой проверки обновлений в UTC.
        /// </summary>
        DateTime? LastCheck { get; set; }

        /// <summary>
        /// Результат последней проверки или попытки обновления.
        /// </summary>
        UpdateResult? LastResult { get; set; }
    }
}
