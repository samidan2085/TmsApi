namespace TmsApi.Services;

using System.Collections.Generic;
using TmsApi.Dtos;
public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
}

public record PagedResponse<T>(IEnumerable<T> Items, int PageNumber, int PageSize, int TotalCount);