using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Common.Pagination
{
    public class PaginationService : IPaginationService
    {
        public async Task<PaginatedResult<T>> PaginateAsync<T>(
            IQueryable<T> query,
            PaginationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.PageNumber <= 0)
                request.PageNumber = 1;

            if (request.PageSize <= 0)
                request.PageSize = 10;

            // Total count before pagination
            var totalCount = await query.CountAsync(cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            var skip = (request.PageNumber - 1) * request.PageSize;

            var items = await query
                .Skip(skip)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new PaginatedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                TotalPages = totalPages,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                HasNextPage = request.PageNumber < totalPages,
                HasPreviousPage = request.PageNumber > 1
            };
        }
    }

}
