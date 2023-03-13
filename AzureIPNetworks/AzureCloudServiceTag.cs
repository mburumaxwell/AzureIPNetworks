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
    public static IReadOnlyCollection<IPNetwork> AllClouds { get; } = new[] {
        AzureCloudServiceTags.PublicCloud.Last().AddressPrefixes,
        AzureCloudServiceTags.ChinaCloud.Last().AddressPrefixes,
        AzureCloudServiceTags.AzureGovernmentCloud.Last().AddressPrefixes,
        AzureCloudServiceTags.AzureGermanyCloud.Last().AddressPrefixes,
    }.SelectMany(p => p).ToArray();

    /// <summary>The subnets for the Public Cloud.</summary>
    public static IReadOnlyCollection<IPNetwork> PublicCloud { get; } = AzureCloudServiceTags.PublicCloud.Last().AddressPrefixes;

    /// <summary>The subnets for the Public Cloud.</summary>
    public static IReadOnlyCollection<IPNetwork> ChinaCloud { get; } = AzureCloudServiceTags.ChinaCloud.Last().AddressPrefixes;

    /// <summary>The subnets for the Public Cloud.</summary>
    public static IReadOnlyCollection<IPNetwork> AzureGovernmentCloud { get; } = AzureCloudServiceTags.AzureGovernmentCloud.Last().AddressPrefixes;

    /// <summary>The subnets for the Public Cloud.</summary>
    public static IReadOnlyCollection<IPNetwork> AzureGermanyCloud { get; } = AzureCloudServiceTags.AzureGermanyCloud.Last().AddressPrefixes;

    /// <summary>
    /// Name of cloud whose networks are known.
    /// For example: <c>Public</c>, <c>China</c>, etc.
    /// </summary>
    public static IReadOnlyCollection<string> CloudNames { get; } = new[] { "Public", "China", "AzureGovernment", "AzureGermany", };

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
    /// <param name="cloud">The Azure Cloud to check in.</param>
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
    public static bool IsAzureIP(IPAddress address, string cloud = "Public", string? service = null, string? region = null)
        => Contained(Filter(cloud, service, region), address);

    /// <summary>Checks if the supplied IP network is used in the Public Cloud.</summary>
    /// <param name="network">The network check against.</param>
    /// <param name="cloud">The Azure Cloud to check in.</param>
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
    public static bool IsAzureIP(IPNetwork network, string cloud = "Public", string? service = null, string? region = null)
        => Contained(Filter(cloud, service, region), network);

    static IEnumerable<IPNetwork> Filter(string cloud, string? service, string? region)
    {
        if (!CloudNames.Contains(cloud))
        {
            throw new ArgumentOutOfRangeException(nameof(cloud), $"'{cloud}' is not a known Cloud.");
        }

        if (service is not null && !ServiceNames.Contains(service))
        {
            throw new ArgumentOutOfRangeException(nameof(service), $"'{service}' is not a known Service.");
        }

        if (region is not null && !Regions.Contains(region))
        {
            throw new ArgumentOutOfRangeException(nameof(region), $"'{region}' is not a known Region.");
        }

        IEnumerable<AzureCloudServiceTag> tags = cloud switch
        {
            "Public" => AzureCloudServiceTags.PublicCloud,
            "China" => AzureCloudServiceTags.ChinaCloud,
            "AzureGovernment" => AzureCloudServiceTags.AzureGovernmentCloud,
            "AzureGermany" => AzureCloudServiceTags.AzureGermanyCloud,
            _ => new[] {
                AzureCloudServiceTags.PublicCloud,
                AzureCloudServiceTags.ChinaCloud,
                AzureCloudServiceTags.AzureGovernmentCloud,
                AzureCloudServiceTags.AzureGermanyCloud,
            }.SelectMany(t => t),
        };

        // if the service name is provided, only retain networks for that service
        if (service is not null) tags = tags.Where(t => t.Service == service);

        // if the region is provided, only retain networks for that region
        if (region is not null) tags = tags.Where(t => t.Region == region);

        return tags.SelectMany(t => t.AddressPrefixes);
    }
    static bool Contained(IEnumerable<IPNetwork> networks, IPAddress ipAddress) => networks.Any(n => n.Contains(ipAddress));
    static bool Contained(IEnumerable<IPNetwork> networks, IPNetwork network) => networks.Any(n => n.Contains(network));
}

[AzureIPNetworksGenerator.GenerateIpNetworksAttribute]
internal static partial class AzureCloudServiceTags { }
