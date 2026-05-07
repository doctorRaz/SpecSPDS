using System;
using System.Reflection;

namespace drz.Abstractions.Infrastructure
{
    /// <summary>Информация о сборке </summary>
    public interface IApplicationInfo_NEW
    {
        #region Public Properties

        /// <summary> Возвращает путь к журналу данных приложения.</summary>
        /// <value>
        /// Путь к журналу данных приложения.
        /// </value>
        string AppDataProductLogPath { get; }

        /// <summary> Возвращает путь к данным приложения.</summary>
        /// <value> Путь к данным приложения. </value>
        string AppDataProductPath { get; }

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

         /// <summary> "Полное Имя" сборки, используется для показа в заголовках диалогов, окон, сообщений </summary>
        /// <value>"Полное Имя" сборки.</value>
        string AssembleFullName { get; }//todo возможно не нужно?

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

        #endregion Public Properties

        #region Public Methods

        //static abstract AppInfo FromAssembly(Assembly assembly);
        //static abstract AppInfo FromCallingAssembly();
        //static abstract AppInfo FromType(Type type);
        //static abstract AppInfo Get(Assembly assembly);
        //static abstract AppInfo Get(Type type);

        /// <summary>Converts to shortstring.</summary>
        /// <returns>shortstring</returns>
        string ToShortString();

        /// <summary>Converts to string.</summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        string ToString();

        /// <summary>Converts to longstring.</summary>
        /// <returns>longstring</returns>
        string ToLongString();

        #endregion Public Methods
    }
}