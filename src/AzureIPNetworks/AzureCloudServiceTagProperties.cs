namespace AzureIPNetworks
{
    /// <summary>
    /// The properties of a service tag
    /// </summary>
    public class AzureCloudServiceTagProperties
    {
        /// <summary>
        /// The name of the Azure region
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? Platform { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string? SystemService { get; set; }

        /// <summary>
        /// The network subnet specifying the IPs in range
        /// </summary>
        public string[]? AddressPrefixes { get; set; }
    }
}
