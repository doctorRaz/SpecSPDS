using dRz.Updater.Models;
using System;

namespace dRz.Updater.Services
{
    /// <summary>
    /// Определяет, требуется ли обновление с учётом минимально допустимой версии.
    /// </summary>
    public static class UpdateVersionChecker
    {
        /// <summary>
        /// Сравнивает установленную и доступную версии.
        /// </summary>
        /// <returns>
        /// <see langword="null"/>, если обновление не требуется;
        /// <see langword="false"/>, если требуется обычное обновление;
        /// <see langword="true"/>, если требуется обязательное обновление.
        /// </returns>
        public static bool? Check(Version currentVersion, UpdateInfo update)
        {
            ArgumentNullException.ThrowIfNull(currentVersion);
            ArgumentNullException.ThrowIfNull(update);

            Version minimumVersion = update.MinimumVersion.ToVersion();
            Version updateVersion = update.Version.ToVersion();

            // Минимальная версия имеет приоритет над mandatory:
            // ниже неё автоматическое обновление запрещено.
            if (currentVersion < minimumVersion)
                throw new MinimumVersionException(currentVersion, minimumVersion);

            if (currentVersion >= updateVersion)
                return null;

            return update.Mandatory;
        }
    }

    /// <summary>
    /// Установленная версия ниже минимально допустимой.
    /// </summary>
    public sealed class MinimumVersionException : Exception
    {
        public MinimumVersionException(Version currentVersion, Version minimumVersion)
            : base($"Установленная версия {currentVersion} ниже минимально допустимой {minimumVersion}. Требуется ручное обновление.")
        {
            CurrentVersion = currentVersion;
            MinimumVersion = minimumVersion;
        }

        public Version CurrentVersion { get; }

        public Version MinimumVersion { get; }
    }
}