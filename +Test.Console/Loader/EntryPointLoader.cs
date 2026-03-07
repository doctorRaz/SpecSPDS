/* EntryPoint.cs
 * © Andrey Bushman, 2014
 * Поиск и загрузка версии плагина .NET, ARX или VBA, наиболее пригодной для 
 * текущей версии AutoCAD.
 * http://bushman-andrey.blogspot.ru/2014/06/dll-autocad.html
 */
using dRz.Loader.Cad.Infrastructure;
using dRz.Loader.Cad.Infrastructure.Logging;
using dRz.Loader.Cad.Interfaces;
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
                //выключать если точно не понадобится
                //если ех оставляем там, продолжаем работу
                TryRegisterAssemblyResolver();//теоретически упасть не может

                //nlog
                // если ехception, поднимаем его сюда, стоп работа
                // пока не сделаю подмену интерфейсов (хотя нужда под вопросом, сборка drzNlog своя!!!!
                // если ех нет хоть и с битым конфигом, работу продолжим, но юзеру о битом конфиге сообщим в msg
           
                if (!LogBootstrap.Init())
                    msg.ConsoleMessage($"[{nameof(LogBootstrap)}.{nameof(Initialize)}]: Ошибка в конфигурации Logger. Загрузка {LoaderEnvironment.ProductName} будет продолжена");

                //грузим адаптер под версию кад, если ex, конец работы, исключения поднимаем сюда, юзеру в msg сообщаем
                CadLoading();

            }
            catch (Exception ex) // ошибка инициализации, все развалилось, лог смысла не имеет
            {
                string message = $"{LoaderEnvironment.ProductName} не загружен!!!" +
                    $"\nСкопируйте это сообщение и отправьте разработчику";

                System.Diagnostics.Trace.WriteLine($"{message}:{ex}");

                msg.ExceptionMessage(message, ex);
            }

            //отписываемся независимо от результата этому аддону подписка  больше не нужен
            finally
            {
                try
                {

                    TryUnregisterAssemblyResolver();
                }
                catch { }
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

                log.Debug($"CAD detected: {version.ToString()}");

                string fileFullName = GetType().Assembly.Location;

                FileInfo? targetDllFullName = FindFile(fileFullName, version, minVersion);

                if (targetDllFullName == null)
                {
                    string mesag = $"Не найден подходящий адаптер для CAD {version.ToString()}";

                    log.Error($"{mesag}");

                    msg.ExceptionMessage(new FileNotFoundException(mesag));

                    return false;
                }

                log.Debug($"CadLoading CAD adapter: {targetDllFullName}");
                try
                {
                    if (targetDllFullName.Extension.Equals(netPluginExtension, StringComparison.CurrentCultureIgnoreCase))
                    {
                        string mesag = $"Загружается адаптер для CAD {version.ToString()}: {targetDllFullName.FullName}";

                        msg.ConsoleMessage(mesag);
                        log.Debug(mesag);

                        //asm = Assembly.LoadFrom(targetDllFullName.FullName);
                    }
                    else
                    {
                        //на случай, если в будущем будет поддержка других типов плагинов, например ARX или VBA
                        throw new NotSupportedException($"Unsupported plugin type: {targetDllFullName.Extension}");
                    }

                    log.Debug("Adapter CAD initialized successfully");

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

                string foundPath = GetFileOfDir(directory, true, targetFileName_);

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
        private string GetFileOfDir(string path, bool withSubfolders, string serchPatern = "*.dll")
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

                msg.ExceptionMessage(messag, ex);

                return string.Empty;
            }
        }


        #region AssemblyResolver

        /// <summary>
        /// Tries the register assembly resolver.
        /// </summary>
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

        /// <summary>
        /// Tries the unregister assembly resolver.
        /// </summary>
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

                string fullPath = GetFileOfDir(_assemblyDirectory, true, dllName);

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

        #endregion

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

        private void TryTerminate()
        {
            try
            {
                log.Trace("LogManager.Shutdown");
                LogManager.Shutdown();
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex);
            }

        }
    }
}