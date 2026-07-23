namespace drz.Updater.Services
{
    /// <summary>
    /// Режим проверки обновлений
    /// </summary>
    public enum UpdateMode
    {
        /// <summary>
        /// не проверять
        /// </summary>
        Disabled,

        /// <summary>
        /// проверить и уведомить
        /// </summary>
        CheckAndNotify,

        /// <summary>
        /// проверить и установить
        /// </summary>
        CheckAndInstall
    }
}