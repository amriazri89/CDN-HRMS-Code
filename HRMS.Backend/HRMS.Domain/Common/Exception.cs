using System;

namespace HRMS.Domain.Common
{
    // Not found resource exception
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }

    // Validation error (invalid data / business rule)
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }

    // Unauthorized (not logged in / invalid token)
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }

    // Forbidden (logged in but no permission)
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
}
