// тест передачи контейнера между сборками
// container transfer test between builds


using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.AddOnRuntime;
using drz.Lib_A;
//using static drz.Src.Infrastructure.AddOnContext;

namespace drz.SpecSpds.Test

{
    internal class ContainerTransferTestBetweenBuilds
    {
        private readonly IDrzLogger _logger;//логгер

        //private readonly IAddOnServices _services;

        private static bool _isAddOnCompositionRoot;//контейнер наполнен

        internal ContainerTransferTestBetweenBuilds()
        {
            if (!_isAddOnCompositionRoot)
            {
                //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
                // один раз в точке входа /Rtm.IExtensionApplication/
                AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(ContainerTransferTestBetweenBuilds).Assembly);


                // экземпляр копии контейнера by ref
                AddOnCtx.Initialize(root.Get<IAddOnServices>());

                _isAddOnCompositionRoot = true;//сервис поднялся
            }

            _logger = AddOnCtx.NLogFactory.GetLogger(typeof(ContainerTransferTestBetweenBuilds));

            _logger.InfoCaller("ContainerTransferTestBetweenBuilds Initialized");
        }

        internal void Run()
        {
            System.Exception ex = new System.Exception("Properties is null");

            _logger.TraceCaller("TraceCaller");
            _logger.DebugCaller("DebugCaller");
            _logger.InfoCaller("InfoCaller");
            _logger.WarnCaller("WarnCaller");

            _logger.ErrorCaller("ErrorCaller", ex);
            _logger.ErrorCaller("ErrorCaller");
            _logger.ErrorCaller(ex, "ErrorCaller");
            _logger.ErrorCaller(ex);

            _logger.FatalCaller("FatalCaller", ex);
            _logger.FatalCaller("FatalCaller");
            _logger.FatalCaller(ex, "FatalCaller");
            _logger.FatalCaller(ex);

            _logger.Debug("CommandB.Run");
            _logger.ForErrorEvent()
                    .Message("Properties is null")
                    .Property("name", 10)
                    .Property("null", "Properties is null")
                    .Property("00", "Properties is null")
                    .Exception(ex)
                    .Log();

            //---- CadInfo -------
            _logger.Info(AddOnCtx.CadInfo.ToLongString());
            AddOnCtx.MsgCmd.InfoMessage(AddOnCtx.CadInfo.ToLongString());

            //----- AddOnInfo ------
            _logger.Info(AddOnCtx.AddOnInfo.ToLongString());
            AddOnCtx.MsgCmd.InfoMessage(AddOnCtx.AddOnInfo.ToLongString());

            //----- SysInfo ------
            _logger.Info(AddOnCtx.SysInfo.ToLongString());
            AddOnCtx.MsgCmd.InfoMessage(AddOnCtx.SysInfo.ToLongString());

            AddOnCtx.MsgGUI.InfoMessage($"End {nameof(ContainerTransferTestBetweenBuilds)}");

            CommandA c = new CommandA(AddOnCtx.Services);
            c.Run();
        }
    }
}