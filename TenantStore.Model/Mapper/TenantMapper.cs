using System;
using System.Collections.Generic;
using System.Text;
using TenantStore.Model.RequestModel;

namespace TenantStore.Model.Mapper
{
    public static class TenantMapper
    {
        public static Tenant Map(TenantRequestModel tenantRequestModel)
        {
            return new Tenant
            {
                id = Guid.NewGuid(),
                name = tenantRequestModel.Name,
                region = tenantRequestModel.Region,
                metadata = tenantRequestModel.Metadata
            };
        }

        public static void Map(TenantRequestModel tenantRequestModel, Tenant existingTenant)
        {
            existingTenant.name = tenantRequestModel.Name;
            existingTenant.region = tenantRequestModel.Region;
            existingTenant.metadata = tenantRequestModel.Metadata;
        }
    }
}
