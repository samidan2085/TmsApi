
namespace TmsApi.Application.Interfaces;

using System.Collections.Generic;
using TmsApi.Application.DTOs;
using TmsApi.Domain.Entities;

public interface ICourseService
{
    Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct);
    Task<bool> CodeExistsAsync(string code, CancellationToken ct);
    Task<TmsApi.Application.DTOs.PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct);
    Task<CourseResponseDto?> UpdateAsync(int id,UpdateCourseRequest request,CancellationToken ct);
    Task<Course?> GetByCodeAsync(string code,CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}

public record PagedResponse<T>(IEnumerable<T> Items, int PageNumber, int PageSize, int TotalCount);