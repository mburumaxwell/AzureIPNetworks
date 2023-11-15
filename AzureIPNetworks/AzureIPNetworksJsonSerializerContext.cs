using System.Text.Json.Serialization;

namespace AzureIPNetworks;

[JsonSerializable(typeof(CloudServiceTags))]
[JsonSourceGenerationOptions(Converters = [typeof(IPNetworkJsonConverter)])]
internal partial class AzureIPNetworksJsonSerializerContext : JsonSerializerContext { }
