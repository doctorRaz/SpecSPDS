using System;
using System.IO;
using SharpSevenZip;

namespace dRz.Test.SevenZipExtractor
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            Console.WriteLine("Тест SharpSevenZip");
            Console.WriteLine($"Разрядность процесса: {(IntPtr.Size == 8 ? "x64" : "x86")}");

            string archivePath = args.Length > 0
                ? args[0]
                : @"d:\@Developers\Programmers\!NET\HTTP_TEST\mandatory\SpecSPDS_1.2.9778.19120-protected.7z";//ReadRequiredValue("Путь к архиву 7z: ");

            string outputPath = args.Length > 1
                ? args[1]
                : @"e:\TEMP\dRz\Updater\test";// Path.Combine(Path.GetTempPath(), "SharpSevenZip-Test");

            Console.Write("Пароль архива: ");
            string password = "SpecSPDS";// ReadPassword();

            try
            {
                if (!File.Exists(archivePath))
                {
                    Console.Error.WriteLine($"Архив не найден: {archivePath}");
                    return 2;
                }

                Console.WriteLine($"Архив: {archivePath}");
                Console.WriteLine($"Каталог распаковки: {outputPath}");

                using (SharpSevenZipExtractor archive = new SharpSevenZipExtractor(archivePath, password))
                {
                    // Пароль передаётся при создании extractor, поэтому библиотека
                    // может открыть архив и прочитать зашифрованные заголовки (-mhe=on).
                    Console.WriteLine("Чтение списка файлов...");
                    foreach (var entry in archive.ArchiveFileData)
                    {
                        Console.WriteLine(
                            $"{(entry.IsDirectory ? "[DIR] " : "[FILE]")} {entry.FileName} ({entry.Size} байт)");
                    }

                    Console.WriteLine("Распаковка...");
                    Directory.CreateDirectory(outputPath);
                    archive.ExtractArchive(outputPath);
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

            finally
            {                
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey(intercept: true);
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

        private static string ReadPassword()
        {
            var password = new System.Text.StringBuilder();

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    return password.ToString();
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (password.Length > 0)
                    {
                        password.Length--;
                        Console.Write("\b \b");
                    }

                    continue;
                }

                if (!char.IsControl(key.KeyChar))
                {
                    password.Append(key.KeyChar);
                    Console.Write("*");
                }
            }
        }
    }
}
