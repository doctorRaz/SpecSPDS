// тест передачи контейнера между сборками
// container transfer test between builds

global using AddOnCtx = drz.Src.Infrastructure.AddOnContext;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Lib_B;

namespace drz.Lib_A

{
    /// <summary>
    ///
    /// </summary>
    public class CommandA
    {
        #region Private Fields

        private readonly IDrzLogger _logger;

        #endregion Private Fields

        //логгер

        //private readonly IAddOnServices _services;

        //private static bool _isAddOnCompositionRoot;//контейнер наполнен

        #region Public Constructors

        /// <summary>Initializes a new instance of the <see cref="CommandA"/> class.</summary>
        /// <param name="services">The services.</param>
        public CommandA(IAddOnServices services)
        {
            // экземпляр копии контейнера by ref
            AddOnCtx.Initialize(services);

            _logger = AddOnCtx.NLogFactory.GetLogger(typeof(CommandA));

            _logger.InfoCaller("CommandB Initialized");
        }

        #endregion Public Constructors

        #region Public Methods

        /// <summary>Commands a run.</summary>
        public void CommandA_Run()
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

            //----- AddOnInfo ------
            _logger.Info(AddOnCtx.AddOnInfo.ToLongString());

            //----- SysInfo ------
            _logger.Info(AddOnCtx.SysInfo.ToLongString());

            CommandB c = new CommandB(AddOnCtx.Services);
            c.CommandB_Run();

            _logger.Info("The End A");
            AddOnCtx.MsgCmd.InfoMessage("The End A");
        }

        #endregion Public Methods
    }
}