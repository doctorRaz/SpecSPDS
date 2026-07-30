using System;
using System.IO;
using System.Linq;

internal static class FileMover
{
    /// <summary>
    /// Перемещает файлы из корневого каталога в подкаталоги,
    /// если в подкаталоге уже существует файл с таким же именем.
    /// </summary>
    /// <param name="rootDirectory">Корневой каталог.</param>
    internal static void MoveDuplicateFiles(string rootDirectory)
    {
        if (!Directory.Exists(rootDirectory))
            throw new DirectoryNotFoundException(rootDirectory);
        int count=0;

        // Получаем все файлы только из корневого каталога.
        string[] rootFiles = Directory.GetFiles(rootDirectory, "*.md");

        int rootTotal=rootFiles.Length; 
        
        // Получаем все файлы из всех подкаталогов.
        string[] subFiles = Directory.GetFiles(rootDirectory, "*", SearchOption.AllDirectories);

        foreach (string rootFile in rootFiles)
        {
            string fileName = Path.GetFileName(rootFile);

            // Ищем одноименный файл в подкаталогах.
            foreach (string subDir in Directory.GetDirectories(rootDirectory, "*", SearchOption.AllDirectories))
            {
                // Имя файла без пути.
                string targetFile = Path.Combine(subDir, fileName);

                if (File.Exists(targetFile))
                {
                    Console.WriteLine($"Перемещение:");
                    Console.WriteLine($"   {rootFile}");
                    Console.WriteLine($"-> {targetFile}");
                    try
                    {
                        // Перемещаем с заменой существующего файла.
                        //todo проверить по дате изменения
                        File.Move(rootFile, targetFile, overwrite: true);

                        Console.WriteLine($"\tOk");
                        count ++;
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\tERROR");
                        Console.WriteLine(ex.Message);
                        //ищем дальше
                    }
                }
            }
        }
        Console.WriteLine($"All root {rootTotal} files\nmoved-> {count} files");
    
    }
}