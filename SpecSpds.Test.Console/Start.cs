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
using dRz.SpecSpds.Test._draft;
using dRz.SpecSpds.Test.nLog;

using System;
namespace dRz.SpecSpds.Test
{

    public class Start
    {
     
        [STAThread]
        static void Main(string[] args)
        {
            //var ff = new _LoaderEnvironment();
            //var f = new LoaderEnvironment();
            NlogDebug nlogDebug = new NlogDebug();
            nlogDebug.Test();

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
