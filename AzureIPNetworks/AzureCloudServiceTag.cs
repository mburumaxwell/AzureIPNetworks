using System.Net;

namespace AzureIPNetworks;

/// <summary>Represents a service tag for the Azure Cloud.</summary>
public sealed class AzureCloudServiceTag
{
    ///
    public AzureCloudServiceTag(string? region, string platform, string? service, IReadOnlyCollection<IPNetwork> addressPrefixes)
    {
        if (string.IsNullOrWhiteSpace(Platform = platform))
        {
            throw new ArgumentException($"'{nameof(platform)}' cannot be null or whitespace.", nameof(platform));
        }

        Region = region;
        Service = service;
        AddressPrefixes = addressPrefixes ?? throw new ArgumentNullException(nameof(addressPrefixes));
    }

    /// <summary>The name of the Azure region.</summary>
    public string? Region { get; }

    ///
    public string Platform { get; }

    /// <summary>The name of the service for the tag.</summary>
    public string? Service { get; }

    /// <summary>The network subnets specifying the IPs in range.</summary>
    public IReadOnlyCollection<IPNetwork> AddressPrefixes { get; }

    /// <summary>The subnets for the Public Cloud.</summary>
    public static IReadOnlyCollection<IPNetwork> PublicCloud { get; } = AzureCloudServiceTags.PublicCloud.Last().AddressPrefixes;

    /// <summary>
    /// Name of services whose networks are known.
    /// For example: <c>AzureApiManagement</c>, <c>PowerBI</c>, etc.
    /// </summary>
    public static IReadOnlyCollection<string> ServiceNames { get; } = AzureCloudServiceTags.ServiceNames;

    /// <summary>
    /// Regions of services whose networks are known.
    /// For example: <c>australiacentral</c>, <c>westeurope</c>, etc.
    /// </summary>
    public static IReadOnlyCollection<string> Regions { get; } = AzureCloudServiceTags.Regions;

    /// <summary>Checks if the supplied IP address is used in the Public Cloud.</summary>
    /// <param name="address">The IP address to check against</param>
    /// <param name="service">
    /// The name of the service where to check.
    /// When not provided (<see langword="null"/>), IPs from all services are checked.
    /// For the list of known services see <see cref="ServiceNames"/>.
    /// </param>
    /// <param name="region">
    /// The name of the region where to check.
    /// When not provided (<see langword="null"/>), IPs from all regions are checked.
    /// For the list of known regions see <see cref="Regions"/>.
    /// </param>
    public static bool IsPublicCloudIP(IPAddress address, string? service = null, string? region = null)
    {
        if (service is not null && region is not null)
        {
            IEnumerable<AzureCloudServiceTag> tags = AzureCloudServiceTags.PublicCloud;

            // if the service name is provided, only retain networks for that service
            if (service != null) tags = tags.Where(t => t.Service == service);

            // if the region is provided, only retain networks for that region
            if (region != null) tags = tags.Where(t => t.Region == region);

            return Contained(tags.SelectMany(t => t.AddressPrefixes), address);
        }

        return Contained(PublicCloud, address);
    }

    /// <summary>Checks if the supplied IP network is used in the Public Cloud.</summary>
    /// <param name="network">The network check against</param>
    /// <param name="service">
    /// The name of the service where to check.
    /// When not provided (<see langword="null"/>), networks from all services are checked.
    /// For the list of known services see <see cref="ServiceNames"/>.
    /// </param>
    /// <param name="region">
    /// The name of the region where to check.
    /// When not provided (<see langword="null"/>), networks from all regions are checked.
    /// For the list of known regions see <see cref="Regions"/>.
    /// </param>
    public static bool IsPublicCloudIP(IPNetwork network, string? service = null, string? region = null)
    {
        if (service is not null && region is not null)
        {
            IEnumerable<AzureCloudServiceTag> tags = AzureCloudServiceTags.PublicCloud;

            // if the service name is provided, only retain networks for that service
            if (service != null) tags = tags.Where(t => t.Service == service);

            // if the region is provided, only retain networks for that region
            if (region != null) tags = tags.Where(t => t.Region == region);

            return Contained(tags.SelectMany(t => t.AddressPrefixes), network);
        }

        return Contained(PublicCloud, network);
    }

    static bool Contained(IEnumerable<IPNetwork> networks, IPAddress ipAddress) => networks.Any(n => n.Contains(ipAddress));
    static bool Contained(IEnumerable<IPNetwork> networks, IPNetwork network) => networks.Any(n => n.Contains(network));
}

[AzureIPNetworksGenerator.GenerateIpNetworksAttribute]
internal static partial class AzureCloudServiceTags { }
