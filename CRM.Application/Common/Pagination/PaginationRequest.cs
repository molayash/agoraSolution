using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRM.Application.Common.Pagination
{
    public class PaginationRequest
    {
        private const int MaxPageSize = 100;

        private int _pageSize = 10;

        public int PageNumber { get; set; } = 1;

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }

        // Extended options (can be ignored by services if not needed)
        public string? SearchText { get; set; }
        public string? SortBy { get; set; }      // Property name
        public string? SortDirection { get; set; } = "ASC"; // ASC or DESC
    }
}
