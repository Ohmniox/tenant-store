using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace TenantStore.Model.RequestModel
{
    public class TenantRequestModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        public string Region { get; set; }

        public string Metadata { get; set; }
    }
}
