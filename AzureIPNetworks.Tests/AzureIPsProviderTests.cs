using System.Net;
using Xunit;

namespace AzureIPNetworks.Tests;

public class AzureIPsProviderTests
{
    [Theory]
    [InlineData(AzureCloud.Public, "40.90.149.32/27")]
    [InlineData(AzureCloud.China, "40.72.175.176/30")]
    [InlineData(AzureCloud.AzureGovernment, "13.72.0.0/18")]
    [InlineData(AzureCloud.AzureGermany, "51.4.32.0/19")]
    public async Task PublicCloud_Works(AzureCloud cloud, string network)
    {
        var networks = await AzureIPsProvider.Local.GetNetworksAsync(cloud, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(networks);
        Assert.True(networks.Any());

#if NET8_0_OR_GREATER
        Assert.Contains(IPNetwork.Parse(network), networks);
#else
        Assert.Contains(IPNetwork2.Parse(network), networks);
#endif
    }

    [Theory]
    [InlineData("52.233.184.181", true)] // Azure APP Service
    [InlineData("52.233.187.181", true)] // Azure APP Service
    [InlineData("52.166.122.9", true)] // Azure APP Service
    [InlineData("52.166.120.253", true)] // Azure APP Service
    [InlineData("52.166.125.196", true)] // Azure APP Service
    [InlineData("13.74.41.233", true)] // Azure APP Service

    [InlineData("207.154.225.144", false)]
    [InlineData("196.207.157.237", false)]
    [InlineData("196.207.161.233", false)]
    public async Task IsAzureIP_Works(string ip, bool expected)
    {
        var actual = await AzureIPsProvider.Local.IsAzureIpAsync(IPAddress.Parse(ip), cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task Remote_WorksAsExpected()
    {
        var networks = await AzureIPsProvider.Remote.GetNetworksAsync(AzureCloud.Public, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(networks);
        Assert.True(networks.Any());

#if NET8_0_OR_GREATER
        Assert.Contains(IPNetwork.Parse("40.90.149.32/27"), networks);
#else
        Assert.Contains(IPNetwork2.Parse("40.90.149.32/27"), networks);
#endif

        Assert.True(await AzureIPsProvider.Remote.IsAzureIpAsync(IPAddress.Parse("52.233.184.181"), AzureCloud.Public, cancellationToken: TestContext.Current.CancellationToken));
    }
}
