using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities
{
    public class Department : EntityBase
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = null!;   // Must be unique (IsDelete = 0)

        [MaxLength(500)]
        public string? Description { get; set; }
        public int SortOrder { get; set; }
    }
}
