// тест передачи контейнера между сборками
// container transfer test between builds

global using AddOnCtx = drz.Src.Infrastructure.AddOnContext;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Abstractions.Services.Message;
using drz.Lib_B;

namespace drz.Lib_A
{
    /// <summary>
    ///
    /// </summary>
    public class ContainerTransferA
    {
        #region Private Fields

        //логер
        private readonly IDrzLogger _logger;

        //ком строка
        private readonly ICommandLineMessageService? _msgCmd;

        #endregion Private Fields

        #region Internal Constructors

        /// <summary>Initializes a new instance of the <see cref="ContainerTransferA"/> class.</summary>
        /// <param name="services">The services.</param>
        internal ContainerTransferA(IAddOnServices services)
        {
            // экземпляр копии контейнера by ref сахарок
            //можно просто
            //  _services=services
            //      и получать интерфейсы по
            //          services.Get<T>
            AddOnCtx.Initialize(services);

            string msg = $"{nameof(ContainerTransferA)} Init";

            _logger = AddOnCtx.NLogFactory.GetLogger(typeof(ContainerTransferA));
            _logger.InfoCaller(msg);

            _msgCmd = AddOnCtx.MsgCmd;
            _msgCmd.InfoMessage(msg);
        }

        #endregion Internal Constructors

        #region Public Methods

        public static void Run(IAddOnServices services)
        {
            ContainerTransferA containerTransferA = new ContainerTransferA(services);
            containerTransferA.CommandA_Run();
        }

        /// <summary>Commands a run.</summary>
        internal void CommandA_Run()
        {
            string msg = $"{nameof(CommandA_Run)} Init";

            _logger.InfoCaller(msg);
            _msgCmd.InfoMessage(msg);

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

            _logger.Debug("ContainerTransferB.Run");
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

            ContainerTransferB.Run(AddOnCtx.Services);

            msg = $"{nameof(CommandA_Run)} end";
            _logger.Info(msg);
            _msgCmd.InfoMessage(msg);
        }

        #endregion Public Methods
    }
}