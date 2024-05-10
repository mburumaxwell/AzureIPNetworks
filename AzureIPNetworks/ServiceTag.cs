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
    public string? Id { get; init; }

    /// <summary>The name of the service tag.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Properties for this service tag.</summary>
    [JsonPropertyName("properties")]
    public ServiceTagProperties? Properties { get; init; }
}

/// <summary>Properties of a <see cref="ServiceTag"/>.</summary>
public record ServiceTagProperties
{
    ///
    [JsonPropertyName("changeNumber")]
    public int ChangeNumber { get; init; }

    /// <summary>The name of the Azure region.</summary>
    [JsonPropertyName("region")]
    public string? Region { get; init; }

    ///
    [JsonPropertyName("regionId")]
    public int RegionId { get; init; }

    ///
    [JsonPropertyName("platform")]
    public string? Platform { get; init; }

    ///
    [JsonPropertyName("systemService")]
    public string? SystemService { get; init; }

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
