﻿using System.Net;
using System.Text.Json;

namespace AzureIPNetworks;

/// <summary>
/// Helper methods for working with Azure IPs
/// </summary>
[Obsolete("Move to AzureIPs Provider instead")]
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
            data[cloud] = ranges = (await JsonSerializer.DeserializeAsync(stream, AzureIPNetworksJsonSerializerContext.Default.CloudServiceTags, cancellationToken))!;
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
#if NET8_0_OR_GREATER
    public static async ValueTask<IEnumerable<IPNetwork>> GetNetworksAsync(AzureCloud cloud = AzureCloud.Public, string? service = null, string? region = null, CancellationToken cancellationToken = default)
#else
    public static async ValueTask<IEnumerable<IPNetwork2>> GetNetworksAsync(AzureCloud cloud = AzureCloud.Public, string? service = null, string? region = null, CancellationToken cancellationToken = default)
#endif
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

#if NET8_0_OR_GREATER
        return tags.SelectMany(t => t.Properties?.AddressPrefixes ?? Array.Empty<IPNetwork>());
#else
        return tags.SelectMany(t => t.Properties?.AddressPrefixes ?? Array.Empty<IPNetwork2>());
#endif
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

#if !NET8_0_OR_GREATER
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
    public static async ValueTask<bool> IsAzureIpAsync(IPNetwork2 network, AzureCloud cloud = AzureCloud.Public, string? service = null, string? region = null, CancellationToken cancellationToken = default)
        => Contained(await GetNetworksAsync(cloud, service, region, cancellationToken), network);
#endif

#if NET8_0_OR_GREATER
    static bool Contained(IEnumerable<IPNetwork> networks, IPAddress ipAddress) => networks.Any(n => n.Contains(ipAddress));
#else
    static bool Contained(IEnumerable<IPNetwork2> networks, IPAddress ipAddress) => networks.Any(n => n.Contains(ipAddress));
#endif

#if !NET8_0_OR_GREATER
    static bool Contained(IEnumerable<IPNetwork2> networks, IPNetwork2 network) => networks.Any(n => n.Contains(network));
#endif
}
