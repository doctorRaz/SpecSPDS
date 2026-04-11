using SimpleInjector;
using System;

namespace drz.AddOn.Composition
{
    /// <summary>
    /// Контекст аддона (1 контейнер на сборку)
    /// </summary>
    public static class AddOnContextRoot
    {
        private static AddOnCompositionRoot? _root;

        public static void Initialize(AddOnCompositionRoot root)
        {
            _root = root ?? throw new ArgumentNullException(nameof(root));
        }

        private static AddOnCompositionRoot Root =>
            _root ?? throw new InvalidOperationException("DI is not initialized");

        /// <summary>
        /// Получить сервис
        /// </summary>
        public static T Get<T>() where T : class
        {
            return Root.Get<T>();
        }

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
    }
}