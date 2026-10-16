using dRz.Abstractions.Updater;
using System;

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
        /// Дата и время последнего завершённого запуска проверки/обновления в UTC. Не обновляется при отмене или блокировке обновления из-за сетевого расположения.
        /// </summary>
        DateTime? LastCheck { get; set; }

        /// <summary>
        /// Результат последней проверки или попытки обновления.
        /// </summary>
        UpdateResult? LastResult { get; set; }
    }
}
