using CRM.Application.Common.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Department_Service
{
    public interface IDepartmentService
    {
        Task<IEnumerable<DepartmentVM>> GetAllAsync();
        Task<PaginatedResult<DepartmentVM>> GetPagedAsync(PaginationRequest request);
        Task<DepartmentVM?> GetByIdAsync(long id);
        Task<long> CreateAsync(DepartmentVM model);
        Task<bool> UpdateAsync(DepartmentVM model);
        Task<bool> DeleteAsync(long id);
    }
}
