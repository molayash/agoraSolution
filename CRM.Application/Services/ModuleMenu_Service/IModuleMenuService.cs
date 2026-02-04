using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.ModuleMenu_Service
{
    public interface IModuleMenuService
    {
        Task<IEnumerable<ModuleMenuVM>> GetAllAsync();
        Task<ModuleMenuVM?> GetByIdAsync(long id);
        Task<long> CreateAsync(ModuleMenuVM model);
        Task<bool> UpdateAsync(ModuleMenuVM model);
        Task<bool> DeleteAsync(long id);
    }
}
