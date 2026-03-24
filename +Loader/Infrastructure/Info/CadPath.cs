using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace dRz.CAD.Runtime.Info
{
    /// <summary>
    /// Класс для получения пути к исполняемому файлу хоста (exe)
    /// </summary>
    public static class CadPath
    {
        /// <summary>
        /// Получить полный путь к exe текущего процесса
        /// </summary>
        public static string GetExePath()
        {
            try
            {
                StringBuilder sb = new StringBuilder(1024);
                int size = GetModuleFileName(IntPtr.Zero, sb, sb.Capacity);
                if (size > 0)
                {
                    return sb.ToString();
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Получить полный путь к exe текущего процесса
        /// </summary>
        public static string GetExePathProcess()
        {
            try
            {
                return Process.GetCurrentProcess().MainModule?.FileName ?? "";
            }
            catch
            {
                return "";
            }
        }


        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetModuleFileName(IntPtr hModule, StringBuilder lpFilename, int nSize);
    }
}
