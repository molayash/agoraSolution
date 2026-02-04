using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.UserModule_Serves;

public interface IUserModuleService
{
    Task<IEnumerable<UserModuleVM>> GetAllAsync();
    Task<UserModuleVM?> GetByIdAsync(long id);
    Task<long> CreateAsync(UserModuleVM model);
    Task<bool> UpdateAsync(UserModuleVM model);
    Task<bool> DeleteAsync(long id);
}
