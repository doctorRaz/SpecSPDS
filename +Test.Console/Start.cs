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

using drz.SpecSpds.Test.SimpleInjector;
using drz.SpecSpds.Test.TestWmi;
using System;
using System.Diagnostics;


namespace drz.SpecSpds.Test
{
    public class Start
    {
        #region Private Methods

        [STAThread]
        private static void Main(string[] args)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //
            SysInfo_NEW2 sysInfo_NEW2 = new SysInfo_NEW2();

            Console.WriteLine(sysInfo_NEW2);
            Console.WriteLine(sysInfo_NEW2.ToShortString());
            Console.WriteLine(sysInfo_NEW2.ToLongString());

            /* //SimpleInjector */
            DrzSimpleInjector drzSimpleInjector = new DrzSimpleInjector();
            drzSimpleInjector.Start();

            /*//логер
            DrzLogger drzLogger = new DrzLogger();
            drzLogger.Start();
            */

            stopwatch.Stop();
            Console.WriteLine($"Total time: {stopwatch.Elapsed}");

            Console.WriteLine("-=End=-");

            //Thread.Sleep(new TimeSpan(0, 0, 10));
            Console.ReadKey();
        }

        #endregion Private Methods
    }

    
}