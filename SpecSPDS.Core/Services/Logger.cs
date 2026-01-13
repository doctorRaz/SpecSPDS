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

            ClearCount();


        }

        List<string> logFiles
        {
            get
            {
                string sPath = Directory.GetParent(_path).FullName;

                return FetchingPatchFiles.GetFilesOfDir(sPath, true, "*.log");

            }
        }


        void ClearCount()
        {
            List<string> files = logFiles;
            files.Sort();

            int delcount = 10;

            for (int i = 0; i < files.Count - delcount; i++)
            {
                try
                {

                    File.Delete(files[i]); //пытаемся удалить без проверки занят или нет    

                }
                catch { }

            }

        }


        public void LogAllClear()
        {

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
