using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.Abstractions.Services.Message;
using System;

namespace drz.Src.Infrastructure
{
    internal static class AddOnContext
    {
        #region Private Fields

        private static IAddOnServices? _services;

        #endregion Private Fields

        #region Internal Properties

        internal static IAddOnInfo AddOnInfo => Services.Get<IAddOnInfo>();
        internal static ICadInfo CadInfo => Services.Get<ICadInfo>();
        internal static ISysInfo SysInfo => Services.Get<ISysInfo>();

        internal static IMessageService Msg
        {
            get
            {
                if (DocService.IsActive)
                {
                    return Services.Get<ICommandLineMessageService>();
                }
                else
                {
                    return Services.Get<IWindowMessageService>();
                }
            }
        }

        internal static IDocumentService DocService => Services.Get<IDocumentService>();
        internal static IMessageService MsgCmd => Services.Get<ICommandLineMessageService>();
        internal static IMessageService MsgGUI => Services.Get<IWindowMessageService>();
        internal static IMessageService MsgMcn => Services.Get<IMcNotificatorMessageService>();
        internal static IDrzLoggerFactory NLogFactory => Services.Get<IDrzLoggerFactory>();

        #endregion Internal Properties

        #region Private Properties

        /// <summary>DI services (инициализируется отдельно)</summary>
        /// <value>The services.</value>
        /// <exception cref="System.InvalidOperationException">AddOnCompositionRoot is not initialized</exception>
        internal static IAddOnServices Services => _services ?? throw new InvalidOperationException("AddOnCompositionRoot is not initialized");

        #endregion Private Properties

        #region Internal Methods

        //todo переделать на фабрику??
        internal static IMessageService GetMessageService(MessageServiceType type)
        {
            switch (type)
            {
                case MessageServiceType.McNotifi:
                    return Services.Get<IMcNotificatorMessageService>();

                case MessageServiceType.CommandLine:
                    return Services.Get<ICommandLineMessageService>();

                case MessageServiceType.Window:
                    return Services.Get<IWindowMessageService>();

                case MessageServiceType.Default:
                    return DocService.IsActive
                        ? Services.Get<ICommandLineMessageService>()
                        : Services.Get<IWindowMessageService>();

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        /// <summary>
        /// Initializes the specified services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <exception cref="System.ArgumentNullException">services</exception>
        internal static void Initialize(IAddOnServices services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (_services != null)
                throw new InvalidOperationException("AddOnContext уже инициализирован.");

            _services = services;
        }

        #endregion Internal Methods
    }
}