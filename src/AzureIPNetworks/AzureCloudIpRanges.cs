using System.Collections.Generic;

namespace AzureIPNetworks
{
    /// <summary>
    /// Representation of the Azure supplied document containing all the IPs.
    /// The official document can be downloaded from https://www.microsoft.com/en-us/download/details.aspx?id=56519
    /// </summary>
    public class AzureCloudIpRanges
    {
        /// <summary>
        /// The name of the cloud the data belongs to
        /// </summary>
        public string? Cloud { get; set; }

        /// <summary>
        /// The values for this cloud by service tag
        /// </summary>
        public ICollection<AzureCloudServiceTag>? Values { get; set; }
    }
}
