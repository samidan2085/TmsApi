using MediatR;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Enrollments.Commands;

public record RejectEnrollmentCommand(int Id)
    : IRequest<Enrollment?>;