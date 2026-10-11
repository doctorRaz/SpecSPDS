namespace dRz.Updater.Services
{
    /// <summary>
    /// Определяет способ запуска проверки обновлений.
    /// </summary>
    public enum UpdateTrigger
    {
        /// <summary>
        /// Автоматический запуск без дополнительных уведомлений о результате проверки.
        /// </summary>
        Automatic,

        /// <summary>
        /// Ручной запуск с уведомлением об отсутствии обновлений и ошибках.
        /// </summary>
        Manual
    }
}