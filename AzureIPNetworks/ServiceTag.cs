using System.Net;
using System.Text.Json.Serialization;

namespace AzureIPNetworks;

/// <summary>
/// Azure <see cref="ServiceTag"/>s for a given <see cref="AzureCloud"/>.
/// </summary>
public record CloudServiceTags
{
    /// <summary>The name of the cloud the data belongs to.</summary>
    [JsonPropertyName("cloud")]
    public AzureCloud Cloud { get; init; }

    /// <summary>ServiceTags for the cloud.</summary>
    [JsonPropertyName("values")]
    public IReadOnlyCollection<ServiceTag> Values { get; init; } = [];
}

/// <summary>Representation of an Azure ServiceTag.</summary>
public record ServiceTag
{
    /// <summary>The identifier for the service tag.</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>The name of the service tag.</summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>Properties for this service tag.</summary>
    [JsonPropertyName("properties")]
    public required ServiceTagProperties Properties { get; init; }
}

/// <summary>Properties of a <see cref="ServiceTag"/>.</summary>
public record ServiceTagProperties
{
    ///
    [JsonPropertyName("changeNumber")]
    public int ChangeNumber { get; init; }

    /// <summary>The name of the Azure region.</summary>
    [JsonPropertyName("region")]
    public required string Region { get; init; }

    ///
    [JsonPropertyName("regionId")]
    public int RegionId { get; init; }

    ///
    [JsonPropertyName("platform")]
    public required string Platform { get; init; }

    ///
    [JsonPropertyName("systemService")]
    public required string SystemService { get; init; }

    ///
    [JsonPropertyName("addressPrefixes")]
#if NET8_0_OR_GREATER
    public IReadOnlyCollection<IPNetwork> AddressPrefixes { get; init; } = [];
#else
    public IReadOnlyCollection<IPNetwork2> AddressPrefixes { get; init; } = [];
#endif

    ///
    [JsonPropertyName("networkFeatures")]
    public IReadOnlyCollection<string> NetworkFeatures { get; init; } = [];
}

///
[JsonConverter(typeof(JsonStringEnumConverter<AzureCloud>))]
public enum AzureCloud
{
    ///
    Public,

    ///
    China,

    ///
    AzureGovernment,

    ///
    AzureGermany,
}

/// <summary>
/// Known data collected from the files and used just for reference
/// </summary>
public static partial class KnownData { }
