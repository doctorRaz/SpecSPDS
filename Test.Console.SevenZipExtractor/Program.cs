using System;
using System.IO;
using SevenZipExtractor;

namespace dRz.Test.Console.SevenZipExtractor
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            Console.WriteLine("Тест SevenZipExtractor");
            Console.WriteLine($"Разрядность процесса: {(IntPtr.Size == 8 ? "x64" : "x86")}");

            string archivePath = args.Length > 0
                ? args[0]
                : ReadRequiredValue("Путь к архиву 7z: ");

            string outputPath = args.Length > 1
                ? args[1]
                : Path.Combine(Path.GetTempPath(), "SevenZipExtractor-Test");

            Console.Write("Пароль архива (ввод отображается): ");
            string password = Console.ReadLine() ?? string.Empty;

            try
            {
                if (!File.Exists(archivePath))
                {
                    Console.Error.WriteLine($"Архив не найден: {archivePath}");
                    return 2;
                }

                Console.WriteLine($"Архив: {archivePath}");
                Console.WriteLine($"Каталог распаковки: {outputPath}");

                using (var archive = new ArchiveFile(archivePath))
                {
                    // Эта операция принципиальна для теста архивов с -mhe=on:
                    // библиотека должна прочитать имена файлов до начала распаковки.
                    Console.WriteLine("Чтение списка файлов...");
                    foreach (Entry entry in archive.Entries)
                    {
                        Console.WriteLine($"{(entry.IsFolder ? "[DIR] " : "[FILE]")} {entry.FileName}");
                    }

                    Console.WriteLine("Распаковка...");
                    archive.Extract(outputPath, overwrite: true, password: password);
                }

                Console.WriteLine("Распаковка завершена.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Тест завершился ошибкой:");
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static string ReadRequiredValue(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                string value = Console.ReadLine() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }

                Console.WriteLine("Значение не должно быть пустым.");
            }
        }
    }
}
