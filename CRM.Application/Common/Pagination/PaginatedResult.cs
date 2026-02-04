using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Common.Pagination
{
    public class PaginatedResult<T>
    {
        public IReadOnlyList<T> Items { get; set; } = new List<T>();

        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
