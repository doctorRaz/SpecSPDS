// тест передачи контейнера между сборками
// container transfer test between builds

using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.AddOnRuntime;
using drz.Lib_A;
using System.Diagnostics;

//using static drz.Src.Infrastructure.AddOnContext;

namespace drz.SpecSPDS.Test

{
    /// <summary>
    /// Выполняет однократную инициализацию контейнера сервисов
    /// и глобального контекста <see cref="AddOnCtx"/>.
    /// </summary>
    internal class ContainerTransfer
    {
        #region Private Fields

        private static bool _isAddOnCompositionRoot;
        private readonly IDrzLogger _logger;

        #endregion Private Fields

        //логгер

        //контейнер создан

        #region Internal Constructors

        /// <summary>
        /// Инициализирует контейнер сервисов AddOn при первом создании экземпляра
        /// и записывает информацию об успешной инициализации в журнал.
        /// </summary>
        internal ContainerTransfer()

        {
            //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
            // один раз в точке входа /Rtm.IExtensionApplication/
            AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(ContainerTransfer).Assembly);

            // экземпляр копии контейнера by ref
            AddOnCtx.Initialize(root.Get<IAddOnServices>());

            _logger = AddOnCtx.NLogFactory.GetLogger(typeof(ContainerTransfer));

            _logger.InfoCaller("Initialized");
            _isAddOnCompositionRoot = true;//сервис поднялся
        }

        #endregion Internal Constructors

        #region Internal Methods

        /// <summary>Containers the transfer run.</summary>
        internal void ContainerTransfer_Run()
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

            AddOnCtx.MsgGUI.InfoMessage($"End {nameof(ContainerTransfer)}");

            CommandA c = new CommandA(AddOnCtx.Services);
            c.CommandA_Run();
        }

        #endregion Internal Methods
    }
}