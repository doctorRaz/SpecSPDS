using System;
using System.IO;

namespace drz.MoveDuplicateFiles
{
    internal class Start
    {
        [STAThread]
        private static void Main(string[] args)
        {
            string def = "";
#if DEBUG1
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debugger.Launch();
            }
            //   def = @"d:\@Developers\В работе\Reminder\SpecSPDS";
#endif

            string root = GetRootDirectory(args, def);

            if (!string.IsNullOrWhiteSpace(root))
            {
                FileMover.MoveDuplicateFiles(root);
            }
            else
            {
                Console.WriteLine("Не задан путь к Root");
            }

            Console.WriteLine("Press eny key...");
            Console.ReadKey();
        }

        private static string GetRootDirectory(string[] args, string def = "")
        {
            //0. проверяем путь по умолчанию (отладка)
            if (!string.IsNullOrWhiteSpace(def))
            {
                if (Directory.Exists(def))
                    return def;
            }

            // 1. Проверяем аргумент командной строки.
            if (args.Length > 0)
            {
                try
                {
                    string path = Path.GetFullPath(args[0]);

                    if (Directory.Exists(path))
                        return path;
                }
                catch
                {
                    // Некорректный путь.
                }
            }

            // 2. Если рабочий каталог существует, используем его.
            string currentDirectory = Directory.GetCurrentDirectory();
            if (Directory.Exists(currentDirectory))
                return currentDirectory;

            // 3. Последний вариант — каталог программы.
            return string.Empty;
        }
    }
}