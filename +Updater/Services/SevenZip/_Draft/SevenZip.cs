using System;
using System.Diagnostics;
using System.IO;

namespace drz.Updater.Services
{
    public class SevenZip
    {
        public static void Extract(
          string archive,
          string destination,
          string password = null)
        {
            Directory.CreateDirectory(destination);

            //распаковка без пароля
            string arguments = $"x \"{archive}\" -o\"{destination}\" -y";
            if (password != null)
            {
                arguments = $"x \"{archive}\" -o\"{destination}\" -p\"{password}\" -y";
            }

            var psi = new ProcessStartInfo
            {
                FileName = "7z.exe",
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using var process = Process.Start(psi)!;

            process.WaitForExit();

            //todo возвращать код ошибки как enum??
            /*
           *    0 No error 
           *    1 Warning (Non fatal error(s)). For example, one or more files were locked by some other application, so they were not compressed. 
           *    2 Fatal error 
           *    7 Command line error 
           *    8 Not enough memory for operation 
           *    255 User stopped the process 
            */
            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    process.StandardError.ReadToEnd());
            }
        }

        //todo добавить метод упаковки с паролем и без
        //todo добавить метод проверки с паролем и без

    }
}
