using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dRz.SpecSPDS.Core.Services
{
    public class Log
    {
        public Log(string path = @"log.txt")
        {

            _path = path;

            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }

        public void LogClear()
        {
            if (File.Exists(_path))
            {
                File.Delete(_path);
            }
        }
        public async void LogWrite(string message)
        {

            string dat = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.FFFFF",
                                            CultureInfo.InvariantCulture);

            string text = $"{dat}: {message}";

            // полная перезапись файла 
            using (StreamWriter writer = new StreamWriter(_path, true))
            {
                await writer.WriteLineAsync(text);
                //writer.WriteLine(text);
            }
        }

       public string _path = @"log.txt";
    }
}
