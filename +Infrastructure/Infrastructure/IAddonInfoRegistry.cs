using System.Collections.Generic;
using System.Reflection;

namespace drz.Abstractions.Infrastructure
{
    /// <summary>
    /// Глобальный реестр информации о загруженных аддонах.
    /// Один экземпляр существует на процесс.
    /// </summary>
    public interface IAddOnInfoRegistry
    {
        /// <summary>
        /// Регистрирует информацию об аддоне.
        /// Если запись уже существует, возвращает существующий объект.
        /// </summary>
        IAddOnInfo GetOrAdd(Assembly assembly);

        /// <summary>
        /// Возвращает информацию по полному пути сборки.
        /// </summary>
        bool TryGet(string assemblyFullName, out IAddOnInfo info);

        /// <summary>
        /// Возвращает все зарегистрированные аддоны.
        /// </summary>
        IReadOnlyCollection<IAddOnInfo> GetValues();

        /// <summary>
        /// Возвращает все пути (ключи) к зарегистрированным аддонам
        /// </summary>
        ICollection<string> GetKeys();
    }
}