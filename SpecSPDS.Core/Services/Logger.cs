using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace dRz.SpecSPDS.Core.Services
{
    public class Logger
    {
        public Logger(string appName = @"logger")
        {
            string date = DateTime.Now.ToString("yyyyMMdd-HH_mm_ss",
                      CultureInfo.InvariantCulture);

            _appName = $"{date}_{appName}.log";
        }

        public void LogClear()
        {
            //!удаляем BAK файл (после предыдущего обновления)
            //каталог DLL
            string sPath = Directory.GetParent(_path).FullName;
            //список BAK
            List<string> logFiles = FetchingPatchFiles.GetFilesOfDir(sPath, true, "*.log");
            foreach (string logFile in logFiles)
            {
                try
                {
                    File.Delete(logFile); //пытаемся удалить без проверки занят или нет             
                }
                catch { }
            }
        }
        public async void Log(string message)
        {

            string dat = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.FFFFF",
                                            CultureInfo.InvariantCulture);

            string text = $"{dat}: {message}";

            using (StreamWriter writer = new StreamWriter(_path, true))
            {
                await writer.WriteLineAsync(text);
            }
        }

        string _appName = "";

        private string _path => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), _appName);
    }
}
