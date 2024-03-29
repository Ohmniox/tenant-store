﻿using Constants;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TenantStore.Model.Entity;
using TenantStore.Service.Contract;

namespace TenantStore.Service.CosmosDB
{
    public class TenantStoreService : ITenantStoreService
    {
        private readonly CosmosClient cosmosClient;
        private readonly Container container;
        private readonly ILogger<TenantStoreService> logger;

        public TenantStoreService(CosmosClient cosmosClient, ILogger<TenantStoreService> logger)
        {
            this.cosmosClient = cosmosClient;
            this.container = GetContainer();
            this.logger = logger;
        }

        public Task CreateTenantAsync(Tenant tenant)
        {
            logger.LogInformation($"Creating tenant with id: {tenant.id} and name :{tenant.name}");
            return container.CreateItemAsync<Tenant>(tenant, new PartitionKey(tenant.region));
        }

        public Task UpdateTenantAsync(Tenant tenant)
        {
            logger.LogInformation($"Updating tenant with id: {tenant.id}");
            return container.UpsertItemAsync<Tenant>(tenant);
        }

        public Task DeleteTenantAsync(Tenant tenant)
        {
            logger.LogInformation($"Deleting tenant with id: {tenant.id}");
            return container.DeleteItemAsync<Tenant>(tenant.id.ToString(), new PartitionKey(tenant.region));
        }

        public async Task<Tenant> GetTenantAsync(Guid id)
        {
            logger.LogInformation($"Fetching tenant with id: {id}");
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
                logger.LogInformation($"Fetched tenants : {tenants.Count}");
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
            try
            {
                Database database = this.cosmosClient.GetDatabase(CosmosKeys.DatabaseId);
                return database.GetContainer(CosmosKeys.ContainerId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception occured while fetching database/container");
                throw;
            }
        }
    }
}
