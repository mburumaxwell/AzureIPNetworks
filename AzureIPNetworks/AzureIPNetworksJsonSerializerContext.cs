using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureIPNetworks;

[JsonSerializable(typeof(CloudServiceTags))]
internal partial class AzureIPNetworksJsonSerializerContext : JsonSerializerContext
{
    private static JsonSerializerOptions DefaultSerializerOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new IPNetworkJsonConverter(),
        },
    };

    static AzureIPNetworksJsonSerializerContext() => s_defaultContext = new AzureIPNetworksJsonSerializerContext(new JsonSerializerOptions(DefaultSerializerOptions));
}
