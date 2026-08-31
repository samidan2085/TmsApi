using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Data;
using System.Security.Claims;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;

namespace TmsApi.Controllers.V2;

[ApiController]
[Route("api/v{version:apiVersion}/courses")]
[ApiVersion("2.0")]
[Authorize(Roles="Admin")]
[AllowAnonymous]
public class CoursesController(
    ICourseService courseService,
    ICachedCourseService cachedCourseService) : ControllerBase
{
    [HttpGet]
    [EnableRateLimiting("anonymous")]
    public async Task<IActionResult> GetCourses(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var courses = await cachedCourseService.GetAllCoursesAsync(ct);

        var totalCount = courses.Count;

        var rows = courses
            .OrderBy(c => c.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return Ok(new
        {
            data = rows,
            meta = new
            {
                totalCount,
                page,
                pageSize,
                totalPages,
                hasNext = page < totalPages,
                hasPrevious = page > 1
            }
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(CourseResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request, CancellationToken ct)
    {
        if (await courseService.CodeExistsAsync(request.code, ct))
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course code already exists",
                Detail = $"A Course with code '{request.code}' is already registered.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var modifiedRequest = request with 
        { 
           InstructorId = request.InstructorId ?? currentUserId 
        };

        var result = await courseService.CreateAsync(modifiedRequest, ct);

        // Invalidate cache when a new course is created
        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        return CreatedAtAction(
            nameof(GetCourses),
            new { version = "2.0", id = result.Id },
            result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateCourse(
        int id,
        UpdateCourseRequest request,
        CancellationToken ct)
    {
        var course = await courseService.UpdateAsync(id, request, ct);

        if (course is null)
        {
            return NotFound();
        }

        // Remove cached courses after a successful update
        await cachedCourseService.InvalidateCourseCacheAsync(ct);

        return Ok(course);
    }
#pragma warning disable ASP0026 // [Authorize] overridden by [AllowAnonymous] from farther away
    [Authorize(Roles = "Admin")]
#pragma warning restore ASP0026 // [Authorize] overridden by [AllowAnonymous] from farther away
#pragma warning restore ASP0026 // [Authorize] overridden by [AllowAnonymous] from farther away
    [HttpDelete("{id:int}")]
public async Task<IActionResult> DeleteCourse(
    int id,
    CancellationToken ct)
{
    var deleted = await courseService.DeleteAsync(id, ct);

    if (!deleted)
    {
        return NotFound(new
        {
            message = "Course not found."
        });
    }

    return NoContent();
}
}