using System.Net;
using System.Text.Json;

namespace AzureIPNetworks;

/// <summary>
/// Provider for Azure IPs to help in allowing/blocking or identifying them.
/// This is useful can be useful in firewalls and applications where the incoming
/// IP needs to be checked before blocking/allowing its traffic.
/// </summary>
public abstract class AzureIPsProvider
{
    /// <summary>
    /// Provider that uses local data (cached in this library).
    /// Use this if you are okay with having stale data from time to time but prefer speed.
    /// </summary>
    public static AzureIPsProvider Local { get; } = new AzureIPsProviderLocal();

    /// <summary>
    /// Provider that uses remote data (downloading) once per application instance.
    /// Use this if you need your application to have fresh data every time it starts and:
    /// <br /> 1. it does not restart too many times (can result in rate limiting)
    /// <br /> 2. it is always connected to the internet
    /// <br /> 3. your are comfortable with a potentially high startup time (downloading)
    /// </summary>
    public static AzureIPsProvider Remote { get; } = new AzureIPsProviderRemote();

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
    public async ValueTask<bool> IsAzureIpAsync(IPAddress address, AzureCloud cloud = AzureCloud.Public, string? service = null, string? region = null, CancellationToken cancellationToken = default)
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
    public async ValueTask<bool> IsAzureIpAsync(IPNetwork network, AzureCloud cloud = AzureCloud.Public, string? service = null, string? region = null, CancellationToken cancellationToken = default)
        => Contained(await GetNetworksAsync(cloud, service, region, cancellationToken), network);
#endif

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
    public async ValueTask<IEnumerable<IPNetwork>> GetNetworksAsync(AzureCloud cloud = AzureCloud.Public, string? service = null, string? region = null, CancellationToken cancellationToken = default)
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

        return tags.SelectMany(t => t.Properties?.AddressPrefixes ?? Array.Empty<IPNetwork>());
    }

    // caching is better than source generator since only the clouds that are used will be loaded
    // caching also reduces double allocations when parsing repeatedly
    private readonly Dictionary<AzureCloud, CloudServiceTags> data = new(Enum.GetValues(typeof(AzureCloud)).Length);

    /// <summary>Get the Azure IP networks.</summary>
    /// <param name="cloud">The Azure Cloud to check in.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns></returns>
    public async ValueTask<IEnumerable<ServiceTag>> GetServiceTagsAsync(AzureCloud cloud, CancellationToken cancellationToken = default)
    {
        if (!data.TryGetValue(cloud, out var ranges))
        {
            // get the stream
            var stream = await GetStreamAsync(cloud, cancellationToken);

            // deserialize the JSON file
            data[cloud] = ranges = (await JsonSerializer.DeserializeAsync(stream, AzureIPNetworksJsonSerializerContext.Default.CloudServiceTags, cancellationToken))!;
        }

        return ranges.Values;
    }

    /// <summary>Gets the stream containing the service tags content.</summary>
    /// <param name="cloud">The Azure Cloud to get a stream for.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    protected abstract ValueTask<Stream> GetStreamAsync(AzureCloud cloud, CancellationToken cancellationToken = default);

    private static bool Contained(IEnumerable<IPNetwork> networks, IPAddress ipAddress) => networks.Any(n => n.Contains(ipAddress));
#if !NET8_0_OR_GREATER
    private static bool Contained(IEnumerable<IPNetwork> networks, IPNetwork network) => networks.Any(n => n.Contains(network));
#endif
}

/// <summary>
/// Implementation of <see cref="AzureIPsProvider"/> for locally cached data.
/// </summary>
internal class AzureIPsProviderLocal : AzureIPsProvider
{
    /// <inheritdoc/>
    protected override ValueTask<Stream> GetStreamAsync(AzureCloud cloud, CancellationToken cancellationToken = default)
    {
        // read the JSON file from embedded resource
        var name = string.Join(".", typeof(AzureIPsProviderLocal).Namespace, "Resources", $"{cloud}.json");
        var stream = typeof(AzureIPsProviderLocal).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Stream '{name}' could not be found. Raise an issue on GitHub.");

        // deserialize the JSON file
        return new ValueTask<Stream>(stream);
    }
}

/// <summary>
/// Implementation of <see cref="AzureIPsProvider"/> for downloading remote data once per run.
/// </summary>
/// <remarks>Creates an <see cref="AzureIPsProviderRemote"/> instance.</remarks>
/// <param name="downloader">The <see cref="AzureIPsDownloader"/> to use for downloading.</param>
internal class AzureIPsProviderRemote(AzureIPsDownloader downloader) : AzureIPsProvider
{
    /// <summary>Creates an <see cref="AzureIPsProviderRemote"/> instance.</summary>
    /// <param name="client">The <see cref="HttpClient"/> instance to use when creating an <see cref="AzureIPsDownloader"/>.</param>
    public AzureIPsProviderRemote(HttpClient client) : this(new AzureIPsDownloader(client)) { }

    /// <summary>Creates an <see cref="AzureIPsProviderRemote"/> instance.</summary>
    public AzureIPsProviderRemote() : this(new HttpClient()) { }

    /// <inheritdoc/>
    protected override async ValueTask<Stream> GetStreamAsync(AzureCloud cloud, CancellationToken cancellationToken = default)
        => (await downloader.DownloadAsync(cloud, cancellationToken)).stream;
}
