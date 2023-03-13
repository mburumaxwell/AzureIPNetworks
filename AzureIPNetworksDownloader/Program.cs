using System.Text.Json;
using System.Text.RegularExpressions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var client = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient("Downloader");

var fileIds = new Dictionary<string, string>
{
    ["Public"] = "56519",
    ["China"] = "57062",
    ["AzureGovernment"] = "57063",
    ["AzureGermany"] = "57064",
};

var fileUriParserRegex = new Regex(@"(https:\/\/download.microsoft.com\/download\/.*?\/ServiceTags_[A-z]+_[0-9]+\.json)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
var targetDirectory = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../AzureIPNetworksGenerator/Resources"));
var downloadedFiles = new Dictionary<string, string>();

foreach (var (cloudName, fileId) in fileIds)
{
    var downloadPageUrl = $"https://www.microsoft.com/en-us/download/confirmation.aspx?id={fileId}";
    logger.LogInformation("Fetch service tag file download url for cloud {CloudName} from {DownloadPageUrl}", cloudName, downloadPageUrl);
    var response = await client.GetStringAsync(downloadPageUrl);
    var matches = fileUriParserRegex.Match(response);
    if (matches.Success)
    {
        var downloadUrl = matches.Value;
        var stream = await client.GetStreamAsync(downloadUrl);
        var path = $"{Path.Combine(targetDirectory, cloudName)}.json";
        if (File.Exists(path)) File.Delete(path);
        var fs = new FileStream(path, FileMode.OpenOrCreate);
        await stream.CopyToAsync(fs);
        await fs.FlushAsync();
        logger.LogInformation("Completed writing service tag file for cloud {CloudName}", cloudName);
        downloadedFiles[cloudName] = Path.GetFileName(downloadUrl);
    }
    else
    {
        logger.LogError("Failed to parse service tag file download url for cloud {CloudName}", cloudName);
    }
}

if (downloadedFiles.Count == fileIds.Count)
{
    var path = $"{Path.Combine(targetDirectory, "files")}.json";
    if (File.Exists(path)) File.Delete(path);
    var fs = new FileStream(path, FileMode.OpenOrCreate);
    await JsonSerializer.SerializeAsync(fs, downloadedFiles, new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true, // eaasier to read
    });
    await fs.FlushAsync();
}
else
{
    logger.LogError("Expected {Expected} files but found {Actual}.\r\n{FileNames}", fileIds.Count, downloadedFiles.Count, downloadedFiles.Keys);
}

logger.LogInformation("Finished!");