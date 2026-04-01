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

using AddonA;
using AddonB;
using dRz.Cad.Diagnostics;
using dRz.Cad.Diagnostics.Cad;
using dRz.Cad.Diagnostics.Os;
using dRz.Loader;
using dRz.Loader.Infrastructure;
using dRz.SpecSpds.Test.Loader;
using dRz.SpecSpds.Test.Tests;
using NLog;
using System;
using System.Diagnostics;
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
            var lt = new LogTests();

            var cmdA = new CommandA();
     
            var cmdB = new CommandB();


            cmdA.LogTest();

            cmdB.LogTest();

            lt.LogTest();




            var rt = RT.Info;
            var id = InfoDll;

            Console.WriteLine($"{RT.Info}");
            Console.WriteLine($"{RT.Os}");
            Console.WriteLine($"{RT.Cad}");

            Console.WriteLine("---------");

            Console.WriteLine($"InfoDll {InfoDll}");

            Console.WriteLine($"cmdA.Execute() {cmdA.Execute()}");

            Console.WriteLine($"cmdB.Execute() {cmdB.Execute()}");


            RuntimeInfo runtime = RuntimeInfo.Current;
            Console.WriteLine(runtime);

          
            Console.WriteLine(InfoOs.Current);
            Console.WriteLine(InfoCad.Current);
                   

            EntryPoint entryPoint = new EntryPoint();

            entryPoint.Initialize();

            Stopwatch stopwatch = Stopwatch.StartNew();
            //NlogDebug test = new NlogDebug();
            //xxx test.Test();


            entryPoint.Terminate();

            stopwatch.Stop();
            Console.WriteLine($"Total time: {stopwatch.Elapsed}");


            Console.WriteLine("-=End=-");
            //Console.ReadKey();




        }
    }



}
