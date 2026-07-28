// тест передачи контейнера между сборками
// container transfer test between builds

using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Abstractions.Services.Message;
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
    internal class ContainerTransfer
    {
        //логер
        private readonly IDrzLogger? _logger;

        //ком строка
        private readonly ICommandLineMessageService? _msgCmd;

        private static bool _isAddOnCompositionRoot;

        /// <summary>
        /// создаем контейнер и регистрируем AddOnCtx static global using AddOnCtx = drz.Src.Infrastructure.AddOnContext;
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        internal ContainerTransfer()
        {
            try
            {
                if (_isAddOnCompositionRoot) return;

                //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
                // один раз в точке входа /Rtm.IExtensionApplication/
                AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(ContainerTransfer).Assembly);

                // экземпляр копии контейнера by ref
                AddOnCtx.Initialize(root.Get<IAddOnServices>());

                string msg = $"{nameof(ContainerTransfer)} Init";

                //логер
                _logger = AddOnCtx.NLogFactory.GetLogger(typeof(ContainerTransfer));
                _logger.InfoCaller(msg);

                //ком строка
                _msgCmd = AddOnCtx.MsgCmd;
                _msgCmd.InfoMessage(msg);

                _isAddOnCompositionRoot=true;   
            }
            catch (Exception ex)
            {
                //роняем загрузчик, без контейнера работы не будет
                throw new InvalidOperationException("AddOnCompositionRoot initialization failed", ex);
            }
        }

        /// <summary>
        /// Инициализирует контейнер сервисов AddOn при первом создании экземпляра
        /// и записывает информацию об успешной инициализации в журнал.
        /// </summary>

        /// <summary>Containers the transfer run.</summary>
        internal void TestContainerTransfer_Run()
        {

            string msg = $"{nameof(TestContainerTransfer_Run)} Init";

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

            //запускаем цепочку библиотек с передачей друг другу AddOnCtx.Services
            ContainerTransferA.Run(AddOnCtx.Services); 

            msg = $"{nameof(TestContainerTransfer_Run)} end";
            _logger.Info(msg);
            _msgCmd.InfoMessage(msg);
        }

        internal static void Run()
        {
            ContainerTransfer testContainerTransfer = new ContainerTransfer();
            testContainerTransfer.TestContainerTransfer_Run();
        }
    }
}