using System.Text.Json.Serialization;

namespace AzureIPNetworks;

///
[JsonConverter(typeof(JsonStringEnumConverter<AzureCloud>))]
public enum AzureCloud
{
    ///
    Public,

    ///
    China,

    ///
    AzureGovernment,

    ///
    AzureGermany,
}
