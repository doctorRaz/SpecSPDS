using System.Text.Json;
using dRz.Updater.Models;

namespace dRz.Updater.Services
{
    /// <summary>
    /// Читает описание обновления из update.json.
    /// </summary>
    public static class UpdateJsonReader
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>
        /// Читает и десериализует update.json.
        /// </summary>
        public static UpdateInfo Read(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл update.json не найден.", filePath);

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<UpdateInfo>(json, _options)
                ?? throw new InvalidDataException("Файл update.json не содержит данных обновления.");
        }
    }
}