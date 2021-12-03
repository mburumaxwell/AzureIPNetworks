using System.Net;

namespace System.Linq;

/// <summary>
/// Extensions for <see cref="IPNetwork"/>
/// </summary>
public static class IPNetworkExtensions
{
    /// <summary>
    /// Checks if an <see cref="IPAddress"/> is contained in any of the supplied networks
    /// </summary>
    /// <param name="networks">the networks to check</param>
    /// <param name="ipAddress">the IP address to be checked</param>
    /// <returns></returns>
    public static bool Contains(this IEnumerable<IPNetwork> networks, IPAddress ipAddress) => networks.Any(n => n.Contains(ipAddress));

    /// <summary>
    /// Checks if an <see cref="IPNetwork"/> is contained in any of the supplied networks
    /// </summary>
    /// <param name="networks">the networks to check</param>
    /// <param name="network">the network to be checked</param>
    /// <returns></returns>
    public static bool Contains(this IEnumerable<IPNetwork> networks, IPNetwork network) => networks.Any(n => n.Contains(network));
}
