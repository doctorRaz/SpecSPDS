// тест передачи контейнера между сборками
// container transfer test between builds

global using AddOnCtx = drz.Src.Infrastructure.AddOnContext;
using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Abstractions.Services.Message;
using System;

namespace drz.Lib_B

{
    /// <summary>
    /// Отдельная сборка о ней знает только А
    /// </summary>
    public class ContainerTransferB
    {
        #region Private Fields

        //логер
        private readonly IDrzLogger _logger;

        //ком строка
        private readonly ICommandLineMessageService _msgCmd;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>Initializes a new instance of the <see cref="ContainerTransferB"/> class.</summary>
        /// <param name="services">The services.</param>
        internal ContainerTransferB(IAddOnServices services)
        {
            // экземпляр копии контейнера by ref
            AddOnCtx.Initialize(services);

            string msg = $"{nameof(ContainerTransferB)} Init";

            _logger = AddOnCtx.NLogFactory.GetLogger(typeof(ContainerTransferB));
            _logger.InfoCaller(msg);

            _msgCmd = AddOnCtx.MsgCmd;
            _msgCmd.InfoMessage(msg);
        }

        #endregion Public Constructors

        #region Public Methods

        public static void Run(IAddOnServices services)
        {
            ContainerTransferB containerTransferB = new ContainerTransferB(services);
            containerTransferB.CommandB_Run();
        }

        /// <summary>Commands the b run.</summary>
        internal void CommandB_Run()
        {
            string msg = $"{nameof(CommandB_Run)} Init";
                     

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

            //если подменить сборку аддона, то покажет новый номер версии
            _msgCmd.InfoMessage(AddOnCtx.AddOnInfo.InstalledVersion.ToString());

            _msgCmd.InfoMessage($"Замените сборку аддона {AddOnCtx.AddOnInfo.AssemblyPath} на одноименную любую сборку другой версии");
            _msgCmd.InfoMessage("И нажмите любую клавишу");
            Console.ReadKey();
            _msgCmd.InfoMessage(AddOnCtx.AddOnInfo.InstalledVersion.ToString());

            msg = $"{nameof(CommandB_Run)} end";
            _logger.Info(msg);
            _msgCmd.InfoMessage(msg);
        }

        #endregion Public Methods
    }
}