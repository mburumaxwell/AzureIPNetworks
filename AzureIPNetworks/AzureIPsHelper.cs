using System.Net;
using System.Text.Json;

namespace AzureIPNetworks;

/// <summary>
/// Helper methods for working with Azure IPs
/// </summary>
public static class AzureIPsHelper
{
    // caching is better that source generator since only the clouds that are used will be loaded
    // caching also reduces double allocations when parsing repeatedly
    private static readonly Dictionary<AzureCloud, CloudServiceTags> data = new(Enum.GetValues(typeof(AzureCloud)).Length);

    private static async ValueTask<CloudServiceTags> ParseFileAsync(AzureCloud cloud = AzureCloud.Public, CancellationToken cancellationToken = default)
    {
        if (!data.TryGetValue(cloud, out var ranges))
        {
            // read the JSON file from embedded resource
            var name = string.Join(".", typeof(CloudServiceTags).Namespace, "Resources", $"{cloud}.json");
            using var stream = typeof(CloudServiceTags).Assembly.GetManifestResourceStream(name)
                ?? throw new InvalidOperationException($"Stream '{name}' could not be found. Raise an issue on GitHub.");

            // deserialize the JSON file
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new IPNetworkJsonConverter(),
                    new System.Text.Json.Serialization.JsonStringEnumConverter(),
                },
            };
            data[cloud] = ranges = (await JsonSerializer.DeserializeAsync<CloudServiceTags>(stream, options, cancellationToken))!;
        }

        return ranges;
    }

    /// <summary>Get the Azure IP networks.</summary>
    /// <param name="cloud">The Azure Cloud to check in.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns></returns>
    public static async ValueTask<IEnumerable<ServiceTag>> GetServiceTagsAsync(AzureCloud cloud = AzureCloud.Public, CancellationToken cancellationToken = default)
        => (await ParseFileAsync(cloud, cancellationToken)).Values;

    /// <summary>Get the Azure IP networks.</summary>
    /// <param name="cloud">The Azure Cloud to check in.</param>
    /// <param name="service">
    /// The name of the service where to check.
    /// When not provided (<see langword="null"/>), networks from all services are checked.
    /// </param>
    /// <param name="region">
    /// The name of the region where to check.
    /// When not provided (<see langword="null"/>), networks from all regions are checked.
    /// </param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns></returns>
    public static async ValueTask<IEnumerable<IPNetwork>> GetNetworksAsync(AzureCloud cloud, string? service = null, string? region = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<ServiceTag> tags = cloud switch
        {
            AzureCloud.Public => await GetServiceTagsAsync(AzureCloud.Public, cancellationToken),
            AzureCloud.China => await GetServiceTagsAsync(AzureCloud.China, cancellationToken),
            AzureCloud.AzureGovernment => await GetServiceTagsAsync(AzureCloud.AzureGovernment, cancellationToken),
            AzureCloud.AzureGermany => await GetServiceTagsAsync(AzureCloud.AzureGermany, cancellationToken),
            _ => throw new NotImplementedException(),
        };

        // if the service name is provided, only retain networks for that service
        if (service is not null) tags = tags.Where(t => t.Properties?.SystemService == service);

        // if the region is provided, only retain networks for that region
        if (region is not null) tags = tags.Where(t => t.Properties?.Region == region);

        return tags.SelectMany(t => t.Properties?.AddressPrefixes);
    }

    /// <summary>Checks if the supplied IP address is an Azure IP.</summary>
    /// <param name="cloud">The Azure Cloud to check in.</param>
    /// <param name="address">The IP address to check against</param>
    /// <param name="service">
    /// The name of the service where to check.
    /// When not provided (<see langword="null"/>), networks from all services are checked.
    /// </param>
    /// <param name="region">
    /// The name of the region where to check.
    /// When not provided (<see langword="null"/>), networks from all regions are checked.
    /// </param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns></returns>
    public static async ValueTask<bool> IsAzureIpAsync(IPAddress address, AzureCloud cloud = AzureCloud.Public, string? service = null, string? region = null, CancellationToken cancellationToken = default)
        => Contained(await GetNetworksAsync(cloud, service, region, cancellationToken), address);

    /// <summary>Checks if the supplied IP network is an Azure IP.</summary>
    /// <param name="cloud">The Azure Cloud to check in.</param>
    /// <param name="network">The network check against</param>
    /// <param name="service">
    /// The name of the service where to check.
    /// When not provided (<see langword="null"/>), networks from all services are checked.
    /// </param>
    /// <param name="region">
    /// The name of the region where to check.
    /// When not provided (<see langword="null"/>), networks from all regions are checked.
    /// </param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns></returns>
    public static async ValueTask<bool> IsAzureIpAsync(IPNetwork network, AzureCloud cloud = AzureCloud.Public, string? service = null, string? region = null, CancellationToken cancellationToken = default)
        => Contained(await GetNetworksAsync(cloud, service, region, cancellationToken), network);

    static bool Contained(IEnumerable<IPNetwork> networks, IPAddress ipAddress) => networks.Any(n => n.Contains(ipAddress));
    static bool Contained(IEnumerable<IPNetwork> networks, IPNetwork network) => networks.Any(n => n.Contains(network));
}
