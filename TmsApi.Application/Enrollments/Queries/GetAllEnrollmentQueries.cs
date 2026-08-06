using MediatR;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;

namespace TmsApi.Application.Enrollments.Queries;

public record GetAllEnrollmentsQuery : IRequest<List<EnrollmentResponseDto>>;

internal class GetAllEnrollmentsQueryHandler : IRequestHandler<GetAllEnrollmentsQuery, List<EnrollmentResponseDto>>
{
    private readonly IEnrollmentService enrollmentService;

    public GetAllEnrollmentsQueryHandler(IEnrollmentService enrollmentService)
    {
        this.enrollmentService = enrollmentService;
    }

    public async Task<List<EnrollmentResponseDto>> Handle(GetAllEnrollmentsQuery request, CancellationToken ct)
    {
        return await enrollmentService.GetAllEnrollmentsAsync(ct);
    }
}