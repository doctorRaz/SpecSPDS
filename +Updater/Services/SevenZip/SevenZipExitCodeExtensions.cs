using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drz.Updater.Services.SevenZip
{
    /// <summary>
    /// Методы расширения для <see cref="SevenZipExitCode"/>.
    /// </summary>
    public static class SevenZipExitCodeExtensions
    {
        /// <summary>
        /// Возвращает текстовое описание кода завершения 7-Zip.
        /// </summary>
        /// <param name="code">
        /// Код завершения операции.
        /// </param>
        /// <returns>
        /// Текстовое описание кода завершения.
        /// Для неизвестных кодов возвращается сообщение с числовым значением кода.
        /// </returns>
        public static string GetDescription(this SevenZipExitCode code)
        {
            return code switch
            {
                SevenZipExitCode.Success => "Операция успешно завершена.",
                SevenZipExitCode.Warning => "Операция выполнена с предупреждениями.",
                SevenZipExitCode.Error => "Фатальная ошибка.",
                SevenZipExitCode.CommandLineError => "Ошибка параметров командной строки.",
                SevenZipExitCode.OutOfMemory => "Недостаточно памяти.",
                SevenZipExitCode.UserCanceled => "Операция отменена пользователем.",
                _ => $"Неизвестный код завершения ({(int)code})."
            };
        }
    }
}
