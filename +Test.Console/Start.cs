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
using drz.SpecSpds.Test;
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

        private static IDrzLogger _logger;//логгер

        [STAThread]
        private static void Main(string[] args)
        {
            Stopwatch swTotal = Stopwatch.StartNew();
            Stopwatch sw = Stopwatch.StartNew();

            ContainerTransferTestBetweenBuilds start = new ContainerTransferTestBetweenBuilds();

            _logger = AddOnCtx.NLogFactory.GetLogger(typeof(Start));
            sw.Stop();

            AddOnCtx.MsgCmd.InfoMessage($"Start: {sw.Elapsed}");
            _logger.Info($"Start: {sw.Elapsed}");
            _logger.InfoCaller($"Start: {sw.Elapsed}");
            AddOnCtx.MsgGUI.InfoMessage($"Start: {sw.Elapsed}");

            //---- CadInfo -------
            _logger.Info(AddOnCtx.CadInfo.ToLongString());
            AddOnCtx.MsgCmd.InfoMessage(AddOnCtx.CadInfo.ToLongString());

            //----- AddOnInfo ------
            _logger.Info(AddOnCtx.AddOnInfo.ToLongString());
            AddOnCtx.MsgCmd.InfoMessage(AddOnCtx.AddOnInfo.ToLongString());

            //----- SysInfo ------
            _logger.Info(AddOnCtx.SysInfo.ToLongString());
            AddOnCtx.MsgCmd.InfoMessage(AddOnCtx.SysInfo.ToLongString());

            AddOnCtx.MsgGUI.InfoMessage("End Init");

            //*******************
            start.Run();
        }

        #endregion Private Methods
    }
}