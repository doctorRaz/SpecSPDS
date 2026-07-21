using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.AddOnRuntime;
using drz.Src.Infrastructure;
using drz.Updater;
using drz.Updater.Services;
using System;
using System.CodeDom;
using static drz.Src.Infrastructure.AddOnContext;

namespace drz.SpecSpds.Test.Updater

{
    internal class TestUpdater
    {
      

        private static bool _isAddOnCompositionRoot;//контейнер наполнен

        private IDrzLogger _logger;//логгер

        private bool _isLoggerProvider;//логер есть


        public TestUpdater()
        {
            //***** ГОТОВИМ СЕРВИСЫ *************
            //AddOnCompositionRoot
            //AddOnCompositionRoot root;

            if (!_isAddOnCompositionRoot)
            {
                AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(TestUpdater).Assembly);
                AddOnContext.Initialize(root);
                _isAddOnCompositionRoot = true;//сервис поднялся
            }

            //Logger
            if (!_isLoggerProvider)
            {               
                _logger = NLogFactory.GetLogger<TestUpdater>();
                _logger.Debug("TestUpdater");

                _logger.ForErrorEvent()
                    .Message("Properties is null")
                    .Property("name", 10)
                    .Property("null", "Properties is null")
                    .Property("null", "Properties is null")
                    .Exception(new Exception("Properties is null"))
                    .Log();
                _isLoggerProvider = true;//сервис поднялся
            }
        }

        public void Run()
        {
            _logger.Debug("TestUpdater.Run");

            //todo выбор интерфейсов
            //https://github.com/kpblc2000/SimpleInjectCadExample/blob/master/README.md
            /* https://www.google.com/search?newwindow=1&safe=strict&sca_esv=19ca41183bcca24c&sxsrf=ANbL-n6SxS1ECu5JJOGjOvSPci6DMBKciA%3A1778047142917&ntc=1&sa=X&ved=2ahUKEwj94ZWlp-GVAxW4TVUIHZ24F-wQoo4PegYIAggBEAI&biw=1870&bih=1085&dpr=1&mtid=60BfauXyAtubwPAPqLWMiQ8&atvm=2&q=IMessageService+messageServices%0AICommandLineMessageService+msgC+%3D+%28ICommandLineMessageService%29messageServices%3B%0A%0AmsgC.ConsoleMessage%28addOnInfo.ProductName%29%3B%0A%0AIWindowMessageService+msgW+%3D+%28IWindowMessageService%29messageServices%3B%0A%D0%B2%D1%81%D0%B5+%D0%B8%D0%BD%D1%82%D0%B5%D1%80%D1%84%D0%B5%D0%B9%D1%81%D1%8B+%D1%83%D0%BD%D0%B0%D1%81%D0%BB%D0%B5%D0%B4%D0%B2%D0%B0%D0%BD%D1%8B+%D0%BE%D1%82+IMessageService+%2C+%D0%BE%D1%82%D0%BB%D0%B8%D1%87%D0%B0%D1%8E%D1%82%D1%81%D1%8F+%D1%82%D0%BE%D0%BB%D1%8C%D0%BA%D0%BE+%D1%80%D0%B5%D0%B0%D0%BB%D0%B8%D0%B7%D0%B0%D1%86%D0%B8%D1%8F%D0%BC%D0%B8%2C+%D1%82%D0%B0%D0%BA+%D0%BA%D0%BE%D1%80%D1%80%D0%B5%D0%BA%D1%82%D0%BD%D0%BE%3F%0AmsgW.InfoMessage%28addOnInfo.PackageDirectory%29%3B&mstk=AUtExfCpb7L6GXK_K2s3DIHu0epEncASNd0z2GpnP_t6qHnzt4xEELHwxICy0KXrBJocWiJ6xHy0zDbys9YUJ7T7SzuIXcviRwClpeHdHt6WMY-qX4mZdSSa2M77gXk_denpiLNthI3g4h1dV7oNIrk27gwXcIu59qvPqS1dIxAiiSwgYMLlEDaQY7qSmlDRJrEiIsuZ6pyui_7HxflzLVe988kzTzarQcX2Auug0X4oGj88Tlve5feGGkvC_MHvMZ_vousE7Ry-1m65UL91fsB2rNcITKV-BMEeWX4&csuir=1&udm=50
            */
            var mesageW = GetMessageService(MessageServiceType.Window);
            mesageW.InfoMessage("test");

            var mesageN = GetMessageService(MessageServiceType.CommandLine);
            Msg.ConsoleMessage($"TestUpdater start");

            var cad = CadInfo;
            var addon = AddonInfo;
            var sys = SysInfo;

            UpdateManager updateManager = new UpdateManager(addon, Msg, NLogFactory, UpdateMode.CheckAndInstall, false);
            updateManager.Run();


        }

    }
}
