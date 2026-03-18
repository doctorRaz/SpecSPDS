/*

У меня не срабатывал, насколько я помню, механизм нескольких определений CommandMethod. Может, потому, что было под АКАД ))

AppSettings я б засунул именно в то, что работает под кадом.

Т.е., если надо, в методы/классы подсовывать не AppSettings.Settings, а именно ApplicationSettings. По крайней мере подменить можно будет при необходимости тестирования

получаю список  свойств маркеров
Ну ты же не напрямую McObject бросаешь, я правильно понимаю? А собственный класс, который маскирует все это дело? А то и абстрактный класс.
 пульну на сборку в таблицу
Отличный вариант написать отдельный тест под это дело ))) Юзер выбрал - вызываем реализацию интефейса типа IConvertMarker, который возвращает коллекцию экземпляров твоего класса.
Эту коллекцию - в метод типа PrepareDataForTable (который может вернуть чуть ли не полный вариант форматирования таблицы, но без привязки к каду).
Результат метода - в чисто кадовский метод CreateMCadTable по твоим же правилам.

Я бы сегодня, наверное, делал так. Сорян, в код не полезу - помимо резюме еще и работы понакидали (((

Насколько это лучше - пока не представляю. Но я в любом случае по максимуму бы отделял то, что без када жить не может, от чисто данных. По крайней мере в сегодняшних реалиях

*/

using dRz.Loader.Cad;
using dRz.SpecSpds.Test.Tests;
using Microsoft.Win32;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;


namespace dRz.SpecSpds.Test
{

    public class Start
    {

        public string HKLM_GetString(string path, string key)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(path);
                if (rk == null)
                {
                    return "";
                }

                return (string)rk.GetValue(key);
            }
            catch { return ""; }
        }

        public string FriendlyName()
        {
            string ProductName = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName");
            string CSDVersion = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CSDVersion");
            if (ProductName != "")
            {
                return (ProductName.StartsWith("Microsoft") ? "" : "Microsoft ") + ProductName +
                            (CSDVersion != "" ? " " + CSDVersion : "");
            }
            return "";
        }

        public int OStype()
        {
            int os = 0;
            IEnumerable<string> list64 = Directory.GetDirectories(Environment.GetEnvironmentVariable("SystemRoot")).Where(s => s.Equals(@"C:\Windows\SysWOW64"));
            IEnumerable<string> list32 = Directory.GetDirectories(Environment.GetEnvironmentVariable("SystemRoot")).Where(s => s.Equals(@"C:\Windows\System32"));
            if (list32.Count() > 0)
            {
                os = 32;
                if (list64.Count() > 0)
                {
                    os = 64;
                }
            }
            return os;
        }

        // Source - https://stackoverflow.com/a/56051955
        // Posted by IceCreamBoi23
        // Retrieved 2026-03-18, License - CC BY-SA 4.0

        [DllImport("ntdll.dll", SetLastError = true)]
        internal static extern uint RtlGetVersion(out OsVersionInfo versionInformation); // return type should be the NtStatus enum

        [StructLayout(LayoutKind.Sequential)]
        internal struct OsVersionInfo
        {
            private readonly uint OsVersionInfoSize;

            internal readonly uint MajorVersion;
            internal readonly uint MinorVersion;

            private readonly uint BuildNumber;

            private readonly uint PlatformId;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            private readonly string CSDVersion;
        }



        /// <summary>
        /// общий логгер
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [STAThread]
        private static void Main(string[] args)
        {


            Console.WriteLine("-=Start=-");
            //Console.ReadKey();
            //InternalLoggerHelpers.ConfigureInternalLogger();

            //var conf = LogManager.Configuration;

            //log.Error("Err");

            int major = 23;
            int minor = 2;
            Version version = new Version(major, minor);

            EntryPoint entryPoint = new EntryPoint();

            entryPoint.Initialize();

            Stopwatch stopwatch = Stopwatch.StartNew();
            NlogDebug test = new NlogDebug();
            //test.Test();


            entryPoint.Terminate();

            stopwatch.Stop();
            Console.WriteLine($"Total time: {stopwatch.Elapsed}");


            Console.WriteLine("-=End=-");
            Console.ReadKey();




        }
    }



}
