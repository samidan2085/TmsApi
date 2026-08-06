namespace TmsApi.Application.Enrollments.Queries;

using MediatR;
using TmsApi.Application.Interfaces;
using TmsApi.Application.DTOs;
using TmsApi.Application.Enrollments.Queries;
public class GetCoursesQueryHandler
    : IRequestHandler<GetCoursesQuery, List<CourseDto>>
{
    private readonly ICachedCourseService _cachedCourseService;

    public GetCoursesQueryHandler(ICachedCourseService cachedCourseService)
    {
        _cachedCourseService = cachedCourseService;
    }

    public async Task<List<CourseDto>> Handle(
        GetCoursesQuery request,
        CancellationToken ct)
    {
        return await _cachedCourseService.GetAllCoursesAsync(ct);
    }
}