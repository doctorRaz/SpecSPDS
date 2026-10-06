using System.Text.Json.Serialization;

namespace dRz.Updater.Models
{
    /// <summary>
    /// Описание доступного обновления из update.json.
    /// </summary>
    public sealed class UpdateInfo
    {
        [JsonPropertyName("product")]
        public string Product { get; init; } = string.Empty;

        [JsonPropertyName("version")]
        public VersionInfo Version { get; init; } = new();

        [JsonPropertyName("tag")]
        public string Tag { get; init; } = string.Empty;

        [JsonPropertyName("asset")]
        public string Asset { get; init; } = string.Empty;

        [JsonPropertyName("sha256")]
        public string Sha256 { get; init; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; init; }

        [JsonPropertyName("commit")]
        public string Commit { get; init; } = string.Empty;

        [JsonPropertyName("mandatory")]
        public bool Mandatory { get; init; }

        [JsonPropertyName("minimumVersion")]
        public VersionInfo MinimumVersion { get; init; } = new();

        [JsonPropertyName("publishedAt")]
        public DateTimeOffset PublishedAt { get; init; }

        [JsonPropertyName("protected")]
        public ProtectedPackageInfo? Protected { get; init; }
    }
}