﻿using AzureIPNetworks;
using System.Text.Json;
using System.CodeDom.Compiler;
using System.Text;
using System.Text.Json.Serialization;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<AzureIPsDownloader>();

var app = builder.Build();

var cancellationToken = CancellationToken.None;
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var downloader = app.Services.GetRequiredService<AzureIPsDownloader>();

var clouds = Enum.GetValues<AzureCloud>();
var targetDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../AzureIPNetworks/Resources"));
var files = new Dictionary<AzureCloud, string>();

// download file for each cloud
foreach (var cloud in clouds)
{
    var cloudName = cloud.ToString();
    var (url, stream) = await downloader.DownloadAsync(cloud, cancellationToken);
    var path = $"{Path.Combine(targetDirectory, cloudName)}.json";
    if (File.Exists(path)) File.Delete(path);
    using var fs = new FileStream(path, FileMode.OpenOrCreate);
    await stream.CopyToAsync(fs, cancellationToken);
    await fs.FlushAsync(cancellationToken);
    logger.LogInformation("Completed writing service tag file for cloud {CloudName}", cloud);
    files[cloud] = Path.GetFileName(url);
}

// write list of file for reference
if (files.Count == clouds.Length)
{
    var path = $"{Path.Combine(targetDirectory, "files")}.json";
    if (File.Exists(path)) File.Delete(path);
    using var fs = new FileStream(path, FileMode.OpenOrCreate);
    await JsonSerializer.SerializeAsync(fs, files, DownloaderJsonSerializerContext.Default.DictionaryAzureCloudString, cancellationToken);
    await fs.FlushAsync(cancellationToken);
    logger.LogInformation("Completed writing files.json");
}

{
    var provider = new AzureIPsProviderTemp(targetDirectory);
    var path = Path.Combine(targetDirectory, "../KnownData.cs");
    await GenerateKnownDataAsync(clouds, provider, path, cancellationToken);
    logger.LogInformation("Completed writing KnownData.cs");
}

logger.LogInformation("Finished!");

static async Task GenerateKnownDataAsync(IEnumerable<AzureCloud> clouds, AzureIPsProvider provider, string path, CancellationToken cancellationToken = default)
{
    // find unique regions and services
    var regions = new List<string>();
    var services = new List<string>();
    foreach (var cloud in clouds)
    {
        var tags = await provider.GetServiceTagsAsync(cloud, cancellationToken);
        regions.AddRange(tags.Select(t => t.Properties.Region));
        services.AddRange(tags.Select(t => t.Properties.SystemService));
    }
    regions = [.. regions.Distinct(StringComparer.OrdinalIgnoreCase).Order()];
    services = [.. services.Distinct(StringComparer.OrdinalIgnoreCase).Order()];
    const string Header = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the StaticDataGenerator source generator
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

#nullable enable

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
";

    var sb = new StringBuilder();
    var writer = new IndentedTextWriter(new StringWriter(sb));

    writer.WriteLine(Header);

    writer.WriteLine("using System.Collections;");
    writer.WriteLine("using System.Collections.Generic;");

    // write the namespace
    writer.WriteLine();
    writer.WriteLine("namespace AzureIPNetworks;");
    writer.WriteLine();

    // begin the class
    writer.WriteLine("public static partial class KnownData");
    writer.WriteLine("{");
    writer.Indent++;

    writer.WriteLine("// The regions");
    writer.WriteLine("public static readonly IReadOnlyList<string> Regions = [");
    writer.Indent++;
    foreach (var r in regions) writer.WriteLine($"\"{r}\",");
    writer.Indent--;
    writer.WriteLine("];");

    writer.WriteLine("");
    writer.WriteLine("// The services");
    writer.WriteLine("public static readonly IReadOnlyList<string> Services = [");
    writer.Indent++;
    foreach (var s in services) writer.WriteLine($"\"{s}\",");
    writer.Indent--;
    writer.WriteLine("];");

    // end the class
    writer.Indent--;
    writer.WriteLine("}");
    await writer.FlushAsync(cancellationToken);

    // output to file
    await File.WriteAllTextAsync(path, sb.ToString(), Encoding.UTF8, cancellationToken);
}

[JsonSerializable(typeof(Dictionary<AzureCloud, string>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
internal partial class DownloaderJsonSerializerContext : JsonSerializerContext { }
