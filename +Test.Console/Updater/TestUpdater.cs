using System;
using System.IO;
using System.Linq;
using System.Reflection;

using drz.Src.Infrastructure;

using static drz.Src.Infrastructure.AddOnContext;

using drz.AddOn.Composition;
using drz.Abstractions.Logger;
using drz.Src.Services;
using drz.Updater;
using drz.Updater.Services;

namespace drz.SpecSpds.Test.Updater

{
    internal class TestUpdater
    {

        private static bool _isAddOnCompositionRoot;//контейнер наполнен

        private bool _isRegisterAssemblyResolver;//register assembly resolver

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
                _logger = LoggerProvider.For<TestUpdater>(/*AddonInfo.ProductName*/);
                _isLoggerProvider = true;//сервис поднялся
            }



        }

        public void Run()
        {
            _logger.Debug($"TestUpdater start");

            UpdateManager.Run(AddonInfo,Msg,UpdateMode.CheckAndInstall,false);


            var mesageW = GetMessageService(Abstractions.Services.MessageServiceType.Window);

            mesageW.InfoMessage("test");

            var mesageN = GetMessageService(Abstractions.Services.MessageServiceType.McNotifi);

            Msg.ConsoleMessage($"TestUpdater start");

            var cad = CadInfo;
            var addon = AddonInfo;
            var sys = SysInfo;

        }

    }
}
