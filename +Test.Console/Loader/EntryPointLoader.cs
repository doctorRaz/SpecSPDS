/* EntryPoint.cs
 * © Andrey Bushman, 2014
 * Поиск и загрузка версии плагина .NET, ARX или VBA, наиболее пригодной для 
 * текущей версии AutoCAD.
 * http://bushman-andrey.blogspot.ru/2014/06/dll-autocad.html
 */
using dRz.Loader.Cad.Infrastructure.Logging;
using dRz.Loader.Cad.Interfaces;
using dRz.Loader.Cad.Infrastructure;
using dRz.SpecSpds.Test.Services;
using NLog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace dRz.SpecSpds.Test.Loader
{
    /// <summary>
    /// Задачей данного класса является поиск и загрузка в AutoCAD наиболее 
    /// подходящей для него версии плагина.
    /// </summary>
    internal sealed class EntryPoint
    {
        private const string netPluginExtension = ".dll";

        //todo В боевом коде логер включать в методе??? возможен вызов до инициализации

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private IMessageService msg = new MessageService();

        private Version? _version;

        private bool _registered;

        public EntryPoint(Version version)
        {
            _version = version;
        }

        public void Initialize()
        {
            //если нет библиотек или еще какой косяк
            try
            {
                //todo понаблюдать, возможно тормозит нану
                TryRegisterAssemblyResolver();//теоретически упасть не может

                //nlog
                TryInitLoger();

                //load adapter
                if (!TryCadLoading())
                {
                 //   throw new FileNotFoundException($"Не найден подходящий адаптер для {LoaderEnvironment.ProductName}");
                }
            }
            catch (Exception ex) // ошибка инициализации, все развалилось, лог смысла не имеет
            {
                msg.ExceptionMessage($"{LoaderEnvironment.ProductName} не загружен", ex);
            }

            //отписываемся независимо от результата этот аддон больше не нужен
            finally
            {
                try
                {
                    TryUnregisterAssemblyResolver();
                }
                catch { }
            }

        }


        private void TryRegisterAssemblyResolver()
        {
            try
            {
                if (_registered) return;

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                _registered = true;
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage("AssemblyResolver registration failed", ex);
            }
        }

        private void TryUnregisterAssemblyResolver()
        {
            try
            {
                if (!_registered) return;
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

                _registered = false;
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage("AssemblyResolver registration failed", ex);
            }
        }
        private void TryInitLoger()
        {
            /*
            ищет конфиг:
                D:\@Developers\Programmers\!NET\!SpecSPDS\SpecSPDS\bin\Debug\SpecSPDS.nCad.exe.nlog
                D:\@Developers\Programmers\!NET\!SpecSPDS\SpecSPDS\bin\Debug\SpecSPDS.nCad.dll.nlog
                D:\@Developers\Programmers\!NET\!SpecSPDS\SpecSPDS\bin\Debug\NLog.config
                D:\@Developers\Programmers\!NET\!SpecSPDS\SpecSPDS\bin\Debug\drzNLog.dll.nlog
            */
            try
            {
                LogBootstrap.Initialize();
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        private bool TryCadLoading()
        {

            try
            {
                return CadLoading();
            }
            catch 
            {
                throw;
            }
        }

        /// <summary>
        /// Cads the loading.
        /// </summary>
        /// <returns></returns>
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

                Version minVersion = new Version(23, 0);

                Version version = _version;// Application.Version;

                log.Info($"CAD detected: {version.ToString()}");

                string fileFullName = GetType().Assembly.Location;

                FileInfo? targetDllFullName = FindFile(fileFullName, version, minVersion);

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
                        string mesag = $"Загружается адаптер для CAD {version.ToString()}: {targetDllFullName.FullName}";

                        msg.ConsoleMessage(mesag);
                        log.Trace(mesag);

                        //asm = Assembly.LoadFrom(targetDllFullName.FullName);
                    }
                    else
                    {
                        //на случай, если в будущем будет поддержка других типов плагинов, например ARX или VBA
                        throw new NotSupportedException($"Unsupported plugin type: {targetDllFullName.Extension}");
                    }

                    log.Info("Adapter CAD initialized successfully");

                }
                catch (Exception ex)
                {
                    log.Error(ex, ex.Message);
                    throw;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex, ex.Message);
                throw;
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
        private FileInfo? FindFile(string fileFullName, Version expectedVersion,
          Version minVersion)
        {

            if (fileFullName == null)
                throw new ArgumentNullException(nameof(fileFullName), "The fileFullName parameter cannot be null.");

            if (fileFullName.Trim() == string.Empty)
                throw new ArgumentException("The fileFullName parameter cannot be an empty string.", nameof(fileFullName));

            if (expectedVersion < minVersion)
                throw new ArgumentException($"The expectedVersion of {expectedVersion} cannot be less than the minimum allowed version of {minVersion}.", nameof(expectedVersion));

            string? directory = Path.GetDirectoryName(fileFullName);
            if (directory == null)
                throw new ArgumentException("The provided fileFullName does not contain a valid directory path.", nameof(fileFullName));

            string fileName = Path.GetFileNameWithoutExtension(fileFullName);

            int major = expectedVersion.Major;
            int minor = expectedVersion.Minor;

            while (true)
            {
                string coreString_ = $"{major}.{minor}";
                string targetFileName_ = $"{fileName}.{coreString_}{netPluginExtension}";

                string foundPath = GetFilesOfDir(directory, true, targetFileName_);

                if (!string.IsNullOrEmpty(foundPath) && File.Exists(foundPath))
                {
                    return new FileInfo(foundPath);
                }

                // Понижение версии
                if (minor == 0)
                {
                    minor = 5;
                    major--;
                }
                else
                {
                    minor--;
                }

                Version currentVersion = new Version(major, minor);

                if (currentVersion < minVersion)
                    break;
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
                string messag = $"Error searching files in {path}";

                log.Error(ex,messag);   
                msg.ExceptionMessage(messag, ex);
                
                return string.Empty;
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

                string fullPath = GetFilesOfDir(_assemblyDirectory, true, dllName);

                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    return Assembly.LoadFile(fullPath);
                }
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage("Failed to resolve assembly", ex);
            }

            return null;
        }

        public void Terminate()
        {
            try
            {
                TryTerminate();
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex);
            }
        }

        void TryTerminate()
        {
            try
            {
                log.Info("LogManager.Shutdown");
                LogManager.Shutdown();
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex);
            }

        }
    }
}