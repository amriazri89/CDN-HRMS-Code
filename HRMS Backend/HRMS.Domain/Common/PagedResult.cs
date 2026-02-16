using System;
using System.Collections.Generic;

namespace HRMS.Domain.Common
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        // Optional: total pages for convenience
        public int TotalPages => (int)System.Math.Ceiling(TotalCount / (double)PageSize);
    }
}
