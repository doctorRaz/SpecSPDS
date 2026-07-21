using drz.Abstractions.Infrastructure;
using drz.Abstractions.Logger;
using drz.Abstractions.Services;
using drz.AddOnRuntime;

using System;

namespace drz.Src.Infrastructure
{
    internal static class AddOnContext
    {
        #region Private Fields

        // DI root (инициализируется отдельно)
        private static AddOnCompositionRoot? _root;

        #endregion Private Fields

        #region Internal Properties

        internal static IAddOnInfo AddOnInfo => Root.Get<IAddOnInfo>();

        internal static ICadInfo CadInfo => Root.Get<ICadInfo>();
        internal static IDocumentService DocService => Root.Get<IDocumentService>();

        internal static IMessageService Msg
        {
            get
            {
                if (DocService.IsActive)
                {
                    return Root.Get<ICommandLineMessageService>();
                }
                else
                {
                    return Root.Get<IWindowMessageService>();
                }
            }
        }

        internal static IMessageService MsgCmd => Root.Get<ICommandLineMessageService>();
        internal static IMessageService MsgGUI => Root.Get<IWindowMessageService>();
        internal static IMessageService MsgMcn => Root.Get<IMcNotificatorMessageService>();
        internal static IDrzLoggerFactory NLogFactory => Root.Get<IDrzLoggerFactory>();
        internal static ISysInfo SysInfo => Root.Get<ISysInfo>();

        #endregion Internal Properties

        #region Private Properties

        /// <summary>DI root (инициализируется отдельно)</summary>
        /// <value>The root.</value>
        /// <exception cref="System.InvalidOperationException">AddOnCompositionRoot is not initialized</exception>
        private static AddOnCompositionRoot Root => _root ?? throw new InvalidOperationException("AddOnCompositionRoot is not initialized");

        #endregion Private Properties

        #region Internal Methods

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        internal static void Dispose()
        {
            _root?.Dispose();
            _root = null;
        }

        //todo переделать нга фабрику
        internal static IMessageService GetMessageService(MessageServiceType type)
        {
            switch (type)
            {
                case MessageServiceType.McNotifi:
                    return Root.Get<IMcNotificatorMessageService>();

                case MessageServiceType.CommandLine:
                    return Root.Get<ICommandLineMessageService>();

                case MessageServiceType.Window:
                    return Root.Get<IWindowMessageService>();

                case MessageServiceType.Default:
                    return DocService.IsActive
                        ? Root.Get<ICommandLineMessageService>()
                        : Root.Get<IWindowMessageService>();

                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        /// <summary>
        /// Initializes the specified root.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <exception cref="System.ArgumentNullException">root</exception>
        internal static void Initialize(AddOnCompositionRoot root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }

        #endregion Internal Methods
    }
}