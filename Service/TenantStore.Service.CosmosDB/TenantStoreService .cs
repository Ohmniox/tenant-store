using Constants;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenantStore.Model;
using TenantStore.Model.RequestModel;
using TenantStore.Service.Contract;

namespace TenantStore.Service.CosmosDB
{
    public class TenantStoreService : ITenantStoreService
    {
        private readonly CosmosClient cosmosClient;
        private readonly ILogger<TenantStoreService> logger;

        public TenantStoreService(CosmosClient cosmosClient, ILogger<TenantStoreService> logger)
        {
            this.cosmosClient = cosmosClient;
            this.logger = logger;
        }

        public async Task CreateTenantAsync(Tenant tenant)
        {
            var container = GetContainer();
            ItemResponse<Tenant> tenantResponse = await container.CreateItemAsync<Tenant>(tenant, new PartitionKey(tenant.region));
        }

        public async Task UpdateTenantAsync(Tenant tenant)
        {
            var container = GetContainer();
            await container.UpsertItemAsync<Tenant>(tenant);
        }

        public async Task DeleteTenantAsync(Tenant tenant)
        {
            var container = GetContainer();
            await container.DeleteItemAsync<Tenant>(tenant.id.ToString(), new PartitionKey(tenant.region));
        }

        public async Task<Tenant> GetTenantAsync(Guid id)
        {
            var sqlQueryText = "SELECT c.id, c.name, c.region, c.metadata FROM c where c.id = @id";
            var parameters = new Dictionary<string, string>
            {
                { "@id", id.ToString() }
            };
            return (await FetchTenants(sqlQueryText, parameters)).FirstOrDefault();
        }

        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            var sqlQueryText = "SELECT c.id, c.name, c.region, c.metadata FROM c";
            return await FetchTenants(sqlQueryText);
        }

        private async Task<List<Tenant>> FetchTenants(string sqlQueryText, Dictionary<string, string> parameters = null)
        {
            logger.LogInformation($"Executing query: {sqlQueryText}");
            try
            {
                QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
                if (parameters != null)
                {
                    foreach (var parameter in parameters)
                    {
                        queryDefinition = queryDefinition.WithParameter(parameter.Key, parameter.Value);
                    }
                }

                var container =  GetContainer();
                FeedIterator<Tenant> queryResultSetIterator = container.GetItemQueryIterator<Tenant>(queryDefinition);
                List<Tenant> tenants = new List<Tenant>();

                while (queryResultSetIterator.HasMoreResults)
                {
                    FeedResponse<Tenant> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                    foreach (Tenant tenant in currentResultSet)
                    {
                        tenants.Add(tenant);
                    }
                }
                return tenants;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occured");
            }
            return null;
        }

        private Container GetContainer()
        {
            Database database = this.cosmosClient.GetDatabase(CosmosKeys.DatabaseId);
            return database.GetContainer(CosmosKeys.ContainerId);
        }
    }
}
