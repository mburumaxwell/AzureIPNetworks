using System.Net;
using System.Text.Json;

namespace AzureIPNetworks;

/// <summary>
/// Helper methods for working with Azure IPs
/// </summary>
public static class AzureIPsHelper
{
    /// <summary>
    /// Get the raw object for the Azure IPs.
    /// The official document can be downloaded from https://www.microsoft.com/en-us/download/details.aspx?id=56519
    /// </summary>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns></returns>
    public static async Task<AzureCloudIpRanges> GetAzureCloudIpsAsync(CancellationToken cancellationToken = default)
    {
        // read the JSON file from embedded resource
        var name = string.Join(".", typeof(AzureCloudIpRanges).Namespace, "Resources", "ServiceTags_Public_20210906.json");
        using var stream = typeof(AzureCloudIpRanges).Assembly.GetManifestResourceStream(name);

        // deserialize the JSON file
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return (await JsonSerializer.DeserializeAsync<AzureCloudIpRanges>(stream, options, cancellationToken))!;
    }

    /// <summary>
    /// Get the Azure IPs in networks. The official document processed by <see cref="GetAzureCloudIpsAsync(CancellationToken)"/> is parsed into <see cref="IPNetwork"/>.
    /// The official document can be downloaded from https://www.microsoft.com/en-us/download/details.aspx?id=41653
    /// </summary>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns></returns>
    public static async Task<IEnumerable<IPNetwork>> GetAzureIpNetworksAsync(CancellationToken cancellationToken = default)
    {
        var ranges = await GetAzureCloudIpsAsync(cancellationToken);
        var prefixes = ranges?.Values?.SelectMany(v => v.Properties?.AddressPrefixes)
                                      .Where(p => !string.IsNullOrWhiteSpace(p));
        return prefixes?.Select(r => IPNetwork.Parse(r)) ?? System.Array.Empty<IPNetwork>();
    }

    /// <summary>
    /// Checks if the supplied IP address is an Azure IP
    /// </summary>
    /// <param name="address">The IP address to check against</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns></returns>
    public static async Task<bool> IsAzureIpAsync(IPAddress address, CancellationToken cancellationToken = default)
        => (await GetAzureIpNetworksAsync(cancellationToken)).Contains(address);

    /// <summary>
    /// Checks if the supplied IP network is an Azure IP
    /// </summary>
    /// <param name="network">The network check against</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns></returns>
    public static async Task<bool> IsAzureIpAsync(IPNetwork network, CancellationToken cancellationToken = default)
        => (await GetAzureIpNetworksAsync(cancellationToken)).Contains(network);
}
