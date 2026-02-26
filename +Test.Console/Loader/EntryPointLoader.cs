/* EntryPoint.cs
 * © Andrey Bushman, 2014
 * Поиск и загрузка версии плагина .NET, ARX или VBA, наиболее пригодной для 
 * текущей версии AutoCAD.
 * http://bushman-andrey.blogspot.ru/2014/06/dll-autocad.html
 */
using dRz.SpecSpds.Test.Interfaces;
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

        internal void Test()
        {
            for (int major = 22; major < 28; major++)

            {
                for (int minor = 0; minor < 5; minor++)
                {
                    Run(new Version(major, minor));
                }

            }
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        private void Run(Version version)
        {
            _version = version;

            if (!CadLoading())
            {
                string mesag = "Ошибка загрузки адаптера для CAD. Работа плагина невозможна.";
                log.Error($"{mesag}");
                msg.ConsoleMessage($"{mesag}");
            }
            else
            {
                string mesag = "Адаптер для CAD загружен успешно.";
                log.Info($"{mesag}");
                msg.ConsoleMessage($"{mesag}");
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
                    msg.ExceptionMessage(ex);
                    log.Error(ex, ex.Message);
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
                msg.ExceptionMessage(ex, $"Error searching files in {path}");
                return string.Empty;
            }
        }

    }
}