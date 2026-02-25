//неудачный совет  GPT((

using dRz.Loader.nCad.AssemblyResolve;
using dRz.Loader.nCad.Infrastructure.Logging;
using dRz.Loader.nCad.Interfaces;
using dRz.Loader.nCad.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;




#if AC

using Rtm = Autodesk.AutoCAD.Runtime;

#elif NC

using HostMgd.ApplicationServices;
using Rtm = Teigha.Runtime;

#endif

namespace dRz.Loader.nCad
{
    internal sealed class _EntryPoint : Rtm.IExtensionApplication
    {
        private const string netPluginExtension = ".dll";
        private static readonly string[] extensions = { ".arx", ".dvb" };
        private static readonly string[] methodNames = { "LoadArx", "LoadDVB" };

        private static Logger? _log;
        private IMessageService _msg = new MessageService();

        public void Initialize()
        {
            try
            {
                // 1. AssemblyResolve должен отработать максимально рано
                TryRegisterAssemblyResolver();

                // 2. Инициализация логгера (может упасть)
                TryInitLogger();

                // 3. Загрузка CAD-адаптера
                TryCadLoading();
            }
            catch (Exception ex)
            {
                // Абсолютный предохранитель — сюда доходить не должны,
                // но если вдруг — не позволяем CAD упасть
                SafeFallbackException(ex);
            }
        }

        private void TryRegisterAssemblyResolver()
        {
            try
            {
                var resolver = new AssemblyResolver();
                resolver.Register();
            }
            catch (Exception ex)
            {
                SafeFallbackException(ex);
            }
        }

        private void TryInitLogger()
        {
            try
            {
                LogBootstrap.Initialize();
                _log = LogManager.GetCurrentClassLogger();
            }
            catch (Exception ex)
            {
                // логгер не поднялся — работаем без него
                _log = null;
                SafeFallbackException(ex);
            }
        }

        private void TryCadLoading()
        {
            try
            {
                CadLoading();
            }
            catch (Exception ex)
            {
                // Не даём вылететь наружу
                SafeFallbackException(ex);
            }
        }

        private void CadLoading()
        {
            Version version = Application.Version;

            SafeLogInfo($"CAD detected: {version}");

            string fileFullName = GetType().Assembly.Location;
            Version minVersion = new Version(23, 0);

            FileInfo? targetDll = FindFile(fileFullName, version, minVersion);

            if (targetDll == null)
            {
                string message = $"Не найден подходящий адаптер для CAD {version}";
                SafeLogError(message);
                _msg.ConsoleMessage(message);
                return;
            }

            SafeLogInfo($"Loading CAD adapter: {targetDll.FullName}");

            if (targetDll.Extension.Equals(netPluginExtension, StringComparison.OrdinalIgnoreCase))
            {
                Assembly.LoadFrom(targetDll.FullName);
            }
            else
            {
                int index = Array.IndexOf(extensions, targetDll.Extension);
                if (index >= 0)
                {
                    object application = Application.AcadApplication;
                    application.GetType().InvokeMember(
                        methodNames[index],
                        BindingFlags.InvokeMethod,
                        null,
                        application,
                        new object[] { targetDll.FullName });
                }
            }

            SafeLogInfo("Adapter initialized successfully");
        }

        // ---------------- SAFE LOGGING ----------------

        private void SafeLogInfo(string message)
        {
            try
            {
                _log?.Info(message);
            }
            catch
            {
                _msg.ConsoleMessage(message);
            }
        }

        private void SafeLogError(string message)
        {
            try
            {
                _log?.Error(message);
            }
            catch
            {
                _msg.ConsoleMessage(message);
            }
        }

        private void SafeFallbackException(Exception ex)
        {
            try
            {
                _log?.Error(ex);
            }
            catch
            {
                // игнор — логгер сломан
            }

            try
            {
                _msg.ExceptionMessage(ex);
            }
            catch
            {
                // даже IMessageService не должен уронить Initialize
            }
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

        public void Terminate()
        {
            try
            {
                SafeLogInfo("LogManager.Shutdown");
                LogManager.Shutdown();
            }
            catch
            {
                // Игнорируем полностью
            }
        }
    }
}
