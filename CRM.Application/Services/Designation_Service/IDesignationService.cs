using CRM.Application.Common.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Designation_Service
{
    public interface IDesignationService
    {
        Task<IEnumerable<DesignationVM>> GetAllAsync();
        Task<PaginatedResult<DesignationVM>> GetPagedAsync(PaginationRequest request);
        Task<DesignationVM?> GetByIdAsync(long id);
        Task<long> CreateAsync(DesignationVM model);
        Task<bool> UpdateAsync(DesignationVM model);
        Task<bool> DeleteAsync(long id);
    }
}
