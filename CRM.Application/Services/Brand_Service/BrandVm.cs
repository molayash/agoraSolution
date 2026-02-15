using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Brand_Service
{
    public class BrandVm
    {
        public long Id { get; set; }    
        [Required]
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
    }
}
