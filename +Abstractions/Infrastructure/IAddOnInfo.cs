using System;
using System.Reflection;

namespace drz.Abstractions.Infrastructure
{
    /// <summary>Информация о сборке </summary>
    public interface IAddOnInfo
    {
        #region Public Properties

        /// <summary> Возвращает путь к журналу данных приложения.</summary>
        /// <value>Путь к журналу данных приложения.</value>
        string AppDataProductLogPath { get; }

        /// <summary> Возвращает путь к данным приложения.</summary>
        /// <value> Путь к данным приложения. </value>
        string AppDataProductPath { get; }

        /// <summary>Возвращает путь к корневому каталогу ад дона где находится package</summary>
        /// <value>путь к корневому каталогу ад дона</value>
        string PackageDirectory { get; }

        /// <summary>Возвращает имя файла package.</summary>
        /// <value>Имя файла package.</value>
        string PackageFileName  { get; }

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

        /// <summary>Возвращает версию сборки.</summary>
        /// <value>Путь версию сборки.</value>
        Version AssemblyVersion { get; }

        /// <summary>Возвращает дату-время компиляции к сборки.</summary>
        /// <value>Дата-время компиляции к сборки.</value>
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

        /// <summary>Возвращает имя файла сборки до первой точки.</summary>
        string FilePrefix { get; }

        /// <summary>Возвращает AssemblyFileVersionAttribute.</summary>
        string FileVersion { get; }

        /// <summary>Возвращает AssemblyInformationalVersionAttribute.</summary>
        string InformationalVersion { get; }

        /// <summary>Признак, что дата сборки получена из версии.</summary>
        bool IsAutoVersion { get; }

        //string NLogConfigPath { get; }

        /// <summary>Возвращает AssemblyProductAttribute.</summary>
        string ProductName { get; }

        /// <summary>Возвращает AssemblyTitleAttribute.</summary>
        string ProductTitle { get; }

        /// <summary>Возвращает ProductName v.AssemblyVersion.</summary>
        string TitlePrefix { get; }

        /// <summary>Gets the repository URL.</summary>
        /// <value>The repository URL.</value>
        string RepositoryUrl {  get; }

        /// <summary>Gets the cad family.</summary>
        /// <value>The cad family.</value>
        string CadFamily {  get; }

        /// <summary>Gets the cad code.</summary>
        /// <value>The cad code.</value>
        string CadCode { get; }

        #endregion Public Properties

        #region Public Methods

        /// <summary>Converts to longstring.</summary>
        /// <returns>long string</returns>
        string ToLongString();

        /// <summary>Converts to shortstring.</summary>
        /// <returns>short string</returns>
        string ToShortString();

        /// <summary>Converts to string.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        string ToString();

        #endregion Public Methods
    }
}