using CRM.Application.Common.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Services.Organization_Service
{
    public interface IOrganizationService
    {
        Task<IEnumerable<OrganizationVM>> GetAllAsync();
        Task<PaginatedResult<OrganizationVM>> GetPagedAsync(PaginationRequest request);
        Task<OrganizationVM?> GetByIdAsync(long id);
        Task<long> CreateAsync(OrganizationVM model);
        Task<bool> UpdateAsync(OrganizationVM model);
        Task<bool> DeleteAsync(long id);
    }
}
