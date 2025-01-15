using System.Text.RegularExpressions;

namespace AzureIPNetworks;

/// <summary>
/// Utility to download the latest service tag files for Azure.
/// </summary>
/// <remarks>Creates an <see cref="AzureIPsDownloader"/> instance.</remarks>
/// <param name="client">The <see cref="HttpClient"/> instance to use when downloading.</param>
/// <exception cref="ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
public partial class AzureIPsDownloader(HttpClient client)
{
    private static readonly Dictionary<AzureCloud, string> fileIds = new()
    {
        [AzureCloud.Public] = "56519",
        [AzureCloud.China] = "57062",
        [AzureCloud.AzureGovernment] = "57063",
        [AzureCloud.AzureGermany] = "57064",
    };

    private const string FileUriParserRegexFormat = @"(https:\/\/download.microsoft.com\/download\/.*?\/ServiceTags_[A-z]+_[0-9]+\.json)";
#if NET7_0_OR_GREATER

    [GeneratedRegex(FileUriParserRegexFormat, RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex GetFileUriParserRegex();
    private static readonly Regex fileUriParserRegex = GetFileUriParserRegex();
#else
    private static readonly Regex fileUriParserRegex = new(FileUriParserRegexFormat, RegexOptions.IgnoreCase | RegexOptions.Compiled);
#endif

    private readonly HttpClient client = client ?? throw new ArgumentNullException(nameof(client));

    /// <summary>Download the latest service tags for a given cloud.</summary>
    /// <param name="cloud">The Azure Cloud to download for.</param>
    /// <param name="cancellationToken">A token that may be used to cancel the operation.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">
    /// When unable to parse the download page to extract a download URL.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The provided <paramref name="cloud"/> does not have a URL mapped.
    /// </exception>
    public async Task<(string url, Stream stream)> DownloadAsync(AzureCloud cloud, CancellationToken cancellationToken = default)
    {
        if (!fileIds.TryGetValue(cloud, out var fileId))
        {
            throw new NotSupportedException($"'{nameof(AzureCloud)}.{cloud}' does not have a file identifier mapped.");
        }

        var pageUrl = $"https://www.microsoft.com/en-us/download/confirmation.aspx?id={fileId}";
#if NET5_0_OR_GREATER
        var response = await client.GetStringAsync(pageUrl, cancellationToken).ConfigureAwait(false);
#else
        var response = await client.GetStringAsync(pageUrl).ConfigureAwait(false);
#endif
        var matches = fileUriParserRegex.Match(response);
        if (matches.Success)
        {
            var url = matches.Value;
#if NET5_0_OR_GREATER
            var stream = await client.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);
#else
            var stream = await client.GetStreamAsync(url).ConfigureAwait(false);
#endif
            return (url, stream);
        }

        throw new InvalidOperationException($"Failed to parse service tag file download url for '{nameof(AzureCloud)}.{cloud}'");
    }
}
