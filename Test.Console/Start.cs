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


using dRz.Loader.Cad.Infrastructure;
using dRz.SpecSpds.Test.nLogTest;
using dRz.SpecSPDS.Core._experimental;
using NLog;
using System;
namespace dRz.SpecSpds.Test
{

    public class Start
    {
        private static readonly ILogger logger = LogManager.GetCurrentClassLogger();

        [STAThread]
        static void Main(string[] args)        
        {
            var f = LoaderEnvironment.ProductName;
            var ff = new LoaderEnvironment();

            LogBootstrapAsync.Initialize();


            logger.Trace("Plugin loaded");
            //logger.Debug("Plugin loaded");
            //logger.Info("Plugin loaded");
            //logger.Warn("Plugin loaded");
            //logger.Error("Plugin loaded");
            //logger.Fatal("Plugin loaded");

            LogTestWrite logDebug = new LogTestWrite();

            LogDebugCore loggebCore = new LogDebugCore();

            for (int i = 0; i < 1; i++)
            {
                logDebug.Test();
                loggebCore.Test();
            }


            LogManager.Shutdown();

            return;
            /*
            TestSpeedConfig tSc = new TestSpeedConfig();

            for (int i = 0; i < 1; i++)
            {

                tSc.Run(); 
            }
            Console.WriteLine("Press eny key...");
            Console.ReadKey();
            */


            NlogDebug nlogDebug = new NlogDebug();

            nlogDebug.Test();

            var DateCreate = GlobalDiagnosticsContext.Get("DateCreate");
            var Caller = GlobalDiagnosticsContext.Get("Caller");


            /*
            Console.WriteLine($"GetExecutingAssembly\t{Assembly.GetExecutingAssembly().Location}");

            Console.WriteLine($"GetEntryAssembly\t{Assembly.GetEntryAssembly().Location}");

            Console.WriteLine($"OR\t{Assembly.GetEntryAssembly()?.Location ?? Assembly.GetExecutingAssembly().Location}");

            Console.WriteLine($"Start{typeof(Start).Assembly.Location}");

            
            */

            /*
            ModuleNameTest2 callerMemberNameTest2 = new ModuleNameTest2();

            callerMemberNameTest2.CallerName("test");

            ModuleNameTest callerMemberNameTest = new ModuleNameTest();

            callerMemberNameTest.CallerName("test");
            */
            //Test.ReadKey();
        }
    }



}
