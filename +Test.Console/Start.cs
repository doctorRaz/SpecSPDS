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

using drz.EnvironmentInfo;
using drz.Lib_A;
using drz.Lib_B;
using drz.Src.Infrastructure;
using drz.SpecSpds.Test.Tests;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.SpecSpds.Test
{
    public class Start
    {
        //public static SimpleInjector.Container ContainerIn;

        //internal static class AddonContext
        //{
        //    public static readonly InfoAdOn Info = InfoAdOn.Get(typeof(Start));
        //}

        /// <summary>
        /// общий логгер
        /// </summary>
        //private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [STAThread]
        private static void Main(string[] args)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //EntryPoint entryPoint = new EntryPoint();

            //entryPoint.Initialize();

            ////Stopwatch stopwatch = Stopwatch.StartNew();
            ////NlogDebug test = new NlogDebug();
            ////xxx test.Test();

            //entryPoint.Terminate();

            TestContainer testContainer = new TestContainer();
            testContainer.TestCondole();

            CommandA cmdA = new CommandA();
            cmdA.msgCommandA();

            CommandB cmdB = new CommandB();
            cmdB.msgCommandB();

            testContainer.TestCondole();
            cmdA.msgCommandA();
            cmdB.msgCommandB();

            string assemblyDirectory = string.Empty;

            string baseDir = Path.Combine(assemblyDirectory, "rrr.ffs");

            LogTests lt = new LogTests();
            //CommandA cmdA = new CommandA();

            CadEnvironmentInfoProvider ff = new CadEnvironmentInfoProvider();
            Console.WriteLine($"{ff.GetSummary()}");
            Console.WriteLine();

            Console.WriteLine($"{lt.Execute()}");
            Console.WriteLine();

            Console.WriteLine($"{cmdA.Execute()}");
            Console.WriteLine();

            Console.WriteLine($"{cmdB.Execute()}");
            Console.WriteLine();

            Console.WriteLine($"{InfoDll.ToStringLong()}");
            rr();
            Console.WriteLine($"{RT.Info}");
            Console.WriteLine($"{RT.Os}");
            Console.WriteLine($"{RT.Cad}");

            Console.WriteLine(typeof(Start).FullName);

            //for (int i = 0; i < 10000; i++)
            //{
            cmdA.LogTest("A0ttrrt");

            cmdB.LogTest("B0trte");

            lt.LogTest("Test0tet");

            cmdA.LogTest("2");

            cmdB.LogTest("2");

            cmdA.LogTest("20");

            cmdB.LogTest("20");
            //}

            //var rt = RT.Info;
            //var id = InfoDll;

            //ConsoleMessage.WriteLine($"{RT.Info}");
            //ConsoleMessage.WriteLine($"{RT.Os}");
            //ConsoleMessage.WriteLine($"{RT.Cad}");

            //ConsoleMessage.WriteLine("---------");

            //ConsoleMessage.WriteLine($"InfoDll {InfoDll}");

            //ConsoleMessage.WriteLine($"cmdA.Execute() {cmdA.Execute()}");

            ////ConsoleMessage.WriteLine($"cmdB.Execute() {cmdB.Execute()}");

            //RuntimeInfo runtime = RuntimeInfo.Current;
            //ConsoleMessage.WriteLine(runtime);

            //ConsoleMessage.WriteLine(SysInfo.Current);
            //ConsoleMessage.WriteLine(CadInfo.Current);

            //EntryPoint entryPoint = new EntryPoint();

            //entryPoint.Initialize();

            //NlogDebug test = new NlogDebug();
            //xxx test.Test();

            //entryPoint.Terminate();

            stopwatch.Stop();
            Console.WriteLine($"Total time: {stopwatch.Elapsed}");

            Console.WriteLine("-=End=-");

            //LogManager.Shutdown();

            //Thread.Sleep(new TimeSpan(0, 0, 10));
            Console.ReadKey();
        }

        private static void rr([CallerMemberName] string? caller = null)
        {
            Console.WriteLine(caller);
        }
    }
}