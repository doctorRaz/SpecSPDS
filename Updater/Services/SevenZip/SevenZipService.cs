using System.IO;
using SharpSevenZip;

namespace dRz.Updater.Services.SevenZip
{
    /// <summary>
    /// Сервис распаковки архивов 7z.
    /// Поддерживает обычные и защищённые паролем архивы,
    /// включая архивы с зашифрованными заголовками.
    /// </summary>
    public sealed class SevenZipService
    {
        /// <summary>
        /// Распаковывает архив 7z в указанный каталог.
        /// </summary>
        /// <param name="archivePath">Путь к архиву.</param>
        /// <param name="destination">
        /// Каталог, в который будут распакованы файлы.
        /// Если каталог не существует, он будет создан.
        /// </param>
        /// <param name="password">
        /// Пароль архива. Если <see langword="null"/> или пустая строка,
        /// считается, что архив не защищён паролем.
        /// </param>
        /// <returns>
        /// Код завершения операции <see cref="SevenZipExitCode"/>.
        /// Значение <see cref="SevenZipExitCode.Success"/> означает,
        /// что распаковка завершилась без исключения.
        /// </returns>
        /// <exception cref="FileNotFoundException">
        /// Архив не найден.
        /// </exception>
        public SevenZipExitCode Extract(
            string archivePath,
            string destination,
            string? password = null)
        {
            if (!File.Exists(archivePath))
            {
                throw new FileNotFoundException(
                    "Архив не найден",
                    archivePath);
            }

            Directory.CreateDirectory(destination);

            // SharpSevenZip передаёт пароль при открытии архива и поддерживает
            // чтение зашифрованных заголовков (-mhe=on).
            using SharpSevenZipExtractor archive = new SharpSevenZipExtractor(
                archivePath,
                password);

            archive.ExtractArchive(destination);

            return SevenZipExitCode.Success;
        }
    }
}