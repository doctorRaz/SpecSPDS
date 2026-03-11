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


using dRz.SpecSpds.Test.Loader;
using dRz.SpecSpds.Test.nLogTest;
using NLog;
using System;
using System.Diagnostics;


namespace dRz.SpecSpds.Test
{

    public class Start
    {

        /// <summary>
        /// общий логгер
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        [STAThread]
        private static void Main(string[] args)
        {

            var info = HostInfo.Current;

            string log = info.ToDiagnosticString();


            Console.WriteLine("-=Start=-");
            Console.ReadKey();
            //InternalLoggerHelpers.ConfigureInternalLogger();

            //var conf = LogManager.Configuration;

            //log.Error("Err");

            int major = 24;
            int minor = 2;
            Version version = new Version(major, minor);

            EntryPoint entryPoint = new EntryPoint(version);

            entryPoint.Initialize();

            Stopwatch stopwatch = Stopwatch.StartNew();
            NlogDebug test = new NlogDebug();
            test.Test();


            entryPoint.Terminate();

            stopwatch.Stop();
            Console.WriteLine($"Total time: {stopwatch.Elapsed}");


            Console.WriteLine("-=End=-");
            Console.ReadKey();




        }
    }



}
