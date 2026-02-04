using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities.Auth
{
    public class ModuleMenu:EntityBase
    {

        [Required]
        public long UserModuleId { get; set; }
        public UserModule? UserModule { get; set; }

        [Required]
        [MaxLength(250)]
        public string MenuName { get; set; }

        [MaxLength(250)]
        public string? MenuIcon { get; set; }

        public bool HasChild { get; set; } = false;

        public long? ParentId { get; set; }

        [MaxLength(500)]
        public string? Url { get; set; }

        public bool IsVisible { get; set; } = true;

        public bool IsEnabled { get; set; } = true;

        public int SortOrder { get; set; } = 0;
    }
}
