using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TenantStore.Model;
using TenantStore.Model.RequestModel;

namespace TenantStore.Service.Contract
{
    public interface ITenantStoreService
    {
        Task CreateTenantAsync(Tenant tenant);

        Task UpdateTenantAsync(Tenant tenant);

        Task<List<Tenant>> GetAllTenantsAsync();

        Task<Tenant> GetTenantAsync(Guid id);

        Task DeleteTenantAsync(Tenant tenant);
    }
}
