using Abstractions.Services;

namespace Abstractions.Enums
{
    /// <summary> Тиы возможных реализаций <see cref="IMessageService">IMessageService</see> </summary>
    public enum MessageServiceType
    {
        /// <summary> Выводить все сообщения в окна типа MessageBox </summary>
        Window,
        /// <summary>
        /// Выводить все сообщения в консоль nanoCAD
        /// </summary>
        CommandLine,
        /// <summary>
        /// Выводить все сообщения в стандартную Console
        /// </summary>
        Console,
    }
}
