using drz.Abstractions.Infrastructure;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace drz.Infrastructure.Infrastructure
{
    /// <summary>
    /// Глобальный реестр информации о загруженных аддонах.
    ///
    /// Экземпляр существует один на процесс.
    /// Используется ConcurrentDictionary, поэтому безопасен
    /// при регистрации из нескольких потоков.
    /// </summary>
    public sealed class AddonInfoRegistry : IAddonInfoRegistry
    {
        /// <summary>
        /// Хранилище.
        ///
        /// Ключ - полное имя сборки
        /// Example:
        ///
        /// SpecSPDS, Version=2.3.0.0,
        /// Culture=neutral,
        /// PublicKeyToken=null
        /// </summary>
        private static readonly ConcurrentDictionary<string, IAddOnInfo> _addons = new();

        /// <inheritdoc/>
        public IAddOnInfo Register(Assembly assembly)
        {
            return _addons.GetOrAdd(assembly.Location /*assembly.FullName*/, key => new AddOnInfo(assembly));
        }

        /// <inheritdoc/>
        public bool TryGet(
            string assemblyFullName,
            out IAddOnInfo info)
        {
            return _addons.TryGetValue(
                assemblyFullName,
                out info!);
        }

        /// <inheritdoc/>
        public IReadOnlyCollection<IAddOnInfo> GetValues()
        {
            return _addons.Values.ToArray();
        }

        public ICollection<string> GetKeys()
        {
            return _addons.Keys;
        }
    }
}