//using dRz.Loader.nCad.Interfaces;
//using dRz.Loader.nCad.Services;
//using HostMgd.ApplicationServices;
//using NLog;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;

//namespace dRz.Loader.nCad.Infrastructure
//{
//    internal  class CadLoader
//    {
//        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
//        internal  void Load()
//        {
//            /*private*/
//            IMessageService msg = new MessageService();
//            try
//            {
//                // Для начала извлекаем информацию о текущей версии AutoCAD и ищем
//                // соответствующую ей версию файла. Имя такого файла должно 
//                // формироваться по правилу: 
//                //    ИмяТекущейСборки.Major.Minor[x86|x64].(dll|arx|dvb).
//                // Где <Major> и <Minor> - это значения одноимённых свойств объекта 
//                // Version, полученного из Application.Version.
//                Version version = Application.Version;

//                log.Info($"CAD detected: {version.ToString()}");

//                string fileFullName = LoaderEnvironment.FileName;// GetType().Assembly.Location;

//                Version minVersion = new Version(23, 0);

//                FileInfo targetDllFullName = FindFile(fileFullName, version, minVersion);

//                if (targetDllFullName == null)
//                {
//                    string mesag = $"Не найден подходящий адаптер для CAD {version.ToString()}";

//                    log.Error($"{mesag}");

//                    //не найден хуже не будет, сообщаем об этом пользователю
//                    //Document doc = Application.DocumentManager.MdiActiveDocument;
//                    //if (doc == null)
//                    //{
//                    //    return;
//                    //}

//                    //Editor ed = doc.Editor;

//                    //ed.WriteMessage($"{mesag}");
//                    msg.ConsoleMessage($"{mesag}");

//                    return;
//                }

//                log.Info($"CadLoading CAD adapter: {targetDllFullName}");

//                // Если найден файл, соответствующий нашей версии AutoCAD, то 
//                // загружаем его.
//                Assembly? asm = null;
//                try
//                {
//                    if (targetDllFullName.Extension.Equals(netPluginExtension, StringComparison.CurrentCultureIgnoreCase))
//                    {
//                        asm = Assembly.LoadFrom(targetDllFullName.FullName);
//                    }
//                    else
//                    {
//                        int index = Array.IndexOf(extensions, targetDllFullName.Extension);

//                        if (index >= 0)
//                        {
//                            object application = Application.AcadApplication;

//                            application.GetType().InvokeMember(methodNames[index], BindingFlags
//                              .InvokeMethod, null, application, new object[] {
//                       targetDllFullName.FullName });
//                        }
//                    }

//                    log.Info("Adapter CAD initialized successfully");

//                }
//                catch (Exception ex)
//                {
//                    log.Error(ex.Message, ex);
//                }
//            }
//            catch (Exception ex)
//            {
//                //Document doc = Application.DocumentManager.MdiActiveDocument;
//                //if (doc == null)
//                //{
//                //    return;
//                //}

//                //Editor ed = doc.Editor;

//                //ed.WriteMessage($"{ex.Message}\n{ex.StackTrace}");
//                msg.ExceptionMessage(ex);
//            }


//        }

//        /// <summary>
//        /// Получить имя наиболее подходящего файла, для его последующей загрузки в
//        /// AutoCAD. Если такой файл не будет найден, то возвращается null.
//        /// </summary>
//        /// <param name="fileFullName">"Базовое" имя файла, т.е. полное имя 
//        /// файла без указания в нём версий ядра и разрядности платформы.</param>
//        /// <param name="expectedVersion">Версия AutoCAD, для которой следует 
//        /// выполнить поиск соответствующей версии файла.</param>
//        /// <param name="minVersion">Наименьшая версия AutoCAD, ниже которой не 
//        /// следует выполнять поиск.</param>
//        /// <returns>Возвращается FileInfo наиболее подходящего файла, для его 
//        /// последующей загрузки в AutoCAD. Если такой файл не будет найден, то 
//        /// возвращается null.</returns>
//        private FileInfo FindFile(string fileFullName, Version expectedVersion,
//          Version minVersion)
//        {

//            if (fileFullName == null)
//                throw new ArgumentNullException("fileFullName");

//            if (fileFullName.Trim() == string.Empty)
//                throw new ArgumentException(
//                  "fileFullName.Trim() == String.Empty");

//            if (expectedVersion < minVersion)
//                throw new ArgumentException(
//                  "expectedVersion < minVersion");

//            int major = expectedVersion.Major;
//            //if (major != 26)
//            //{
//            //    major = 23;
//            //}
//            int minor = expectedVersion.Minor;

//            string? directory = Path.GetDirectoryName(fileFullName);
//            string fileName = Path.GetFileNameWithoutExtension(fileFullName);

//            string coreString = string.Format("{0}.{1}", major.ToString(),
//              minor.ToString());
//            //string coreString = string.Format("{0}", major.ToString());

//            string subDirectoryName = "R" + coreString;
//            string subDirectoryName_xPlatform = subDirectoryName + (IntPtr.Size == 4
//              ? "x86" : "x64");

//            string targetFileName = string.Empty;
//            string targetFileName_xPlatform = string.Empty;
//            string targetFileFullName = string.Empty;
//            string targetFileFullName_xPlatform = string.Empty;

//            List<string> items = new List<string>(extensions);
//            items.Insert(0, netPluginExtension);

//            string name = string.Empty;

//            foreach (string extension in items)
//            {

//                targetFileName = string.Format("{0}.{1}{2}", fileName, coreString,
//                  extension);
//                targetFileName_xPlatform = string.Format("{0}.{1}{2}{3}", fileName,
//                  coreString, IntPtr.Size == 4 ? "x86" : "x64", extension);

//                // Сначала выполняем поиск в текущем каталоге
//                targetFileFullName = Path.Combine(directory, targetFileName);
//                if (File.Exists(targetFileFullName))
//                {
//                    name = targetFileFullName;
//                    break;
//                }
//                targetFileFullName_xPlatform = Path.Combine(directory,
//                  targetFileName_xPlatform);
//                if (File.Exists(targetFileFullName_xPlatform))
//                {
//                    name = targetFileFullName_xPlatform;
//                    break;
//                }

//                // Если в текущем каталоге подходящий файл не найден, то продолжаем
//                // поиск по соответствующим подкаталогам
//                targetFileFullName = directory + "\\" + subDirectoryName +
//                  "\\" + targetFileName;
//                if (File.Exists(targetFileFullName))
//                {
//                    name = targetFileFullName;
//                    break;
//                }

//                targetFileFullName_xPlatform = directory + "\\" +
//                  subDirectoryName_xPlatform + "\\" + targetFileName_xPlatform;
//                if (File.Exists(targetFileFullName_xPlatform))
//                {
//                    name = targetFileFullName_xPlatform;
//                    break;
//                }
//            }

//            // Если найден файл, соответствующий нашей версии AutoCAD, то возвращаем 
//            // соответствующий ему объект FileInfo.
//            if (File.Exists(name))
//            {
//                return new FileInfo(name);
//            }
//            // Если соответствия не найдено, то продолжаем поиск, последовательно 
//            // проверяя наличие подходящего файла для более ранних версий AutoCAD
//            else
//            {
//                if (minor == 0)
//                {
//                    minor = 3;
//                    --major;
//                }
//                else
//                {
//                    --minor;
//                }

//                Version version = new Version(major, minor);
//                if (version < minVersion)
//                    return null;
//                FileInfo file = FindFile(fileFullName, new Version(major, minor),
//                  minVersion);
//                return file;
//            }
//        }

//    }
//}
