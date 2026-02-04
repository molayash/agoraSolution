using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Common.Pagination
{
    public interface IPaginationService
    {
        Task<PaginatedResult<T>> PaginateAsync<T>(IQueryable<T> query, PaginationRequest request, CancellationToken cancellationToken = default);
    }
}
