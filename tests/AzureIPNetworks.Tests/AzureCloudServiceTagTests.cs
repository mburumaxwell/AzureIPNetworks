using System.Net;
using Xunit;

namespace AzureIPNetworks.Tests;

public class AzureCloudServiceTagTests
{
    [Fact]
    public void GetPublicCloudNetworks_Works()
    {
        var networks = AzureCloudServiceTag.PublicCloud;
        Assert.NotNull(networks);
        Assert.True(networks.Any());

        Assert.Contains(IPNetwork.Parse("40.90.149.32/27"), networks);
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
    public void IsPublicCloudIP_Works(string ip, bool expected)
    {
        var actual = AzureCloudServiceTag.IsPublicCloudIP(IPAddress.Parse(ip));
        Assert.Equal(expected, actual);
    }

}
