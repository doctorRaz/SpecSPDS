/* EntryPoint.cs
 * © Andrey Bushman, 2014
 * Поиск и загрузка версии плагина .NET, ARX или VBA, наиболее пригодной для 
 * текущей версии AutoCAD.
 * http://bushman-andrey.blogspot.ru/2014/06/dll-autocad.html
 */


using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using dRz.Loader.Cad.Infrastructure;
using System.Linq;
using dRz.Loader.Cad.Interfaces;
using dRz.Loader.Cad.Infrastructure.Logging;
using dRz.Loader.Cad;
using dRz.Loader.Cad.Services;




#if AC

using Rtm = Autodesk.AutoCAD.Runtime;

#elif NC

using HostMgd.ApplicationServices;
using Rtm = Teigha.Runtime;

#endif

[assembly: Rtm.ExtensionApplication(typeof(EntryPoint))]

namespace dRz.Loader.Cad
{

    /// <summary>
    /// Задачей данного класса является поиск и загрузка в AutoCAD наиболее 
    /// подходящей для него версии плагина.
    /// </summary>
    internal sealed class EntryPoint : Rtm.IExtensionApplication
    {
        private const string netPluginExtension = ".dll";
        private static readonly string[] extensions = new string[] { ".arx", ".dvb" };
        private static readonly string[] methodNames = new string[] { "LoadArx", "LoadDVB" };

        //возможен вызов до инициализации??
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private IMessageService msg = new MessageService();


        /// <summary>
        /// Код этого метода будет запущен на исполнение при загрузке сборки в 
        /// AutoCAD. В результате его работы происходит попытка найти и загрузить в
        /// AutoCAD наиболее подходящую версию плагина из имеющихся в наличии.
        /// </summary>
#if DEBUG
        [Rtm.CommandMethod("инитЛД")]
        [Description("ручной инит загрузчика")]
#endif
        public void Initialize()
        {
            //если нет библиотек или еще какой косяк
            try
            {
                //todo понаблюдать, возможно тормозит нану
                TryRegisterAssemblyResolver();

                //nlog
                //todo если false return и message
                TryInitLoger();

                //load adapter
                if (!TryCadLoading())
                {
                    //todo messagg  Error
                    msg.ConsoleMessage($"\n-= [{nameof(Initialize)}] сбой загрузки. {LoaderEnvironment.ProductName} не загружен =-\n");
                }
            }
            catch (Exception ex) // ошибка инициализации, все развалилось, лог смысла не имеет
            {
                msg.ExceptionMessage(ex);
            }
        }

        private void TryRegisterAssemblyResolver()
        {
            //https://adn-cis.org/forum/index.php?topic=10332.msg47741#msg47741
            //https://autolisp.ru/2025/01/27/loading-another-assemblies/
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex, "AssemblyResolver registration failed");
            }
        }

        private bool TryInitLoger()
        {
            try
            {
                    LogBootstrap.Initialize();
                return true;
                 
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex);
                return false;
            }
        }

        /// <summary>
        /// Загрузка адаптера
        /// </summary>
        /// <returns>успех</returns>
        private bool TryCadLoading()
        {
            try
            {
                return CadLoading();
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex);
                log.Error(ex.Message, ex);
                return false;
            }
        }
        private bool CadLoading()
        {
            try
            {
                // Для начала извлекаем информацию о текущей версии AutoCAD и ищем
                // соответствующую ей версию файла. Имя такого файла должно 
                // формироваться по правилу: 
                //    ИмяТекущейСборки.Major.Minor[x86|x64].(dll|arx|dvb).
                // Где <Major> и <Minor> - это значения одноимённых свойств объекта 
                // Version, полученного из Application.Version.
                Version version = Application.Version;

                log.Info($"CAD detected: {version.ToString()}");

                string fileFullName = GetType().Assembly.Location;

                Version minVersion = new Version(23, 0);

                FileInfo targetDllFullName = FindFile(fileFullName, version, minVersion);

                if (targetDllFullName == null)
                {
                    string mesag = $"Не найден подходящий адаптер для CAD {version.ToString()}";

                    log.Error($"{mesag}");

                    msg.ConsoleMessage($"{mesag}");

                    return false;
                }

                log.Info($"CadLoading CAD adapter: {targetDllFullName}");

                // Если найден файл, соответствующий нашей версии AutoCAD, то 
                // загружаем его.
                Assembly? asm = null;
                try
                {
                    if (targetDllFullName.Extension.Equals(netPluginExtension, StringComparison.CurrentCultureIgnoreCase))
                    {
                        asm = Assembly.LoadFrom(targetDllFullName.FullName);
                    }
                    else
                    {
                        int index = Array.IndexOf(extensions, targetDllFullName.Extension);

                        if (index >= 0)
                        {
                            object application = Application.AcadApplication;

                            application.GetType().InvokeMember(methodNames[index], BindingFlags
                              .InvokeMethod, null, application, new object[] {
                            targetDllFullName.FullName });
                        }
                    }

                    log.Info("Adapter CAD initialized successfully");

                }
                catch (Exception ex)
                {
                    msg.ExceptionMessage(ex);
                    log.Error(ex.Message, ex);
                    return false;
                }
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex);
                log.Error(ex, ex.Message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Получить имя наиболее подходящего файла, для его последующей загрузки в
        /// AutoCAD. Если такой файл не будет найден, то возвращается null.
        /// </summary>
        /// <param name="fileFullName">"Базовое" имя файла, т.е. полное имя 
        /// файла без указания в нём версий ядра и разрядности платформы.</param>
        /// <param name="expectedVersion">Версия AutoCAD, для которой следует 
        /// выполнить поиск соответствующей версии файла.</param>
        /// <param name="minVersion">Наименьшая версия AutoCAD, ниже которой не 
        /// следует выполнять поиск.</param>
        /// <returns>Возвращается FileInfo наиболее подходящего файла, для его 
        /// последующей загрузки в AutoCAD. Если такой файл не будет найден, то 
        /// возвращается null.</returns>
        private FileInfo FindFile(string fileFullName, Version expectedVersion,
          Version minVersion)
        {

            if (fileFullName == null)
                throw new ArgumentNullException("fileFullName");

            if (fileFullName.Trim() == string.Empty)
                throw new ArgumentException("fileFullName.Trim() == String.Empty");

            if (expectedVersion < minVersion)
                throw new ArgumentException("expectedVersion < minVersion");

            int major = expectedVersion.Major;

            int minor = expectedVersion.Minor;

            string? directory = Path.GetDirectoryName(fileFullName);

            string fileName = Path.GetFileNameWithoutExtension(fileFullName);

            string coreString = string.Format("{0}.{1}", major.ToString(), minor.ToString());

            string subDirectoryName = "R" + coreString;
            string subDirectoryName_xPlatform = subDirectoryName + (IntPtr.Size == 4 ? "x86" : "x64");

            string targetFileName = string.Empty;
            string targetFileName_xPlatform = string.Empty;
            string targetFileFullName = string.Empty;
            string targetFileFullName_xPlatform = string.Empty;

            List<string> items = new List<string>(extensions);
            items.Insert(0, netPluginExtension);

            string name = string.Empty;

            foreach (string extension in items)
            {

                targetFileName = string.Format("{0}.{1}{2}", fileName, coreString, extension);
                targetFileName_xPlatform = string.Format("{0}.{1}{2}{3}", fileName, coreString, IntPtr.Size == 4 ? "x86" : "x64", extension);

                // Сначала выполняем поиск в текущем каталоге
                targetFileFullName = Path.Combine(directory, targetFileName);
                if (File.Exists(targetFileFullName))
                {
                    name = targetFileFullName;
                    break;
                }
                targetFileFullName_xPlatform = Path.Combine(directory, targetFileName_xPlatform);

                if (File.Exists(targetFileFullName_xPlatform))
                {
                    name = targetFileFullName_xPlatform;
                    break;
                }

                // Если в текущем каталоге подходящий файл не найден, то продолжаем
                // поиск по соответствующим подкаталогам
                targetFileFullName = directory + "\\" + subDirectoryName + "\\" + targetFileName;
                if (File.Exists(targetFileFullName))
                {
                    name = targetFileFullName;
                    break;
                }

                targetFileFullName_xPlatform = directory + "\\" + subDirectoryName_xPlatform + "\\" + targetFileName_xPlatform;

                if (File.Exists(targetFileFullName_xPlatform))
                {
                    name = targetFileFullName_xPlatform;
                    break;
                }
            }

            // Если найден файл, соответствующий нашей версии AutoCAD, то возвращаем 
            // соответствующий ему объект FileInfo.
            if (File.Exists(name))
            {
                return new FileInfo(name);
            }
            // Если соответствия не найдено, то продолжаем поиск, последовательно 
            // проверяя наличие подходящего файла для более ранних версий AutoCAD
            else
            {
                if (minor == 0)
                {
                    minor = 3;
                    --major;
                }
                else
                {
                    --minor;
                }

                Version version = new Version(major, minor);

                if (version < minVersion)
                    return null;

                FileInfo file = FindFile(fileFullName, new Version(major, minor), minVersion);
                return file;
            }
        }

        /// <summary>
        /// Обработчик события AssemblyResolve
        /// </summary>
        private Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                string dllName = $"{args.Name.Split(',')[0]}.dll";

                // Полный путь к текущей сборке
                string _assemblyDirectory = Path.GetDirectoryName(typeof(EntryPoint).Assembly.Location) ?? string.Empty;

                string fullPath  = GetFilesOfDir(_assemblyDirectory, true, dllName);
                
                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    return Assembly.LoadFile(fullPath);
                }
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex,"Failed to resolve assembly");
            }

            return null;
        }

        /// <summary>Получить список путей фалов в директории</summary>
        /// <param name="path">Директория с файлами</param>
        /// <param name="withSubfolders">Учитывать поддиректории</param>
        /// <param name="serchPatern">Маска поиска</param>
        /// <returns>Пути к файлам</returns>
        private string GetFilesOfDir(string path, bool withSubfolders, string serchPatern = "*.dll")
        {
            try
            {
                string[] files = Directory.GetFiles(path,
                                          serchPatern,
                                          withSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
                return files.FirstOrDefault() ?? string.Empty;
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex, $"Error searching files in {path}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Код данного метода выполняется при завершении работы AutoCAD.
        /// </summary>
        public void Terminate()
        {
            try
            {
                log.Info("LogManager.Shutdown");
                LogManager.Shutdown();
            }
            catch { }
        }
    }
}