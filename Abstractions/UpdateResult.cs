namespace dRz.Abstractions.Updater
{
    /// <summary>Результат проверки и установки обновления.</summary>
    public enum UpdateResult
    {
        Installed,
        UpToDate,
        Declined,
        Disabled,
        NetworkPath,
        Failed,
        Cancelled
    }
}
