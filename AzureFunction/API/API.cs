using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TenantStore.Service.Contract;
using TenantStore.Model;
using TenantStore.Model.RequestModel;
using Extensions;
using System.Linq;
using TenantStore.Model.Mapper;

namespace TenantStore.AzureFunction.API
{
    public class API
    {
        private readonly ITenantStoreService tenantStoreService;

        public API(ITenantStoreService tenantStoreService)
        {
            this.tenantStoreService = tenantStoreService;
        }

        [FunctionName("CreateTenant")]
        public async Task<IActionResult> CreateTenant(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "tenant")] HttpRequest req,
            ILogger log)
        {
            var body = await req.GetBodyAsync<TenantRequestModel>();
            if (!body.IsValid)
            {
                return new BadRequestObjectResult(body.ValidationMessage);
            }

            var tenant = TenantMapper.Map(body.Value);
            await tenantStoreService.CreateTenantAsync(tenant);
            return new CreatedResult(new Uri($"api/tenant/{tenant.id}", UriKind.Relative), tenant.id);
        }

        [FunctionName("GetAllTenant")]
        public async Task<IActionResult> GetAllTenant(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "tenant")] HttpRequest req,
            ILogger log)
        {
            var tenants = await tenantStoreService.GetAllTenantsAsync();
            return new OkObjectResult(tenants);
        }

        [FunctionName("GetTenant")]
        public async Task<IActionResult> GetTenant(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "tenant/{id}")] HttpRequest req,
            [FromRoute(Name = "id")]string id,
            ILogger log)
        {
            var parseResult = Guid.TryParse(id, out Guid tenantId);
            if (!parseResult)
            {
                return new BadRequestObjectResult("Invalid tenantId");
            }

            var tenant = await tenantStoreService.GetTenantAsync(tenantId);
            if (tenant == null)
            {
                return new NotFoundObjectResult("Tenant not found");
            }
            return new OkObjectResult(tenant);
        }

        [FunctionName("UpdateTenant")]
        public async Task<IActionResult> UpdateTenant(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = "tenant/{id}")] HttpRequest req,
           [FromRoute(Name = "id")]string id,
           ILogger log)
        {
            var parseResult = Guid.TryParse(id, out Guid tenantId);
            if (!parseResult)
            {
                return new BadRequestObjectResult("Invalid tenantId");
            }

            var body = await req.GetBodyAsync<TenantRequestModel>();
            if (!body.IsValid)
            {
                return new BadRequestObjectResult(body.ValidationMessage);
            }
            var tenantRequestModel = body.Value;

            var existingTenant = await tenantStoreService.GetTenantAsync(tenantId);
            if (existingTenant == null)
            {
                return new NotFoundObjectResult("Tenant not found");
            }

            TenantMapper.Map(tenantRequestModel, existingTenant);
            await tenantStoreService.UpdateTenantAsync(existingTenant);
            return new NoContentResult();
        }

        [FunctionName("DeleteTenant")]
        public async Task<IActionResult> DeleteTenant(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "tenant/{id}")] HttpRequest req,
            [FromRoute(Name = "id")]string id,
            ILogger log)
        {
            var parseResult = Guid.TryParse(id, out Guid tenantId);
            if (!parseResult)
            {
                return new BadRequestObjectResult("Invalid tenantId");
            }

            var tenant = await tenantStoreService.GetTenantAsync(tenantId);
            if (tenant == null)
            {
                return new NotFoundObjectResult("Tenant not found");
            }

            await tenantStoreService.DeleteTenantAsync(tenant);
            return new NoContentResult();
        }
    }
}
