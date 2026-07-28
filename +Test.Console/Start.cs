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
using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services.Message;
using drz.Infrastructure.Infrastructure;
using drz.Infrastructure.Services;
using drz.Lib_A;
using drz.n.Infrastructure.Services;
using drz.Updater;
using System;
using System.Collections.Generic;
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
                TestContainer tc = new TestContainer();
                _logger = AddOnCtx.NLogFactory.GetLogger(typeof(Start));
                _isLoggerProvider = true;
                _logger.Info($"Start: {sw.Elapsed}");
                _msgCmd = AddOnCtx.MsgCmd;

                //********
                //test add addon Info

                //
                TestGetOrADDAddonInfo tgAdd = new TestGetOrADDAddonInfo();

                IAddOnInfo addOnInfo1 = tgAdd.AddAssembly((typeof(TestContainerTransfer).Assembly));
                IAddOnInfo addOnInfo10 = tgAdd.AddAssembly((typeof(TestContainerTransfer).Assembly));
                IAddOnInfo addOnInfo2 = tgAdd.AddAssembly((typeof(CommandA).Assembly));

                TestGetOrADDAddonInfo tgAdd2 = new TestGetOrADDAddonInfo();

                IAddOnInfo addOnInfo3 = tgAdd2.AddAssembly((typeof(UpdateManager).Assembly));
                IAddOnInfo addOnInfo100 = tgAdd2.AddAssembly((typeof(TestContainerTransfer).Assembly));

                IReadOnlyCollection<IAddOnInfo> all = tgAdd.GetAll();
                IReadOnlyCollection<IAddOnInfo> all2 = tgAdd2.GetAll();

                var k=tgAdd.GetKeys();
                //********
                //test sys info

                ISysInfo sysInfo = new SysInfo();
                _msgCmd.InfoMessage($"SysInfo: {sysInfo.ToLongString()}");

                ISysInfo sysInfo22 = new SysInfo();
                _msgCmd.InfoMessage($"SysInfo: {sysInfo22.ToLongString()}");

                ICadInfo cadInfo = new CadInfo();
                _msgCmd.InfoMessage($"CadInfo: {cadInfo.ToLongString()}");

                //******
                // test message
                TestMessage tm = new TestMessage();

                ////message cmd to console
                tm.documentService.IsActive = true;

                tm.RunMsgCmd();

                ////message gui to win
                tm.RunMsgGui();

                ////router message win to console
                tm.RunMsg();

                tm.documentService.IsActive = false;
                tm.RunMsg();

                //*******************
                //тест проброса объектов и сервисов между библиотеками по цепочке и логгирование
                //Test.Console->Test.LibA->Test.LibB->
                //LibA, LibB знают только интерфейсы, Abstractions
                TestContainerTransfer tct = new TestContainerTransfer();
                tct.TestContainerTransfer_Run();
            }
            catch (Exception ex)
            {
                if (_isLoggerProvider) _logger.Fatal(ex, "Продолжение не возможно");
                AddOnCtx.Msg.ErrorMessage("Продолжение не возможно", ex);
            }
            finally
            {
                if (_isLoggerProvider) _logger.Info("Terminate");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        #endregion Private Methods

        #region Private Fields

        private static IDrzLogger? _logger;
        private static bool _isLoggerProvider;//логер есть
        private static IMessageService _msg;
        private static ICommandLineMessageService _msgCmd;

        #endregion Private Fields
    }
}