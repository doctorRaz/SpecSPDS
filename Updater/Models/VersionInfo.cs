namespace dRz.Updater.Models
{
    /// <summary>
    /// Четыре компонента версии из update.json.
    /// </summary>
    public sealed class VersionInfo
    {
        public int Major { get; init; }
        public int Minor { get; init; }
        public int Build { get; init; }
        public int Revision { get; init; }

        /// <summary>
        /// Преобразует описание версии в <see cref="Version"/>.
        /// </summary>
        public Version ToVersion()
        {
            return new Version(Major, Minor, Build, Revision);
        }
    }
}