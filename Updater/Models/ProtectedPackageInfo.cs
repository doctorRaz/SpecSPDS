using System.Text.Json.Serialization;

namespace dRz.Updater.Models
{
    /// <summary>
    /// Описание защищённого пакета обновления.
    /// </summary>
    public sealed class ProtectedPackageInfo
    {
        [JsonPropertyName("asset")]
        public string Asset { get; init; } = string.Empty;

        [JsonPropertyName("sha256")]
        public string Sha256 { get; init; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; init; }

        [JsonPropertyName("format")]
        public string Format { get; init; } = string.Empty;

        [JsonPropertyName("encryption")]
        public string Encryption { get; init; } = string.Empty;

        [JsonPropertyName("encryptedHeaders")]
        public bool EncryptedHeaders { get; init; }

        [JsonPropertyName("password")]
        public string Password { get; init; } = string.Empty;
    }
}