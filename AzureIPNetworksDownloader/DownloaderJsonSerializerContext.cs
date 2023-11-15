using System.Text.Json.Serialization;

namespace AzureIPNetworks;

[JsonSerializable(typeof(Dictionary<AzureCloud, string>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class DownloaderJsonSerializerContext : JsonSerializerContext { }
