using System.Net;
using System.Text.Json.Serialization;

namespace AzureIPNetworks;

/// <summary>
/// Azure <see cref="ServiceTag"/>s for a given <see cref="AzureCloud"/>.
/// </summary>
public class CloudServiceTags
{
    /// <summary>The name of the cloud the data belongs to.</summary>
    [JsonPropertyName("cloud")]
    public string? Cloud { get; set; }

    /// <summary>ServiceTags for the cloud.</summary>
    [JsonPropertyName("values")]
    public IReadOnlyCollection<ServiceTag> Values { get; set; } = new List<ServiceTag>();
}

/// <summary>Representation of an Azure ServiceTag.</summary>
public class ServiceTag
{
    /// <summary>The identifier for the service tag.</summary>
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>The name of the service tag.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>Properties for this service tag.</summary>
    [JsonPropertyName("properties")]
    public ServiceTagProperties? Properties { get; set; }
}

/// <summary>Properties of a <see cref="ServiceTag"/>.</summary>
public class ServiceTagProperties
{
    /// <summary>The name of the Azure region.</summary>
    [JsonPropertyName("region")]
    public string? Region { get; set; }

    ///
    [JsonPropertyName("platform")]
    public string? Platform { get; set; }

    ///
    [JsonPropertyName("systemService")]
    public string? SystemService { get; set; }

    ///
    [JsonPropertyName("addressPrefixes")]
    public IReadOnlyCollection<IPNetwork> AddressPrefixes { get; set; } = new List<IPNetwork>();
}
