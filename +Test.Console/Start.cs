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

global using AddOnCtx = drz.Src.Infrastructure.AddOnContext;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using System;
using System.Diagnostics;

namespace drz.SpecSPDS.Test
{
    /// <summary>
    /// Start
    /// </summary>
    public class Start
    {
        #region Private Methods

        [STAThread]
        private static void Main(string[] args)
        {
            Stopwatch sw = Stopwatch.StartNew();

            ContainerTransfer ct = new ContainerTransfer();

            _logger = AddOnCtx.NLogFactory.GetLogger(typeof(Start));
            _logger.Info($"Start: {sw.Elapsed}");
            sw.Restart();

            //*******************
            //тест проброса объектов и сервисов между библиотеками по цепочке и логгирование
            //Test.Console->Test.LibA->Test.LibB->
            //LibA, LibB знают только интерфейсы, Abstractions
            ct.ContainerTransfer_Run();
            _logger.Info("The End");
            Console.ReadKey();
        }

        #endregion Private Methods

        #region Private Fields

        private static IDrzLogger? _logger;

        #endregion Private Fields

       
    }
}