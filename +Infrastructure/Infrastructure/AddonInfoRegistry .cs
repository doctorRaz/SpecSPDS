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
    public class AddOnInfoRegistry : IAddOnInfoRegistry
    {
        #region Private Fields

        /// <summary>
        /// Хранилище.
        /// Ключ - полный путь к файлу
        /// </summary>
        private static readonly ConcurrentDictionary<string, IAddOnInfo> _addons = new();

        #endregion Private Fields

        #region Public Properties

        public int Count => _addons.Count;

        #endregion Public Properties

        #region Public Methods

        public ICollection<string> GetKeys()
        {
            return _addons.Keys;
        }

        public IAddOnInfo GetOrAdd(Assembly assembly)
        {
            return _addons.GetOrAdd(assembly.Location, key => new AddOnInfo(assembly));
        }

        public IAddOnInfo GetOrAdd<T>() => GetOrAdd(typeof(T).Assembly);

        public IAddOnInfo GetOrAdd(Type type) => GetOrAdd(type.Assembly);

        public IReadOnlyCollection<IAddOnInfo> GetValues()
        {
            return _addons.Values.ToArray();
        }

        public bool TryGet(Assembly assembly, out IAddOnInfo info)
        {
            return _addons.TryGetValue(
                assembly.Location,
                out info!);
        }

        /// <summary>
        /// Пытается получить информацию об аддоне по типу.
        /// </summary>
        /// <typeparam name="T">
        /// Тип, определяющий сборку, для которой требуется получить информацию.
        /// </typeparam>
        /// <param name="info">
        /// При успешном выполнении содержит информацию об аддоне.
        /// </param>
        /// <returns>
        /// <see langword="true"/>, если информация найдена; иначе <see langword="false"/>.
        /// </returns>
        public bool TryGet<T>(out IAddOnInfo info)
        {
            return TryGet(
                typeof(T).Assembly,
                out info);
        }

        /// <summary>
        /// Пытается получить информацию об аддоне по типу.
        /// </summary>
        /// <param name="type">
        /// Тип, определяющий сборку, для которой требуется получить информацию.
        /// </param>
        /// <param name="info">
        /// При успешном выполнении содержит информацию об аддоне.
        /// </param>
        /// <returns>
        /// <see langword="true"/>, если информация найдена; иначе <see langword="false"/>.
        /// </returns>
        public bool TryGet(Type type, out IAddOnInfo info)
        {
            return TryGet(type.Assembly,
                out info);
        }

        #endregion Public Methods
    }
}