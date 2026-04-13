/* EntryPoint.cs
 * © Andrey Bushman, 2014
 * Поиск и загрузка версии плагина .NET, ARX или VBA, наиболее пригодной для
 * текущей версии AutoCAD.
 * http://bushman-andrey.blogspot.ru/2014/06/dll-autocad.html
 */

using NLog;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using drz.Cad.Diagnostics;

using drz.Loader.Infrastructure;

//using AC = drz.Loader.Infrastructure.AddOnContext;

using static drz.Loader.Infrastructure.AddOnContext;

using drz.AddOn.Composition;
using HostMgd.ApplicationServices;
using HostMgd.EditorInput;

#if CMD

using drz.SpecSpds;

#elif NC

using drz.Loader;
using Rtm = Teigha.Runtime;
using Scm = System.ComponentModel;

[assembly: Rtm.ExtensionApplication(typeof(EntryPoint))]
#endif

namespace drz.Loader
{
    /// <summary>
    /// Задачей данного класса является поиск и загрузка в AutoCAD наиболее
    /// подходящей для него версии плагина.
    /// </summary>
#if CMD

    internal sealed class EntryPoint
#else

    internal sealed class EntryPoint : Rtm.IExtensionApplication
#endif
    {
        private static bool _isAddOnCompositionRoot;//контейнер наполнен

        private bool _isRegisterAssemblyResolver;//register assembly resolver

        private Logger log;//логгер

        private bool _isLoggerProvider;//логер есть

        private const string netPluginExtension = ".dll";

#if DEBUG && NC

        [Rtm.CommandMethod($"инит_{GeneratedCompile.CommandSuf}", Rtm.CommandFlags.Session)]
        [Scm.Description($"ручной инит загрузчика для {GeneratedCompile.CommandSuf}")]
        public static void test()
        {
            Msg.ConsoleMessage($"инит {GeneratedCompile.CommandSuf}");
            EntryPoint entryPoint = new EntryPoint();
            entryPoint.Initialize();
        }

        [Rtm.CommandMethod($"console-message-test-{GeneratedCompile.CommandSuf}", Rtm.CommandFlags.Session)]
        public static void ConsoleMessageCommand()
        {
            MsgCmd.ConsoleMessage("Console message");
        }

#endif

        /// <summary>
        /// Код этого метода будет запущен на исполнение при загрузке сборки в
        /// AutoCAD. В результате его работы происходит попытка найти и загрузить в
        /// AutoCAD наиболее подходящую версию плагина из имеющихся в наличии.
        /// </summary>
        public void Initialize()
        {
            //если нет библиотек или еще какой косяк
            try
            {
                /* регистрируемся
                    TryRegisterAssemblyResolver();
                    каждая регистрация вызыввается во всех модулях
                */

                TryAddOnCompositionRoot();//получаем окружение

                //nlog
                //обертка инит логера, если ех на старте, то отловим в месадж
                TryLoggerProvider();

                //грузим адаптер под версию кад, если ex, конец работы, исключения поднимаем сюда, юзеру в msgClass1 сообщаем
                CadLoading();
            }
            catch (Exception ex) // ошибка инициализации, все развалилось, лог смысла не имеет
            {
                string message = $"Приложение не загружено!!!" +
                     $"\nСкопируйте это сообщение и отправьте разработчику";

                if (_isLoggerProvider)//todo если лог инит ПРОВЕРИТЬ не вызовет ли еще один ЕХ если false??!!
                {
                    log.Error(message, ex);
                }
                if (_isAddOnCompositionRoot)
                {
                    Msg.ExceptionMessage(message, ex);
                }
                else
                {
                    message = $"Exception: {message}\n{ex.Message}\n{ex.StackTrace}";

                    Document document = Application.DocumentManager.MdiActiveDocument;
                    if (document != null)
                    {
                        Editor editor = document.Editor;

                        editor.WriteMessage(message);
                    }
                    else
                    {

                        Application.ShowAlertDialog(message);
                    }
                }
            }

            /*отписываемся независимо от результата этому аддону подписка  больше не нужен
            finally
            {
                try
                {
                    //TryUnregisterAssemblyResolver();
                }
                catch { }
            }
            */
        }

        private void TryAddOnCompositionRoot()
        {
            try
            {
                if (_isAddOnCompositionRoot)
                {
                    return;
                }

                AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(EntryPoint).Assembly);

                AddOnContext.Initialize(root);

                _isAddOnCompositionRoot = true;//сервис поднялся
            }
            catch (Exception ex)
            {
                //роняем загрузчик
                throw new InvalidOperationException("AddOnCompositionRoot initialization failed", ex);
            }
        }

        /// <summary>
        /// Tries the logger.
        /// </summary>
        private void TryLoggerProvider()
        {
            try
            {
                log = LoggerProvider.For<EntryPoint>();

                _isLoggerProvider = true;//сервис поднялся
            }
            catch (Exception ex)
            {
                //роняем загрузчик
                throw new InvalidOperationException("LoggerProvider initialization failed", ex);
            }
        }

        /// <summary>
        /// Cads the loading.
        /// </summary>
        /// <returns></returns>
        private bool CadLoading()
        {
            //var  nlogFactory=NlogFactory.Logger;
            //ILogger log = LogManager.GetCurrentClassLogger();

            try
            {
                // Для начала извлекаем информацию о текущей версии AutoCAD и ищем
                // соответствующую ей версию файла. Имя такого файла должно
                // формироваться по правилу:
                //    ИмяТекущейСборки.Major.Minor[x86|x64].(dll|arx|dvb).
                // Где <Major> и <Minor> - это значения одноимённых свойств объекта
                // Version, полученного из Application.Version.
                Version version = RT.Cad.ProductVersion;// Version;

                string fileDescription = RT.Cad.FileDescription;

                log.Debug("Обнаружен: {0}", RT.Cad);

                string fileFullName = GetType().Assembly.Location;

                int minMajor = GeneratedCompile.MinVersion;//из Directory.Build.props проекта

                Version minVersion = new Version(minMajor, 0);

                log.Debug("minVersion {0}", minVersion);

                FileInfo? targetDllFullName = FindFile(fileFullName, version, minVersion);

                if (targetDllFullName == null)
                {
                    string mesag = $"Не найден подходящий адаптер для {RT.Cad}";

                    log.Error($"{mesag}");

                    Msg.ExceptionMessage(new FileNotFoundException(mesag));

                    return false;
                }

                log.Debug("Адаптер найден в: {0}", targetDllFullName);//найден адаптер

                // Если найден файл, соответствующий нашей версии CAD, то
                // загружаем его.
                Assembly? asm = null;
                try
                {
                    if (targetDllFullName.Extension.Equals(netPluginExtension, StringComparison.CurrentCultureIgnoreCase))
                    {
                        //string mesag = $"Загружается адаптер для: {fileDescription} v{version}, целевая сборка: {targetDllFullName.FullName}";

                        log.Debug("Загружается адаптер для: {0}, целевая сборка: {1}", RT.Cad, targetDllFullName.FullName);

                        asm = Assembly.LoadFile(targetDllFullName.FullName);
                    }
                    else
                    {
                        //на случай, если в будущем будет поддержка других типов плагинов, например ARX или VBA
                        NotSupportedException exception = new NotSupportedException($"Unsupported plugin type: {targetDllFullName.Extension}");

                        log.Error(exception, "Plugin type validation failed");

                        throw exception;
                    }

                    log.Debug("Адаптер для {0} загружен", RT.Cad);
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

                throw new InvalidOperationException("failed", ex);
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

#if CMD
            fileName = "SpecSPDSn";
#endif
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
                Msg.ExceptionMessage(ex, $"Error searching files in {path}");
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
                if (_isRegisterAssemblyResolver)
                {
                    return;
                }

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                _isRegisterAssemblyResolver = true;
            }
            catch (Exception ex)
            {
                Msg.ExceptionMessage("AssemblyResolver registration failed", ex);
            }
        }

        /// <summary>
        /// Tries the unregister assembly resolver.
        /// </summary>
        private void TryUnregisterAssemblyResolver()
        {
            try
            {
                if (!_isRegisterAssemblyResolver)
                {
                    return;
                }

                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

                _isRegisterAssemblyResolver = false;
            }
            catch (Exception ex)
            {
                Msg.ExceptionMessage("AssemblyResolver unregistered failed", ex);
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

                //think переделать класс на статик как у жопите,
                //проверять путь от запросившей сборки с проверкой на нул,
                //если нул тогда от _resolverBaseDir или тупо вернуть нулл

                string _resolverBaseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                //d:\@Developers\В работе\Reminder\VS\GPT-Assembly Resolver в другом модуле.md
                /*
                string assemblyDirectory = args.RequestingAssembly.Location;
                */
                string fullPath = GetFileOfDir(_assemblyDirectory, true, dllName);

                if (!string.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                {
                    return Assembly.LoadFile(fullPath);
                }
            }
            catch (Exception ex)
            {
                Msg.ExceptionMessage("Failed to resolve assembly", ex);
            }

            return null;
        }

        #endregion AssemblyResolver

        #region Terminate

        /// <summary>
        /// Код данного метода выполняется при завершении работы AutoCAD.
        /// </summary>
        public void Terminate()
        {
            try
            {
                TryTerminate();
            }
            catch { } // смысла нет что то показывать при закрытии наны
        }

        private void TryTerminate()
        {
            try
            {
                log.Debug("Terminate");

                AddOnContext.Dispose();
            }
            catch { } // смысла нет что то показывать при закрытии наны
        }

        #endregion Terminate
    }
}