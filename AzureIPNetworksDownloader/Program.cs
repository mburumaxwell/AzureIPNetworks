using AzureIPNetworks;
using System.Text.Json;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddHttpClient<AzureIPsDownloader>();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var downloader = app.Services.GetRequiredService<AzureIPsDownloader>();

var clouds = Enum.GetValues<AzureCloud>();
var targetDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../AzureIPNetworks/Resources"));
var files = new Dictionary<AzureCloud, string>();

foreach (var cloud in clouds)
{
    var cloudName = cloud.ToString();
    var (url, stream) = await downloader.DownloadAsync(cloud);
    var path = $"{Path.Combine(targetDirectory, cloudName)}.json";
    if (File.Exists(path)) File.Delete(path);
    var fs = new FileStream(path, FileMode.OpenOrCreate);
    await stream.CopyToAsync(fs);
    await fs.FlushAsync();
    logger.LogInformation("Completed writing service tag file for cloud {CloudName}", cloud);
    files[cloud] = Path.GetFileName(url);
}

if (files.Count == clouds.Length)
{
    var path = $"{Path.Combine(targetDirectory, "files")}.json";
    if (File.Exists(path)) File.Delete(path);
    var fs = new FileStream(path, FileMode.OpenOrCreate);
    var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true, // easier to read
        Converters =
        {
            new System.Text.Json.Serialization.JsonStringEnumConverter(),
        },
    };
    await JsonSerializer.SerializeAsync(fs, files, options);
    await fs.FlushAsync();
}

logger.LogInformation("Finished!");