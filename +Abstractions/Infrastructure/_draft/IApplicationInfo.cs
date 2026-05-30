using System;

namespace drz.Abstractions.Infrastructure
{
    /// <summary> Информация о сборке от Крыс</summary>
    public interface IApplicationInfo
    {
        #region Public Properties

        /// <summary> Текущая версия сборки </summary>
        Version AssemblyVersion { get; }

        /// <summary> "Полное Имя" сборки, используется для показа в заголовках диалогов, окон, сообщений </summary>
        string AssembleFullName { get; }

        /// <summary> Полный путь к основной загруженной сборке в *CAD </summary>
        string AssemblyPath { get; }

        /// <summary> Префикс заголовка окна </summary>
        string TitlePrefix { get; }

        #endregion Public Properties
    }
}