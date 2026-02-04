using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Auth_Service
{
    public interface IRolesService
    {
        Task<IEnumerable<RoleVM>> GetAllAsync();
        Task<RoleVM?> GetByIdAsync(string id);
        Task<bool> CreateAsync(RoleVM model);
        Task<bool> UpdateAsync(RoleVM model);
    }

}
