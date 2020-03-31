﻿using System;

namespace TenantStore.Model
{
    public class Tenant
    {
        public Guid id { get; set; }
        
        public string name { get; set; }

        public string region { get; set; }

        public string metadata { get; set; }
    }
}
