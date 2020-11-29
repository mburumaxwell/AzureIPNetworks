namespace AzureIPNetworks
{
    /// <summary>
    /// Representation of IPs for a service in an Azure region
    /// </summary>
    public class AzureCloudServiceTag
    {
        /// <summary>
        /// The name of the service tag
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The identifier for the service
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Properties for this service tag
        /// </summary>
        public AzureCloudServiceTagProperties Properties { get; set; }
    }
}
