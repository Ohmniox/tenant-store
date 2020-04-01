using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using TenantStore.Service.Contract;
using TenantStore.Service.CosmosDB;

[assembly: FunctionsStartup(typeof(TenantStore.API.Startup))]
namespace TenantStore.API
{

    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddSingleton((s) => {
                string endpoint = Environment.GetEnvironmentVariable("CosmosDBEndPointUrl");
                if (string.IsNullOrEmpty(endpoint))
                {
                    throw new ArgumentNullException("Please specify a valid endpoint in the local.settings.json file or your Azure Functions Settings.");
                }

                string authKey = Environment.GetEnvironmentVariable("CosmosDBAuthorizationKey");
                if (string.IsNullOrEmpty(authKey))
                {
                    throw new ArgumentException("Please specify a valid AuthorizationKey in the local.settings.json file or your Azure Functions Settings.");
                }

                CosmosClientBuilder configurationBuilder = new CosmosClientBuilder(endpoint, authKey);
                return configurationBuilder.Build();
            });
            builder.Services.AddScoped<ITenantStoreService, TenantStoreService>();
            builder.Services.AddLogging((options) =>
            {
                options.AddConsole();
            });
        }
    }
}
