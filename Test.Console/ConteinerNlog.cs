using dRz.Abstractions.Logger;
using dRz.Abstractions.Services;
using dRz.Abstractions.Services.Message;
using dRz.AddOnRuntime;
using dRz.Src.Infrastructure;
using System;

namespace dRz.Test.Console
{
    internal class ConteinerNlog
    {
        private static bool _isAddOnCompositionRoot;
        private readonly IDrzLogger? _logger;
        private readonly ICommandLineMessageService? _msgCmd;

        internal ConteinerNlog()
        {
            try
            {
                if (_isAddOnCompositionRoot)
                {
                    return;
                }

                //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
                // один раз в точке входа /Rtm.IExtensionApplication/
                AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(ConteinerNlog).Assembly);

                // экземпляр копии контейнера by ref
                AddOnContext.Initialize(root.Get<IAddOnServices>());

                string msg = $"{nameof(ConteinerNlog)} Init";

                //логер
                _logger = AddOnContext.NLogFactory.GetLogger(typeof(ConteinerNlog));
                _logger.InfoCaller(msg);

                //ком строка
                _msgCmd = AddOnContext.MsgCmd;
                _msgCmd.InfoMessage(msg);

                _isAddOnCompositionRoot = true;//сервис поднялся
            }
            catch (Exception ex)
            {
                //роняем загрузчик
                throw new InvalidOperationException("AddOnCompositionRoot initialization failed", ex);
            }
        }

        internal void ConteinerNlogRun()
        {
            _logger.Info(AddOnContext.AddOnInfo.ToLongString());
            _logger.Info(AddOnContext.CadInfo.ToLongString());
            _logger.Info(AddOnContext.SysInfo.ToLongString());
        }

        public static void Run()
        {
            ConteinerNlog conteineNlog = new ConteinerNlog();
            conteineNlog.ConteinerNlogRun();
        }
    }
}