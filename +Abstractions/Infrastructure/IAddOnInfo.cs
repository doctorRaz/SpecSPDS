using System;
using System.Collections.Generic;

namespace drz.Abstractions.Infrastructure
{
    /// <summary>Информация о сборке </summary>
    public interface IAddOnInfo : IStringConvertible
    {
        #region Public Properties

        /// <summary>Возвращает путь к настройкм аддона <br/>
        /// Логично коль общий путь зависит от компании и названия продукта<br/>
        /// хранить его в метаданных сборки </summary>
        /// <value>The product data directory.</value>
        string ProductDataDirectory { get; }

        /// <summary>Gets the company.</summary>
        /// <value>The company.</value>
        string Company { get; }

        /// <summary>Возвращает путь к корневому каталогу ад дона где находится package</summary>
        /// <value>путь к корневому каталогу ад дона</value>
        string PackageDirectory { get; }

        /// <summary>Возвращает имя файла package.</summary>
        /// <value>Имя файла package.</value>
        string PackageFileName { get; }

        /// <summary>Gets a value indicating whether this instance has package.</summary>
        /// <value>
        ///   <c>true</c> if this instance has package; otherwise, <c>false</c>.
        /// </value>
        bool HasPackage { get; }

        /// <summary> "Полное Имя" сборки, используется для показа в заголовках диалогов, окон, сообщений </summary>
        /// <value>"Полное Имя" сборки.</value>
        string AssembleFullName { get; }

        /// <summary> Возвращает директорию сборки. </summary>
        /// <value> Директория сборки. </value>
        string AssemblyDirectory { get; }

        /// <summary>Возвращает полный путь к сборке.</summary>
        /// <value>Полный путь к сборке.</value>
        string AssemblyPath { get; }

        /// <summary>Возвращает версию загруженной сборки.</summary>
        /// <value>версия сборки.</value>
        Version RunningVersion { get; }

        /// <summary>Возвращает версию установленной сборки.</summary>
        /// <value>версия сборки.</value>
        Version InstalledVersion { get; }

        /// <summary>Возвращает дату-время компиляции сборки.</summary>
        /// <value>Дата-время компиляции сборки.</value>
        DateTime BuildDate { get; }

        /// <summary>Возвращает информацию о копирайте.</summary>
        /// <value>Копирайт.</value>
        string Copyright { get; }

        /// <summary>Возвращает описание сборки.</summary>
        /// <value>The description.</value>
        string Description { get; }

        /// <summary>Возвращает имя файла сборки без расширения.</summary>
        /// <value>Имя файла сборки без расширения.</value>
        string FileName { get; }

        /// <summary>Возвращает AssemblyFileVersionAttribute.</summary>
        string FileVersion { get; }

        /// <summary>Возвращает AssemblyInformationalVersionAttribute.</summary>
        string InformationalVersion { get; }

        /// <summary>Признак, что дата сборки получена из версии.</summary>
        bool IsAutoVersion { get; }

        /// <summary>Возвращает AssemblyProductAttribute.</summary>
        string Product { get; }

        /// <summary>Возвращает AssemblyTitleAttribute.</summary>
        string ProductTitle { get; }

        /// <summary>Возвращает ProductName v.RunningVersion.</summary>
        string ProductTitlePrefix { get; }

        /// <summary>Gets the repository URL.</summary>
        /// <value>The repository URL.</value>
        string RepositoryUrl { get; }

        /// <summary>
        /// Семейство хоста, для которого собрана сборка
        /// </summary>
        string HostFamily { get; }

        /// <summary>
        /// Короткий код хоста для условной компиляции, логов и имен файлов
        /// </summary>
        string HostCode { get; }

        /// <summary>
        /// Все доступные ключи метаданных.
        /// </summary>
        IEnumerable<string> MetadataKeys { get; }

        /// <summary>
        /// Все пары ключ-значение метаданных.
        /// Используется для диагностики и вывода информации.
        /// </summary>
        IEnumerable<KeyValuePair<string, string>> MetadataItems { get; }

        /// <summary>
        /// Получить значение метаданных.
        /// </summary>
        string? GetMetadata(string key);

        /// <summary>
        /// Получить значение метаданных с заданным значением по умолчанию.
        /// </summary>
        string GetMetadata(string key, string defaultValue);

        /// <summary>
        /// Попытаться получить значение метаданных.
        /// </summary>
        bool TryGetMetadata(string key, out string value);

        #endregion Public Properties
    }
}