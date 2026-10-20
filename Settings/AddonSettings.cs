using dRz.Abstractions.Settings;
using System;

namespace dRz.Settings
{
    /// <summary>Корневой объект настроек аддона.</summary>
    public sealed class AddonSettings : IAddonSettings
    {
        private JsonSettingsStore? _store;

        /// <summary>Настройки обновления.</summary>
        public UpdateSettings Updates { get; set; } = new UpdateSettings();

        IUpdateSettings IAddonSettings.Updates => Updates;

        /// <inheritdoc />
        public void Save()
        {
            JsonSettingsStore store = _store ?? throw new InvalidOperationException("Хранилище настроек не подключено.");
            store.Save(this);
        }

        internal void AttachStore(JsonSettingsStore store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            Updates ??= new UpdateSettings();
        }
    }
}