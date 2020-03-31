﻿using Microsoft.Azure.Cosmos;
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

        private readonly string databaseId = "TenantStore";
        private readonly CosmosClient cosmosClient;
        private readonly ILogger<TenantStoreService> logger;
        private readonly string containerId = "Inventory";
        private Database database;
        private Container container;

        public TenantStoreService(CosmosClient cosmosClient, ILogger<TenantStoreService> logger)
        {
            this.cosmosClient = cosmosClient;
            this.logger = logger;
        }

        public async Task CreateTenantAsync(Tenant tenant)
        {
            await CreateDatabaseAndContainerAsync();
            ItemResponse<Tenant> tenantResponse = await this.container.CreateItemAsync<Tenant>(tenant, new PartitionKey(tenant.region));
        }

        public async Task UpdateTenantAsync(Tenant tenant)
        {
            await CreateDatabaseAndContainerAsync();
            await container.UpsertItemAsync<Tenant>(tenant);
        }

        public async Task DeleteTenantAsync(Tenant tenant)
        {
            await CreateDatabaseAndContainerAsync();
            await container.DeleteItemAsync<Tenant>(tenant.id.ToString(), new PartitionKey(tenant.region));
        }

        public async Task<Tenant> GetTenantAsync(Guid id)
        {
            await CreateDatabaseAndContainerAsync();
            var sqlQueryText = $"SELECT * FROM c where c.id = @id";
            var parameters = new Dictionary<string, string>();
            parameters.Add("@id", id.ToString());
            return (await FetchTenants(sqlQueryText, parameters)).FirstOrDefault();
        }

        public async Task<List<Tenant>> GetAllTenantsAsync()
        {
            await CreateDatabaseAndContainerAsync();
            var sqlQueryText = "SELECT * FROM c";
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

                FeedIterator<Tenant> queryResultSetIterator = this.container.GetItemQueryIterator<Tenant>(queryDefinition);
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

        private async Task CreateDatabaseAndContainerAsync()
        {
            // Create a new database
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
            await CreateContainerAsync();
        }

        private async Task CreateContainerAsync()
        {
            // Create a new container
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/region", 400);
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }
    }
}