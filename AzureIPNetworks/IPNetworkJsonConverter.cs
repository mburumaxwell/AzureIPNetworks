using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AzureIPNetworks;

/// <summary>
/// A <see cref="JsonConverter{T}"/> for <c>IPNetwork</c> and <c>IPNetwork2</c>
/// </summary>
#if NET8_0_OR_GREATER
internal class IPNetworkJsonConverter : JsonConverter<IPNetwork>
#else
internal class IPNetworkJsonConverter : JsonConverter<IPNetwork2>
#endif
{
    ///// <inheritdoc/>
#if NET8_0_OR_GREATER
    public override IPNetwork Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
#else
    public override IPNetwork2? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
#endif
    {
        var s = reader.GetString();
#if NET8_0_OR_GREATER
        return string.IsNullOrWhiteSpace(s) ? default : IPNetwork.Parse(s);
#else
        return string.IsNullOrWhiteSpace(s) ? default : IPNetwork2.Parse(s);
#endif
    }

    /// <inheritdoc/>
#if NET8_0_OR_GREATER
    public override void Write(Utf8JsonWriter writer, IPNetwork value, JsonSerializerOptions options)
#else
    public override void Write(Utf8JsonWriter writer, IPNetwork2 value, JsonSerializerOptions options)
#endif
    {
        writer.WriteStringValue(value.ToString());
    }
}
