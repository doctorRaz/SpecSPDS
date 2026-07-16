using System;
using System.IO;
using System.Text.RegularExpressions;

namespace drz.Cleaner.Infrastructure
{
    /// <summary>
    /// чистка архивных файлов
    /// </summary>
    public class BackupCleaner

    {
        #region Private Fields

        /// <summary>
        /// Регулярное выражение для файлов вида "Имя(число).Расширение"
        /// </summary>
        //private static Regex digitsInBracketsAtEnd = new Regex(@"\(\d+\)\.\w+$");
        private static readonly Regex digitsInBracketsAtEnd =
                                new(@"\(\d+\)\.[^.]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        #endregion Private Fields

        #region Public Methods

        /// <summary>
        /// чистилка *.bak и *****(111).****
        /// </summary>
        /// <param name="directoryPath"></param>
        public static void Clean(string directoryPath)
        {
            //digitsInBracketsAtEnd = new Regex(@"\(\d+\)$");
            //digitsInBracketsAtEnd = new Regex(@"\(\d+\)\.\*");
            try
            {
                // EnumerateFiles лениво проходит файлы рекурсивно
                foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*.*", SearchOption.AllDirectories))
                {
                    string fileName = Path.GetFileName(filePath);
                    //string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string extension = Path.GetExtension(filePath);

                    if (extension.Equals(".bak", StringComparison.OrdinalIgnoreCase) || digitsInBracketsAtEnd.IsMatch(fileName))
                    {
                        try
                        {
                            File.Delete(filePath);
                        }
                        catch { }
                    }
                }
            }
            catch { }
        }

        #endregion Public Methods
    }
}