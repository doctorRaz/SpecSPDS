using drz.Abstractions.Infrastructure;
using drz.Abstractions.Services;
using drz.AddOn.Composition;
using drz.Cad.Diagnostics.AddOn;
using SimpleInjector;
using System;

namespace drz.Loader.Infrastructure
{
    internal static class AddOnContext
    {
        // metadata (всегда доступно)
        internal static readonly InfoAddOn InfoDll = InfoAddOn.Get(typeof(AddOnContext));

        //---R
        //***B

        // DI root (инициализируется отдельно)
        private static AddOnCompositionRoot? _root;

        internal static IDocumentService DocService => Root.Get<IDocumentService>();

        internal static IApplicationInfo AddOn => Root.Get<IApplicationInfo>();

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

        private static AddOnCompositionRoot Root => _root ?? throw new InvalidOperationException("AddOnCompositionRoot is not initialized");

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

        #region Спрятать методы

        /// <summary>
        /// Создать scope
        /// </summary>
        private static Scope BeginScope()
        {
            return Root.BeginScope() ?? throw new InvalidOperationException("AddOnCompositionRoot not initialized");
        }

        /// <summary>
        /// Получить сервис
        /// </summary>
        private static T Get<T>() where T : class
        {
            return Root.Get<T>() ?? throw new InvalidOperationException("AddOnCompositionRoot not initialized");
        }

        #endregion Спрятать методы
    }
}