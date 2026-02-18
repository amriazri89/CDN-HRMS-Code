using System;

namespace HRMS.Domain.Common
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string? Detail { get; set; } // nullable, hide in production
        public DateTime Timestamp { get; set; }
    }
}
