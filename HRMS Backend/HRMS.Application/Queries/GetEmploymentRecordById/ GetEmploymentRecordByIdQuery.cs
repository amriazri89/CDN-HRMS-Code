// =====================================================
// FILE 1: GetEmploymentRecordByIdQuery.cs
// Path: HRMS.Application/Queries/GetEmploymentRecordById/
// =====================================================
using MediatR;
using HRMS.Domain.Entities;

namespace HRMS.Application.Queries.GetEmploymentRecordById;

public class GetEmploymentRecordByIdQuery : IRequest<EmploymentRecord>
{
    public Guid EmploymentRecordId { get; set; }
}