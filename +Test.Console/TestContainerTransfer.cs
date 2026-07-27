// тест передачи контейнера между сборками
// container transfer test between builds

using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.AddOnRuntime;
using drz.Lib_A;
using System;

//using static drz.Src.Infrastructure.AddOnContext;

namespace drz.SpecSPDS.Test

{
    /// <summary>
    /// Выполняет однократную инициализацию контейнера сервисов
    /// и глобального контекста <see cref="AddOnCtx"/>.
    /// </summary>
    internal class TestContainerTransfer
    {
        private readonly IDrzLogger? _logger = AddOnCtx.NLogFactory.GetLogger(typeof(TestContainerTransfer));

        /// <summary>
        /// Инициализирует контейнер сервисов AddOn при первом создании экземпляра
        /// и записывает информацию об успешной инициализации в журнал.
        /// </summary>

        /// <summary>Containers the transfer run.</summary>
        internal void TestContainerTransfer_Run()
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

            CommandA c = new CommandA(AddOnCtx.Services);
            c.CommandA_Run();

            _logger.Info("The End TestContainerTransfer_Run");
            AddOnCtx.Msg.InfoMessage("The End TestContainerTransfer_Run");
        }
    }
}