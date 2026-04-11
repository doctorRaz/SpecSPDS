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

        private static AddOnCompositionRoot Root =>
            _root ?? throw new InvalidOperationException("DI is not initialized");

        /// <summary>
        /// Создать scope
        /// </summary>
        public static Scope BeginScope()
        {
            return Root.BeginScope();
        }

        public static void Dispose()
        {
            _root?.Dispose();
            _root = null;
        }

        /// <summary>
        /// Получить сервис
        /// </summary>
        public static T Get<T>() where T : class
        {
            return Root.Get<T>();
        }

        public static void Initialize(AddOnCompositionRoot root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }
    }
}