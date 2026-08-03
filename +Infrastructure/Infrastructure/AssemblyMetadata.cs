using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace drz.Infrastructure.Infrastructure
{
    /// <summary>
    /// получает метаданные из атрибутов сборки и хранит их в словаре
    /// </summary>
    public class AssemblyMetadata
    {
        private readonly Dictionary<string, string> _metadata;

        //public IReadOnlyDictionary<string, string> Items => _metadata;

        public AssemblyMetadata(Assembly assembly)
        {
            _metadata = assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .ToDictionary(
                    x => x.Key,
                    x => x.Value ?? string.Empty,
                    StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGet(string key, out string value)
        {
            return _metadata.TryGetValue(key, out value);
        }

        public string? Get(string key)
        {
            return _metadata.TryGetValue(key, out var value)
                ? value
                : null;
        }



        public string Get(string key, string defaultValue)
        {
            return _metadata.TryGetValue(key, out var value)
                ? value
                : defaultValue;
        }

        public IEnumerable<string> Keys => _metadata.Keys;

        public IEnumerable<KeyValuePair<string, string>> Items => _metadata;

    }
}
