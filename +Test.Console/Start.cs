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

using dRz.Cad.Diagnostics;
using dRz.Cad.Diagnostics.Cad;
using dRz.Cad.Diagnostics.Os;
using dRz.Loader;
using dRz.Loader.Infrastructure;
using dRz.LogServices.Diagnostics;
using dRz.SpecSpds.Test.Tests;
using dRz.SpecSPDS;
using NLog;
using NLog.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using static dRz.Loader.Infrastructure.AddonContext;

namespace dRz.SpecSpds.Test
{

    public class Start
    {
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
            string assemblyDirectory = string.Empty;
            
            string baseDir = Path.Combine(assemblyDirectory, "rrr.ffs");

            var lt = new LogTests();
            var cmdA = new CommandA();
            var cmdB = new CommandB();



            CadEnvironmentInfoProvider ff=new CadEnvironmentInfoProvider();
            Console.WriteLine($"{ff.GetSummary()}");
            Console.WriteLine( );
            
            Console.WriteLine($"{lt.Execute()}");
            Console.WriteLine( );

            Console.WriteLine($"{cmdA.Execute()}");
            Console.WriteLine( );

            Console.WriteLine($"{cmdB.Execute()}");
            Console.WriteLine( );







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

            //Console.WriteLine($"{RT.Info}");
            //Console.WriteLine($"{RT.Os}");
            //Console.WriteLine($"{RT.Cad}");

            //Console.WriteLine("---------");

            //Console.WriteLine($"InfoDll {InfoDll}");

            //Console.WriteLine($"cmdA.Execute() {cmdA.Execute()}");

            ////Console.WriteLine($"cmdB.Execute() {cmdB.Execute()}");


            //RuntimeInfo runtime = RuntimeInfo.Current;
            //Console.WriteLine(runtime);


            //Console.WriteLine(InfoOs.Current);
            //Console.WriteLine(InfoCad.Current);


            //EntryPoint entryPoint = new EntryPoint();

            //entryPoint.Initialize();

            Stopwatch stopwatch = Stopwatch.StartNew();
            //NlogDebug test = new NlogDebug();
            //xxx test.Test();


            //entryPoint.Terminate();

            stopwatch.Stop();
            Console.WriteLine($"Total time: {stopwatch.Elapsed}");


            Console.WriteLine("-=End=-");

            //LogManager.Shutdown();

            Thread.Sleep(new TimeSpan(0,0,10));
            //Console.ReadKey();




        }

     static   void rr([CallerMemberName] string? caller = null)
        {

            Console.WriteLine(caller);

        }
    }



}
