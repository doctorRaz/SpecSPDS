using dRz.SpecSPDS.nCad.Interfaces;
using dRz.SpecSPDS.nCad.Services;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace dRz.SpecSPDS.nCad.AssemblyResolve
{
    internal class AssemblyResolver
    {
        private IMessageService msg = new MessageService();

        /// <summary>
        /// Подписка на событие AssemblyResolve 
        /// </summary>
        internal bool Register()
        {

            //https://adn-cis.org/forum/index.php?topic=10332.msg47741#msg47741
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                return true;
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex, "AssemblyResolver registration failed\n");
                return false;
            }
        }

        /// <summary>
        /// Обработчик события AssemblyResolve
        /// </summary>
        private Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            //GPT
            try
            {
                string dllName = args.Name.Split(',')[0] + ".dll";

                string fullPath = GetFullAssemblyPath(dllName);

                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    return Assembly.LoadFile(fullPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to resolve assembly {args.Name}: {ex.Message}");
            }

            return null;

            //by dRz on 24.02.2026 at 15:39
            /*
            string path = string.Empty;

            if (args.Name.IndexOf(",") > -1)
            {
                path = AssemblFulNameDll(args.Name.Substring(0, args.Name.IndexOf(",")) + ".dll");
            }
            else
            {
                path = AssemblFulNameDll(args.Name + ".dll");
            }

            if (path != string.Empty)
            {
                return Assembly.LoadFile(path);
            }
            return null;
            */
        }

        /// <summary>
        /// Получить полный путь к DLL по имени
        /// </summary>
        private string GetFullAssemblyPath(string dllName)
        {
            //GPT
            var files = GetFilesOfDir(_assemblyDirectory, true, dllName);
            return files.FirstOrDefault() ?? string.Empty;
        }



        //by dRz on 24.02.2026 at 15:43
        /*
        /// <summary>
        /// Получить полный путь к файлу загружаемой dll
        /// </summary>
        /// <param name="sDllName">имя dll</param>
        /// <returns>Путь и имя к библиотеке</returns>
     
        string AssemblFulNameDll(string sDllName)
        {
            string asmPath = String.Empty;
            string sAsmFileFullName = _assemblyDirectory;//каталог DLL
                                                         // string sAsmFileFullName = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
            string path = Directory.GetParent(sAsmFileFullName)?.FullName;


            string[] asmPaths = GetFilesOfDir(path, true, sDllName);
            if (asmPaths.Length > 0)
            {
                string asmPathTmp = asmPaths[0];//хватаем первую в списке

                if (File.Exists(asmPathTmp)) asmPath = asmPathTmp;//присваиваем ее значение
            }
            return asmPath;
        }
        
        */

        /// <summary>Получить список путей фалов в директории</summary>
        /// <param name="sPath">Директория с файлами</param>
        /// <param name="withSubfolders">Учитывать поддиректории</param>
        /// <param name="serchPatern">Маска поиска</param>
        /// <returns>Пути к файлам</returns>
        internal /*static*/ string[] GetFilesOfDir(string path, bool withSubfolders, string serchPatern = "*.dll")
        {
            try
            {
                return Directory.GetFiles(path,
                                        serchPatern,
                                        (withSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                                        );
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex, $"Error searching files in {path}\n");
               return Array.Empty<string>();
                // return new string[0];
            }
        }

        /// <summary>
        /// Полный путь к текущей сборке
        /// </summary>
        private readonly string _assemblyDirectory = Path.GetDirectoryName(typeof(AssemblyResolver).Assembly.Location) ?? string.Empty;

    }
}
