// тест передачи контейнера между сборками
// container transfer test between builds

global using AddOnCtx = drz.Src.Infrastructure.AddOnContext;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Lib_B;
using drz.Src.Infrastructure;


namespace drz.Lib_A

{
    public class CommandA
    {
        private readonly IDrzLogger _logger;//логгер

        //private readonly IAddOnServices _services;

        //private static bool _isAddOnCompositionRoot;//контейнер наполнен

        public CommandA(IAddOnServices services)
        {
            // экземпляр копии контейнера by ref
            AddOnCtx. Initialize(services);

            _logger = AddOnCtx.NLogFactory.GetLogger(typeof(CommandA));

            _logger.InfoCaller("CommandB Initialized");
        }

        public void Run()
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

            AddOnCtx.MsgGUI.InfoMessage($"End {nameof(CommandA)}");

            CommandB c = new CommandB(AddOnCtx.Services);
            c.Run();
        }
    }
}

