using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ModuleMenu_Service
{
    public class ModuleMenuVM
    {
        public long Id { get; set; }

        public long UserModuleId { get; set; }
        public string? UserModuleName { get; set; }

        public string MenuName { get; set; }

        public string? MenuIcon { get; set; }

        public bool HasChild { get; set; }

        public long? ParentId { get; set; }

        public string? Url { get; set; }

        public bool IsVisible { get; set; } = true;

        public bool IsEnabled { get; set; } = true;

        public int SortOrder { get; set; } = 0;
        public int ModuleSortOrder { get; set; } = 0;

       public int? IsDelete { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public string? UpdatedBy { get; set; }
    }
}
