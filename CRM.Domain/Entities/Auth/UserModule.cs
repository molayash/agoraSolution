using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Domain.Entities.Auth
{
    public class UserModule:EntityBase
    {

        [Required]
        [MaxLength(250)]
        public string ModuleName { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsSubscribersModule { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }
}
