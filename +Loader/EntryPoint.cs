/* EntryPoint.cs
 * © Andrey Bushman, 2014
 * Поиск и загрузка версии плагина .NET, ARX или VBA, наиболее пригодной для 
 * текущей версии AutoCAD.
 * http://bushman-andrey.blogspot.ru/2014/06/dll-autocad.html
 */
using dRz.Loader.Cad.Interfaces;
using dRz.Loader.Cad.Infrastructure.Logging;
using dRz.Loader.Cad;
using dRz.Loader.Cad.Services;
using dRz.Loader.Cad.Infrastructure;

using NLog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel;
using dRz.Loader.Cad.Infrastructure.Info;


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

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private IMessageService msg = new MessageService();

        private bool _registered;

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
                //понаблюдать, возможно тормозит нану
                TryRegisterAssemblyResolver();

                //nlog
                // если ехception, поднимаем его сюда, стоп работа
                // пока не сделаю подмену интерфейсов (хотя нужда под вопросом, сборка drzNlog своя!!!!
                // если ех нет хоть и с битым конфигом, работу продолжим, но юзеру о битом конфиге сообщим в msg
                if (!LogBootstrap.Init())
                {
                    msg.ConsoleMessage($"[{nameof(LogBootstrap)}.{nameof(LogBootstrap.Init)}]: Ошибка в конфигурации Logger." +
                        $"\nЗагрузка {InfoAdOn.ProductName} будет продолжена");
                }


                //стартуем очистку копий и bak
                CleanBackups();

                //грузим адаптер под версию кад, если ex, конец работы, исключения поднимаем сюда, юзеру в msg сообщаем
                CadLoading();

            }
            catch (Exception ex) // ошибка инициализации, все развалилось, лог смысла не имеет
            {
                string message = $"{InfoAdOn.ProductName} не загружен!!!" +
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

        private void CleanBackups()
        {
            try
            {
                string directoryPath = InfoAdOn.AssemblyDirectory;

                CleaningBackups.Cleaning(directoryPath);
            }

            catch { }

        }


        /// <summary>
        /// Cads the loading.
        /// </summary>
        /// <returns></returns>
        private bool CadLoading()
        {
            //todo в лог пишем инфу о системе и каде

            try
            {
                // Для начала извлекаем информацию о текущей версии AutoCAD и ищем
                // соответствующую ей версию файла. Имя такого файла должно 
                // формироваться по правилу: 
                //    ИмяТекущейСборки.Major.Minor[x86|x64].(dll|arx|dvb).
                // Где <Major> и <Minor> - это значения одноимённых свойств объекта 
                // Version, полученного из Application.Version.
                Version version = Application.Version;

                //получаем информацию о хосте для лога
                InfoCad infoCad = InfoCad.Current;

                string fileDescription = infoCad.FileDescription;

                log.Info($"CAD detected: {fileDescription}.{version.ToString()}");

                string fileFullName = GetType().Assembly.Location;

                Version minVersion = new Version(23, 0);

                FileInfo? targetDllFullName = FindFile(fileFullName, version, minVersion);

                if (targetDllFullName == null)
                {
                    string mesag = $"Не найден подходящий адаптер для {fileDescription}.{version.ToString()}";

                    log.Error($"{mesag}");

                    msg.ExceptionMessage(new FileNotFoundException(mesag));

                    return false;
                }

                log.Debug($"CadLoading CAD adapter: {targetDllFullName}");//найден адаптер

                // Если найден файл, соответствующий нашей версии CAD, то 
                // загружаем его.
                Assembly? asm = null;
                try
                {
                    if (targetDllFullName.Extension.Equals(netPluginExtension, StringComparison.CurrentCultureIgnoreCase))
                    {
                        string mesag = $"Загружается адаптер для {fileDescription}.{version.ToString()}: {targetDllFullName.FullName}";

                        msg.ConsoleMessage(mesag);

                        log.Debug(mesag);

                        asm = Assembly.LoadFrom(targetDllFullName.FullName);
                    }
                    else
                    {
                        //на случай, если в будущем будет поддержка других типов плагинов, например ARX или VBA
                        NotSupportedException exception = new NotSupportedException($"Unsupported plugin type: {targetDllFullName.Extension}");

                        log.Error(exception, "Plugin type validation failed");

                        throw exception;
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
        private FileInfo? FindFile(string fileFullName,
                                   Version expectedVersion,
                                   Version minVersion)
        {

            if (fileFullName == null)
            {
                throw new ArgumentNullException(nameof(fileFullName), "The fileFullName parameter cannot be null.");
            }

            if (fileFullName.Trim() == string.Empty)
            {
                throw new ArgumentException("The fileFullName parameter cannot be an empty string.", nameof(fileFullName));
            }

            if (expectedVersion < minVersion)
            {
                throw new ArgumentException($"The expectedVersion of {expectedVersion} cannot be less than the minimum allowed version of {minVersion}.", nameof(expectedVersion));
            }

            string? directory = Path.GetDirectoryName(fileFullName);
            if (directory == null)
            {
                throw new ArgumentException("The provided fileFullName does not contain a valid directory path.", nameof(fileFullName));
            }

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
                {
                    break;
                }
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
                msg.ExceptionMessage(ex, $"Error searching files in {path}");
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
                if (_registered)
                {
                    return;
                }

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
                if (!_registered)
                {
                    return;
                }

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

        /// <summary>
        /// Код данного метода выполняется при завершении работы AutoCAD.
        /// </summary>
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
                log.Debug("LogManager.Shutdown");
                LogManager.Shutdown();
            }
            catch (Exception ex)
            {
                msg.ExceptionMessage(ex);
            }

        }
    }
}