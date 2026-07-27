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
using drz.Abstractions.Services.Message;
using drz.Infrastructure.Services;
using drz.n.Infrastructure.Services;
using System;
using System.Diagnostics;
using System.Windows;

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
            //MessageBoxResult f = MessageBox.Show("text", "caption",MessageBoxButton.YesNo,MessageBoxImage.Exclamation);

            //WindowMessageService_test wt = new WindowMessageService_test(IntPtr.Zero);

            //MessageResult rr = wt.AskYesNo("test","Caption");

            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                ContainerTransfer ct = new ContainerTransfer();
                var add = AddOnCtx.AddOnInfo;
                _logger = AddOnCtx.NLogFactory.GetLogger(typeof(Start));
                _isLoggerProvider = true;
                _logger.Info($"Start: {sw.Elapsed}");

                //todo так делать нехорошо , но для отладки можно(((
                DocumentService ds = (DocumentService)AddOnCtx.DocService;

                ds.IsActive = true;//doc yes
                AddOnCtx.Msg.InfoMessage("test");

                ds.IsActive = !ds.IsActive;//doc no
                AddOnCtx.Msg.InfoMessage(ds.FullPath);//ex

                AddOnCtx.MsgCmd.InfoMessage("test");
                AddOnCtx.MsgGUI.InfoMessage("test");

                sw.Restart();
            }
            catch (Exception ex)
            {
                if (_isLoggerProvider) _logger.Fatal(ex, "Продолжение не возможно");
                AddOnCtx.Msg.ErrorMessage("Продолжение не возможно",ex);
                AddOnCtx.Msg.ErrorMessage(ex);
                AddOnCtx.Msg.ErrorMessage("Продолжение не возможно");
            }

            //*******************
            //тест проброса объектов и сервисов между библиотеками по цепочке и логгирование
            //Test.Console->Test.LibA->Test.LibB->
            //LibA, LibB знают только интерфейсы, Abstractions
            /*ct.ContainerTransfer_Run();*/

            AddOnCtx.MsgCmd.InfoMessage("Press any key to exit...");
            Console.ReadKey();
        }

        #endregion Private Methods

        #region Private Fields

 
        private static IDrzLogger? _logger;
        private static bool _isLoggerProvider;//логер есть

        #endregion Private Fields
    }
}