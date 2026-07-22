# 7z

**User:** dRz  
**Created:** 7/18/2026 12:44:31  
**Updated:** 7/18/2026 12:58:56  
**Exported:** 7/18/2026 13:16:42  
**Link:** [https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a5b4ada-21a4-83eb-856b-fef85e02ca4c](https://chatgpt.com/g/g-p-6a5b3001db6c8191adf643193a601a8b-updater/c/6a5b4ada-21a4-83eb-856b-fef85e02ca4c)  

## Prompt:
18.07.2026, 12:44:31

Напиши класс для проверки архива 7z
Шифрованного и нет

## Response:
18.07.2026, 12:44:31

Для проверки 7z-архива лучше использовать `7z.exe` через `Process`, так как он умеет проверять целостность как обычных, так и зашифрованных архивов.

Команда:
- без пароля: `7z t archive.7z`
- с паролем: `7z t archive.7z -pPASSWORD`

Класс:

```csharp
using System;
using System.Diagnostics;
using System.IO;

namespace drz.Updater.Infrastructure
{
    public sealed class SevenZipValidator
    {
        private readonly string _sevenZipPath;

        public SevenZipValidator(string sevenZipPath = "7z.exe")
        {
            _sevenZipPath = sevenZipPath;
        }

        public bool TestArchive(string archivePath, string? password = null)
        {
            if (!File.Exists(archivePath))
                throw new FileNotFoundException("Архив не найден", archivePath);

            var psi = new ProcessStartInfo
            {
                FileName = _sevenZipPath,
                Arguments = BuildArguments(archivePath, password),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = new Process
            {
                StartInfo = psi
            };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            // 7z:
            // 0 - OK
            // 1 - предупреждения
            // 2 - ошибка
            // 7 - ошибка командной строки
            // 8 - недостаточно памяти
            // 255 - отмена

            return process.ExitCode == 0;
        }

private static string BuildArguments(string archivePath, string? password)
        {
            var args = $"t \"{archivePath}\" -y";

            if (!string.IsNullOrEmpty(password))
            {
                args += $" -p\"{password}\"";
            }

            return args;
        }
    }
}
```

Использование:

```csharp
var validator = new SevenZipValidator(@"C:\Tools\7z.exe");

// обычный архив
bool ok = validator.TestArchive(@"C:\Temp\update.7z");

// зашифрованный
bool ok2 = validator.TestArchive(
    @"C:\Temp\update.7z",
    "SpecSPDS");
```

Для твоего Updater я бы сделал проверку **до распаковки**:

1. Скачать архив во временный файл:
```
update.tmp.7z
```

2. Проверить:
```
7z t update.tmp.7z -pProductName
```

3. Если `ExitCode == 0`:
```
update.tmp.7z -> package.7z
```

4. Только потом распаковка.

Так ты не начнёшь замену DLL из битого или недокачанного архива.

## Prompt:
18.07.2026, 12:49:59

Добавь в класс методы распаковки и упаковки, с паролем и без
Оформи готовым классом с комментариями и описаниями методов

## Response:
18.07.2026, 12:49:59

Для Updater логично объединить работу с 7z в один сервис. Ниже класс, который умеет:

- проверять архив (`TestArchive`)
- распаковывать (`Extract`)
- упаковывать (`CreateArchive`)
- работать с паролем и без пароля
- возвращать `bool` вместо исключений для ошибок 7z
- хранить путь к `7z.exe`
- иметь комментарии XML для IntelliSense

```csharp
using System;
using System.Diagnostics;
using System.IO;

namespace drz.Updater.Infrastructure
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
        /// Путь к 7z.exe. Если не указан, используется поиск из PATH.
        /// </param>
        public SevenZipService(string sevenZipPath = "7z.exe")
        {
            _sevenZipPath = sevenZipPath;
        }

/// <summary>
        /// Проверяет целостность архива 7z.
        /// Работает как с обычными, так и с зашифрованными архивами.
        /// </summary>
        /// <param name="archivePath">Путь к архиву.</param>
        /// <param name="password">
        /// Пароль архива. Если null или пустой — архив без пароля.
        /// </param>
        /// <returns>
        /// true — архив корректный;
        /// false — ошибка проверки.
        /// </returns>
        public bool TestArchive(
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
        /// Распаковывает архив в указанную директорию.
        /// </summary>
        /// <param name="archivePath">Путь к архиву.</param>
        /// <param name="destination">
        /// Каталог назначения.
        /// </param>
        /// <param name="password">
        /// Пароль архива.
        /// </param>
        /// <returns>
        /// true — распаковка успешна;
        /// false — ошибка.
        /// </returns>
        public bool Extract(
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
        /// Создает 7z архив из указанного каталога.
        /// </summary>
        /// <param name="archivePath">
        /// Путь создаваемого архива.
        /// </param>
        /// <param name="sourceDirectory">
        /// Каталог для упаковки.
        /// </param>
        /// <param name="password">
        /// Пароль архива.
        /// </param>
        /// <returns>
        /// true — архив создан;
        /// false — ошибка упаковки.
        /// </returns>
        public bool CreateArchive(
            string archivePath,
            string sourceDirectory,
            string? password = null)
        {
            if (!Directory.Exists(sourceDirectory))
                throw new DirectoryNotFoundException(
                    sourceDirectory);

            string arguments =
                $"a \"{archivePath}\" \"{sourceDirectory}\\*\" -t7z -y";

            AddPassword(ref arguments, password);

            return Execute(arguments);
        }

/// <summary>
        /// Выполняет команду 7z.exe.
        /// </summary>
        private bool Execute(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = _sevenZipPath,
                Arguments = arguments,

                UseShellExecute = false,
                CreateNoWindow = true,

                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

using var process = new Process
            {
                StartInfo = psi
            };

process.Start();

            // Читаем вывод, чтобы избежать блокировки буфера процесса
            _ = process.StandardOutput.ReadToEnd();
            _ = process.StandardError.ReadToEnd();

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

            return process.ExitCode == 0;
        }

/// <summary>
        /// Добавляет параметр пароля в командную строку 7z.
        /// </summary>
        private static void AddPassword(
            ref string arguments,
            string? password)
        {
            if (!string.IsNullOrEmpty(password))
            {
                arguments += $" -p\"{password}\"";
            }
        }
    }
}
```

Пример использования в твоём Updater:

```csharp
var sevenZip = new SevenZipService(
    Path.Combine(AppContext.BaseDirectory, "7z.exe"));

// проверка скачанного пакета
if (!sevenZip.TestArchive(
        "package.7z",
        "SpecSPDS"))
{
    throw new Exception("Архив поврежден");
}

// резервная копия аддона
sevenZip.CreateArchive(
    "backup_1.2.0.7z",
    "Addon",
    "SpecSPDS");

// установка обновления
sevenZip.Extract(
    "package.7z",
    "Addon",
    "SpecSPDS");
```

Для твоей схемы обновления я бы ещё добавил в этот класс:
- `event Action<long,long>? ProgressChanged` через разбор вывода 7z (`-bsp1`);
- `CancellationToken` для остановки скачивания/распаковки;
- `SevenZipResult` вместо `bool`, чтобы получать `ExitCode`, `stdout`, `stderr`.

Для Updater это будет удобнее при диагностике ошибок у пользователей.



---
Powered by [ChatGPT Exporter](https://www.chatgptexporter.com)