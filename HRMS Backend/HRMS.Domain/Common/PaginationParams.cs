using System;

namespace HRMS.Domain.Common
{
   public class PaginationParams
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "Name";
        public bool SortDescending { get; set; } = false;
        public string? SearchTerm { get; set; }
        public bool IncludeArchived { get; set; } = false; 
    }
}
