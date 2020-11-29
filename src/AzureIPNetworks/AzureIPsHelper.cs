using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AzureIPNetworks
{
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
            var resourceName = string.Join(".", typeof(AzureCloudIpRanges).Namespace, "Resources", "ServiceTags_Public_20201012.json");
            using (var stream = typeof(AzureCloudIpRanges).Assembly.GetManifestResourceStream(resourceName))
            {
                // deserialize the JSON file
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return await JsonSerializer.DeserializeAsync<AzureCloudIpRanges>(stream, options, cancellationToken);
            }
        }

        /// <summary>
        /// Get the Azure IPs in networks. The official document processed by <see cref="GetAzureCloudIpsAsync(CancellationToken)"/> is parsed into <see cref="IPNetwork"/>.
        /// The official document can be downloaded from https://www.microsoft.com/en-us/download/details.aspx?id=41653
        /// </summary>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns></returns>
        public static async Task<IEnumerable<IPNetwork>> GetAzureIpNetworksAsync(CancellationToken cancellationToken = default)
        {
            var ipRanges = await GetAzureCloudIpsAsync(cancellationToken);
            var prefixes = ipRanges?.Values.SelectMany(v => v.Properties?.AddressPrefixes);
            return prefixes.Select(r => IPNetwork.Parse(r));
        }

        /// <summary>
        /// Checks if the supplied IP address is an Azure IP
        /// </summary>
        /// <param name="ipAddress">the IP address to check against</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns></returns>
        public static async Task<bool> IsAzureIpAsync(IPAddress ipAddress, CancellationToken cancellationToken = default)
            => (await GetAzureIpNetworksAsync(cancellationToken)).Contains(ipAddress);

        /// <summary>
        /// Checks if the supplied IP network is an Azure IP
        /// </summary>
        /// <param name="network">the network check against</param>
        /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
        /// <returns></returns>
        public static async Task<bool> IsAzureIpAsync(IPNetwork network, CancellationToken cancellationToken = default)
            => (await GetAzureIpNetworksAsync(cancellationToken)).Contains(network);
    }
}
