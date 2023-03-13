using System.Net;
using Xunit;

namespace AzureIPNetworks.Tests;

public class AzureCloudServiceTagTests
{
    [Fact]
    public void PublicCloud_Works()
    {
        var networks = AzureCloudServiceTag.PublicCloud;
        Assert.NotNull(networks);
        Assert.True(networks.Any());

        Assert.Contains(IPNetwork.Parse("40.90.149.32/27"), networks);
    }

    [Fact]
    public void ChinaCloud_Works()
    {
        var networks = AzureCloudServiceTag.ChinaCloud;
        Assert.NotNull(networks);
        Assert.True(networks.Any());

        Assert.Contains(IPNetwork.Parse("40.72.0.0/18"), networks);
    }

    [Fact]
    public void AzureGovernmentCloud_Works()
    {
        var networks = AzureCloudServiceTag.AzureGovernmentCloud;
        Assert.NotNull(networks);
        Assert.True(networks.Any());

        Assert.Contains(IPNetwork.Parse("13.72.0.0/18"), networks);
    }

    [Fact]
    public void AzureGermanyCloud_Works()
    {
        var networks = AzureCloudServiceTag.AzureGermanyCloud;
        Assert.NotNull(networks);
        Assert.True(networks.Any());

        Assert.Contains(IPNetwork.Parse("51.4.32.0/19"), networks);
    }

    [Fact]
    public void AllClouds_Works()
    {
        var networks = AzureCloudServiceTag.AllClouds;
        Assert.NotNull(networks);
        Assert.True(networks.Any());

        Assert.Contains(IPNetwork.Parse("40.90.149.32/27"), networks);
        Assert.Contains(IPNetwork.Parse("40.72.0.0/18"), networks);
        Assert.Contains(IPNetwork.Parse("13.72.0.0/18"), networks);
        Assert.Contains(IPNetwork.Parse("51.4.32.0/19"), networks);
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
    public void IsAzureIP_Works(string ip, bool expected)
    {
        var actual = AzureCloudServiceTag.IsAzureIP(IPAddress.Parse(ip));
        Assert.Equal(expected, actual);
    }

}
