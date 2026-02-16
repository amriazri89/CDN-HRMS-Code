using System.Reflection;

namespace HRMS.Application
{
    /// <summary>
    /// Assembly reference for MediatR and FluentValidation scanning
    /// </summary>
    public static class AssemblyReference
    {
        public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
    }
}