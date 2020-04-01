using System;

namespace Constants
{
    public static class CosmosKeys
    {
        public const string DatabaseId = "TenantStore";
        public const string ContainerId = "Inventory";
        public const string PartitionKeyPath = "/region";
    }
}
