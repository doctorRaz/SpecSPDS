using drz.Abstractions.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace drz.Infrastructure.Infrastructure
{
    /// <summary>
    /// Глобальный реестр информации о загруженных сборках.
    /// Экземпляр существует один на процесс.
    /// Используется ConcurrentDictionary, поэтому безопасен
    /// при регистрации из нескольких потоков.
    /// </summary>
    public sealed class AddOnInfoRegistry : IAddOnInfoRegistry
    {
        /// <summary>
        /// Хранилище.
        /// Ключ - полный путь к файлу
        /// </summary>
        private static readonly ConcurrentDictionary<string, IAddOnInfo> _addons = new();

        public IAddOnInfo GetOrAdd(Assembly assembly)
        {
            return _addons.GetOrAdd(assembly.Location /*assembly.FullName*/, key => new AddOnInfo(assembly));
        }

        public IAddOnInfo GetOrAdd<T>() => GetOrAdd(typeof(T).Assembly);

        public IAddOnInfo GetOrAdd(Type type) => GetOrAdd(type.Assembly);


        public bool TryGet(
            string assemblyFullName,
            out IAddOnInfo info)
        {
            return _addons.TryGetValue(
                assemblyFullName,
                out info!);
        }

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