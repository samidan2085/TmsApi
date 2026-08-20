using MediatR;
using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Application.Enrollments.Commands;

public class RejectEnrollmentCommandHandler
    : IRequestHandler<RejectEnrollmentCommand, Enrollment?>
{
    private readonly ITmsDbContext _context;

    public RejectEnrollmentCommandHandler(ITmsDbContext context)
    {
        _context = context;
    }

    public async Task<Enrollment?> Handle(
        RejectEnrollmentCommand request,
        CancellationToken ct)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(
                e => e.Id == request.Id,
                ct);

        if (enrollment is null)
            return null;

        // Change status
        enrollment.Status = "panding";

        await _context.SaveChangesAsync(ct);

        return enrollment;
    }
}