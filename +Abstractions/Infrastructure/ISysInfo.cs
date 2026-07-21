//GPT
// https://chatgpt.com/c/69c44adf-7f6c-8331-80de-c905a35fea87

using System;

namespace drz.Abstractions.Infrastructure
{
    /// <summary>Информация о системе </summary>
    public interface ISysInfo
    {
        #region Public Properties

        /// <summary>Gets the architecture.</summary>
        /// <value>The architecture.</value>
        string Architecture { get; }

        /// <summary>Gets the build lab.</summary>
        /// <value>The build lab.</value>
        string BuildLab { get; }

        /// <summary>Gets the display version.</summary>
        /// <value>The display version.</value>
        string DisplayVersion { get; }

        /// <summary>Gets the edition identifier.</summary>
        /// <value>The edition identifier.</value>
        string EditionId { get; }

        /// <summary>Gets the gpu information.</summary>
        /// <value>The gpu information.</value>
        string GpuInfo { get; }

        /// <summary>Gets the type of the installation.</summary>
        /// <value>The type of the installation.</value>
        string InstallationType { get; }

        /// <summary> true = использован fallback (Environment).</summary>
        /// <value>
        ///   <c>true</c> if this instance is fallback; otherwise, <c>false</c>.
        /// </value>
        bool IsFallback { get; }

        /// <summary>Gets the os version.</summary>
        /// <value>The os version.</value>
        Version OsVersion { get; }

        /// <summary>Gets the name of the processor.</summary>
        /// <value>The name of the processor.</value>
        string ProcessorName { get; }

        /// <summary>Gets the name of the product.</summary>
        /// <value>The name of the product.</value>
        string ProductName { get; }

        /// <summary>Gets the ram total gb.</summary>
        /// <value>The ram total gb.</value>
        string RamTotalGb { get; }

        /// <summary>Gets the version string.</summary>
        /// <value>The version string.</value>
        string VersionString { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>Converts to string.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        string ToString();

        /// <summary>Converts to long string.</summary>
        /// <returns></returns>
        string ToLongString();

        /// <summary>Converts to short string.</summary>
        /// <returns></returns>
        string ToShortString();

        #endregion Public Methods
    }
}