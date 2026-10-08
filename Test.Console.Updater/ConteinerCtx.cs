using dRz.Abstractions.Logger;
using dRz.Abstractions.Services;
using dRz.Abstractions.Services.Message;
using dRz.AddOnRuntime;
using System;

namespace dRz.Test.Console
{
    internal class ConteinerCtx
    {
        private static bool _isAddOnCompositionRoot;
        private readonly IDrzLogger? _logger;
        private readonly ICommandLineMessageService? _msgCmd;

        internal ConteinerCtx()
        {
            try
            {
                if (_isAddOnCompositionRoot)
                {
                    return;
                }

                //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
                // один раз в точке входа /Rtm.IExtensionApplication/
                AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(ConteinerCtx).Assembly);

                // экземпляр копии контейнера by ref
                AddOnCtx.Initialize(root.Get<IAddOnServices>());
            }
            catch (Exception ex)
            {
                //роняем загрузчик
                throw new InvalidOperationException("AddOnCompositionRoot initialization failed", ex);
            }
        }

        public static void Run()
        {
            ConteinerCtx conteineNlog = new ConteinerCtx();
        }
    }
}