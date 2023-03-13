﻿using System.Net;
using Xunit;

namespace AzureIPNetworks.Tests;

public class AzureIPsHelperTests
{
    [Theory]
    [InlineData(AzureCloud.Public, "40.90.149.32/27")]
    [InlineData(AzureCloud.China, "40.72.0.0/18")]
    [InlineData(AzureCloud.AzureGovernment, "13.72.0.0/18")]
    [InlineData(AzureCloud.AzureGermany, "51.4.32.0/19")]
    public async Task PublicCloud_Works(AzureCloud cloud, string network)
    {
        var networks = await AzureIPsHelper.GetNetworksAsync(cloud);
        Assert.NotNull(networks);
        Assert.True(networks.Any());

        Assert.Contains(IPNetwork.Parse(network), networks);
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
        var actual = await AzureIPsHelper.IsAzureIpAsync(IPAddress.Parse(ip));
        Assert.Equal(expected, actual);
    }
}
