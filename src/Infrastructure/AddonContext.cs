using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using drz.AddOn.Composition;

using System;

namespace drz.Src.Infrastructure
{
    internal static class AddOnContext
    {
        // metadata (всегда доступно)
        //internal static readonly AppInfo InfoDll = AppInfo.Get(typeof(AddOnContext));

        //---R
        //***B

        // DI root (инициализируется отдельно)
        private static AddOnCompositionRoot? _root;


        //internal static IApplicationInfo AddOn => Root.Get<IApplicationInfo>();

        internal static IAddOnInfo InfoDll => Root.Get<IAddOnInfo>();

        internal static ISysInfo SysInfo => Root.Get<ISysInfo>();

        internal static ICadInfo CadInfo => Root.Get<ICadInfo>();

        internal static IDocumentService DocService => Root.Get<IDocumentService>();

        internal static IMessageService MsgCmd => Root.Get<ICommandLineMessageService>();

        internal static IMessageService MsgGUI => Root.Get<IWindowMessageService>();

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

        /// <summary>DI root (инициализируется отдельно)</summary>
        /// <value>The root.</value>
        /// <exception cref="System.InvalidOperationException">AddOnCompositionRoot is not initialized</exception>
        private static AddOnCompositionRoot Root => _root ?? throw new InvalidOperationException("AddOnCompositionRoot is not initialized");

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        internal static void Dispose()
        {
            _root?.Dispose();
            _root = null;
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
    }
}