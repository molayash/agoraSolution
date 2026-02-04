using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Employee_Service
{
    public interface IEmployeeService
    {
        Task<EmployeeViewModel> AddAsync(EmployeeViewModel model);
        Task<EmployeeViewModel?> UpdateAsync(EmployeeViewModel model);
        Task<bool> DeleteAsync(long id);
        Task<EmployeeViewModel?> GetByIdAsync(long id);
        Task<IEnumerable<EmployeeViewModel>> GetAllAsync();
    }

}
