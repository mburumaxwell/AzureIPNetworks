# AzureIPNetworks

[![NuGet](https://img.shields.io/nuget/v/AzureIPNetworks.svg)](https://www.nuget.org/packages/AzureIPNetworks/)
[![Nuget](https://img.shields.io/nuget/dt/AzureIPNetworks)](https://www.nuget.org/packages/AzureIPNetworks/)

## Introduction

This library eases working with known Azure IP networks in dotnet. `AzureCloudServiceTag` provides the IP ranges for the entire
cloud and is also broken out by region within that cloud. You cannot create your own service tag nor can you specify which IP addresses are included within a tag. Microsoft manages the address prefixes encompassed by the service tag, and automatically updates the service tag and their addresses as they change.

Service tags are stored in a JSON file which contains the IP address ranges for Public Azure as a whole, each Azure region within Public, and ranges for several Azure services such as AD, EventHub, KeyVault, Storage, SQL, and AzureTrafficManager in Public. The JSON file is downloaded from Microsoft's Website every so often from:

- <https://www.microsoft.com/en-us/download/details.aspx?id=56519>
- <https://www.microsoft.com/en-us/download/details.aspx?id=57062>
- <https://www.microsoft.com/en-us/download/details.aspx?id=57063>
- <https://www.microsoft.com/en-us/download/details.aspx?id=57064>

The library offers capabilities such as:

- Listing IP addresses or IP networks used by the whole of Azure or filtered to a single service tag.
- Checking if a given IP address or IP network belongs to Azure

The current version of the files can be found at [files.json](./src/AzureIPNetworks/Resources/files.json)

You can also download the files from the remote URL once per application run using `AzureIPsProvider.Remote.xxxx` methods

## Installation

Using the [.NET Core command-line interface (CLI) tools][dotnet-core-cli-tools]:

```sh
dotnet add package AzureIPNetworks
```

Using the [NuGet Command Line Interface (CLI)][nuget-cli]:

```sh
nuget install AzureIPNetworks
```

Using the [Package Manager Console][package-manager-console]:

```powershell
Install-Package AzureIPNetworks
```

From within Visual Studio:

1. Open the Solution Explorer.
2. Right-click on a project within your solution.
3. Click on *Manage NuGet Packages...*
4. Click on the *Browse* tab and search for "AzureIPNetworks".
5. Click on the `AzureIPNetworks` package, select the appropriate version in the right-tab and click *Install*.

## Example 1 - Get all IP Networks (local)

```csharp
var networks = await AzureIPsProvider.Local.GetNetworksAsync(AzureCloud.Public);
foreach (var net in networks)
{
    Console.WriteLine($"{net} ({range.FirstUsable} to {net.LastUsable})");
}
```

## Example 2 - Check if an IP is used by Azure (local)

```csharp
var ip = "30.0.0.20";
var used = await AzureIPsProvider.Local.IsAzureIpAsync(IPAddress.Parse(ip));
Console.WriteLine($"{ip} is {(used ? "" : "not")} used by any Azure service");
```

## Example 3 - Get all IP Networks (remote)

```csharp
var networks = await AzureIPsProvider.Remote.GetNetworksAsync(AzureCloud.Public);
foreach (var net in networks)
{
    Console.WriteLine($"{net} ({range.FirstUsable} to {net.LastUsable})");
}
```

## Example 42 - Check if an IP is used by Azure (remote)

```csharp
var ip = "30.0.0.20";
var used = await AzureIPsProvider.Remote.IsAzureIpAsync(IPAddress.Parse(ip));
Console.WriteLine($"{ip} is {(used ? "" : "not")} used by any Azure service");
```

### Issues &amp; Comments

Please leave all comments, bugs, requests, and issues on the Issues page. We'll respond to your request ASAP!

### License

The Library is licensed under the [MIT](http://www.opensource.org/licenses/mit-license.php "Read more about the MIT license form") license. Refer to the [LICENSE](./LICENSE) file for more information.
