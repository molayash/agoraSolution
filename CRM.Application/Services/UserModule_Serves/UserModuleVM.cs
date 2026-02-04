using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.UserModule_Serves;

public class UserModuleVM
{
    public long Id { get; set; }

    [Required]
    [MaxLength(250)]
    public string ModuleName { get; set; }

    public int SortOrder { get; set; }

    public bool IsSubscribersModule { get; set; }

    public bool IsActive { get; set; }=true;
    public int? IsDelete { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}
