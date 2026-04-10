using System;

namespace Abstractions.Infrastructure
{
    /// <summary> Информация о сборке </summary>
    public interface IApplicationInfo
    {
        /// <summary> Текущая версия сборки </summary>
        Version Version { get; }
        /// <summary> Полный путь к основной загруженной сборке в *CAD </summary>
        string Path { get; }
        /// <summary> "Имя" сборки, используется для показа в заголовках диалогов, окон, сообщений </summary>
        string Name { get; }
        /// <summary> Указатель на окно CAD </summary>
        IntPtr CadWindowHandle { get; }
        /// <summary> Префикс заголовка окна </summary>
        string TitlePrefix { get; }
    }
}
