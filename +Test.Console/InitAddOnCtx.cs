using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.AddOnRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace drz.SpecSPDS.Test
{
    /// <summary>
    /// Выполняет однократную инициализацию контейнера сервисов 
    /// и глобального контекста <see cref="AddOnCtx"/>.
    /// </summary>
    internal class InitAddOnCtx
    {
        private static bool _isAddOnCompositionRoot;//контейнер наполнен

        /// <summary>
        /// Инициализирует контейнер сервисов AddOn при первом создании экземпляра
        /// и записывает информацию об успешной инициализации в журнал.
        /// </summary>
        internal InitAddOnCtx()
        {
            if (!_isAddOnCompositionRoot)
            {
                //***** РЕГИСТРИРУЕМ СЕРВИСЫ *************
                // один раз в точке входа /Rtm.IExtensionApplication/
                AddOnCompositionRoot root = new AddOnCompositionRoot(typeof(InitAddOnCtx).Assembly);

                // экземпляр копии контейнера by ref
                AddOnCtx.Initialize(root.Get<IAddOnServices>());

                _isAddOnCompositionRoot = true;//сервис поднялся
            }

            IDrzLogger _logger = AddOnCtx.NLogFactory.GetLogger(typeof(InitAddOnCtx));

            _logger.InfoCaller("Initialized");
        }
    }
}