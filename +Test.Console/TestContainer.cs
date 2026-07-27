using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.AddOnRuntime;
using System;

namespace drz.SpecSPDS.Test
{
    internal class TestContainer
    {
        private static bool _isAddOnCompositionRoot;
        private readonly IDrzLogger? _logger;

        internal TestContainer()

        {
            try
            {
                if (_isAddOnCompositionRoot) return;

                //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
                // один раз в точке входа /Rtm.IExtensionApplication/
                AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(TestContainerTransfer).Assembly);

                // экземпляр копии контейнера by ref
                AddOnCtx.Initialize(root.Get<IAddOnServices>());

                _logger = AddOnCtx.NLogFactory.GetLogger(typeof(TestContainerTransfer));

                _logger.InfoCaller("Initialized");
                _isAddOnCompositionRoot = true;//сервис поднялся
            }
            catch (Exception ex)
            {
                //роняем загрузчик
                throw new InvalidOperationException("AddOnCompositionRoot initialization failed", ex);
            }
        }
    }
}