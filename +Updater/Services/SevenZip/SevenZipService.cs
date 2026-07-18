using System.Diagnostics;
using System.IO;

namespace drz.Updater.Services.SevenZip
{
    /// <summary>
    /// Сервис работы с архивами 7z через консольный модуль 7-Zip.
    /// Поддерживает создание, проверку и распаковку архивов
    /// с использованием пароля или без него.
    /// </summary>
    public sealed class SevenZipService
    {
        private readonly string _sevenZipPath;

        /// <summary>
        /// Создает экземпляр сервиса 7-Zip.
        /// </summary>
        /// <param name="sevenZipPath">
        /// Путь к 7z.exe. Если не указан, используется корень аддона (в кад может быть неверным).
        /// </param>
        public SevenZipService(string sevenZipPath = "")
        {

            string sevenZipName = "7z.exe";
            _sevenZipPath = Path.Combine(sevenZipPath, sevenZipName);
        }

        /// <summary>
        /// Проверяет целостность архива 7z.
        /// Поддерживает проверку обычных и защищённых паролем архивов.
        /// </summary>
        /// <param name="archivePath">
        /// Путь к проверяемому архиву.
        /// </param>
        /// <param name="password">
        /// Пароль архива. Если <see langword="null"/> или пустая строка,
        /// считается, что архив не защищён паролем.
        /// </param>
        /// <returns>
        /// Код завершения операции <see cref="SevenZipExitCode"/>.
        /// Значение <see cref="SevenZipExitCode.Success"/> означает,
        /// что архив успешно прошёл проверку.
        /// </returns>
        /// <exception cref="FileNotFoundException">
        /// Архив не найден.
        /// </exception>
        public SevenZipExitCode TestArchive(
            string archivePath,
            string? password = null)
        {
            if (!File.Exists(archivePath))
                throw new FileNotFoundException(
                    "Архив не найден",
                    archivePath);

            string arguments =
                $"t \"{archivePath}\" -y";

            AddPassword(ref arguments, password);

            return Execute(arguments);
        }

        /// <summary>
        /// Распаковывает архив 7z в указанный каталог.
        /// Поддерживает распаковку обычных и защищённых паролем архивов.
        /// </summary>
        /// <param name="archivePath">
        /// Путь к архиву.
        /// </param>
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
        /// что архив успешно распакован.
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
                throw new FileNotFoundException(
                    "Архив не найден",
                    archivePath);

            Directory.CreateDirectory(destination);

            string arguments =
                $"x \"{archivePath}\" -o\"{destination}\" -y";

            AddPassword(ref arguments, password);

            return Execute(arguments);
        }

        /// <summary>
        /// Создает архив формата 7z из содержимого указанного каталога.
        /// При указании пароля архив защищается паролем, а также
        /// включается шифрование заголовков архива.
        /// </summary>
        /// <param name="archivePath">
        /// Путь создаваемого архива.
        /// </param>
        /// <param name="sourceDirectory">
        /// Каталог, содержимое которого будет помещено в архив.
        /// </param>
        /// <param name="password">
        /// Пароль архива. Если <see langword="null"/> или пустая строка,
        /// архив создается без защиты паролем.
        /// </param>
        /// <param name="compressionLevel">
        /// Уровень сжатия архива.
        /// </param>
        /// <returns>
        /// Код завершения операции <see cref="SevenZipExitCode"/>.
        /// Значение <see cref="SevenZipExitCode.Success"/> означает,
        /// что архив успешно создан.
        /// </returns>
        /// <exception cref="DirectoryNotFoundException">
        /// Исходный каталог не найден.
        /// </exception>
        public SevenZipExitCode CreateArchive(
            string archivePath,
            string sourceDirectory,
            string? password = null,
            SevenZipCompressionLevel compressionLevel = SevenZipCompressionLevel.Ultra)
        {
            if (!Directory.Exists(sourceDirectory))
                throw new DirectoryNotFoundException(
                    sourceDirectory);

            string arguments =
                $"a \"{archivePath}\" \"{sourceDirectory}\\*\" -t7z  -mx={(int)compressionLevel} -y";

            AddArchiveEncryption(ref arguments, password);

            return Execute(arguments);
        }

        /// <summary>
        /// Выполняет команду 7z.exe и ожидает завершения процесса.
        ///
        /// Метод запускает внешний процесс 7-Zip с указанными параметрами,
        /// перенаправляет стандартный вывод и поток ошибок, ожидает завершения
        /// процесса и возвращает код завершения, преобразованный в
        /// <see cref="SevenZipExitCode"/>.
        ///
        /// Для успешного выполнения 7z.exe должен быть доступен по пути,
        /// указанному в поле <c>_sevenZipPath</c>.
        /// </summary>
        /// <param name="arguments">
        /// Аргументы командной строки, передаваемые 7z.exe.
        /// Например:
        /// <c>a archive.7z folder\* -mx=9</c>.
        /// </param>
        /// <returns>
        /// Код завершения операции.
        /// Значение <see cref="SevenZipExitCode.Success"/> означает,
        /// что команда выполнена успешно.
        /// </returns>
        /// <exception cref="Win32Exception">
        /// Не удалось запустить процесс 7z.exe
        /// (например, файл не найден или отсутствуют права доступа).
        /// </exception>
        private SevenZipExitCode Execute(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                // Исполняемый файл 7-Zip.
                FileName = _sevenZipPath,

                // Аргументы командной строки.
                Arguments = arguments,

                // Процесс запускается напрямую, без использования
                // системной оболочки (cmd.exe / Explorer).
                UseShellExecute = false,

                // Не создавать консольное окно.
                CreateNoWindow = true,

                // Перенаправить стандартный вывод.
                // Это позволяет при необходимости анализировать сообщения 7-Zip
                // (например, для отображения прогресса или ведения журнала).
                RedirectStandardOutput = true,

                // Перенаправить поток ошибок.
                RedirectStandardError = true
            };

            using var process = new Process
            {
                StartInfo = psi
            };

            process.Start();

            // Читаем вывод, чтобы избежать блокировки буфера процесса
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            /*
                         * Коды возврата 7-Zip:
                         *
                         * 0  - успешно
                         * 1  - предупреждение
                         * 2  - ошибка
                         * 7  - ошибка параметров
                         * 8  - недостаточно памяти
                         * 255- отмена пользователем
                         */

            return (SevenZipExitCode)process.ExitCode;
        }

        /// <summary>
        /// Добавляет параметр пароля в аргументы командной строки 7-Zip.
        /// Если пароль не указан, аргументы не изменяются.
        /// </summary>
        /// <param name="arguments">
        /// Строка аргументов командной строки, в которую при необходимости
        /// будет добавлен параметр <c>-p</c>.
        /// </param>
        /// <param name="password">
        /// Пароль архива. Если <see langword="null"/> или пустая строка,
        /// параметр пароля не добавляется.
        /// </param>
        private static void AddPassword(
            ref string arguments,
            string? password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                // Добавить параметр пароля.
                arguments += $" -p\"{password}\"";
            }
        }

        /// <summary>
        /// Добавляет параметры защиты архива в аргументы командной строки 7-Zip.
        /// Если указан пароль, включается защита архива паролем и шифрование
        /// заголовков архива.
        /// </summary>
        /// <param name="arguments">
        /// Строка аргументов командной строки, в которую при необходимости
        /// будут добавлены параметры защиты.
        /// </param>
        /// <param name="password">
        /// Пароль архива. Если <see langword="null"/> или пустая строка,
        /// параметры защиты не добавляются.
        /// </param>
        private static void AddArchiveEncryption(
            ref string arguments,
            string? password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                // Добавить пароль архива.
                arguments += $" -p\"{password}\"";

                // Шифровать заголовки архива (имена файлов и структуру каталогов).
                arguments += " -mhe=on";
            }
        }

    }
}



