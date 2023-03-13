using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureIPNetworks;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for <see cref="IPNetwork"/>
/// </summary>
internal class IPNetworkJsonConverter : JsonConverter<IPNetwork>
{
    /// <inheritdoc/>
    public override IPNetwork? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString();
        return string.IsNullOrWhiteSpace(s) ? null : IPNetwork.Parse(s);
    }

    /// <inheritdoc/>
    public override void Write(Utf8JsonWriter writer, IPNetwork value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
