
using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Entities;
using TmsApi.Dtos;

namespace TmsApi.Services;



public class CourseService(TmsDbContext context, ILogger<CourseService> logger) : ICourseService
{
/*
    public async Task<Course?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Courses.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, ct);
            throw new NotImplementedException();
    }
   */
public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
context.Courses
.AsNoTracking()
.Where(c => c.Id == id)
.Select(c => new CourseResponseDto(
c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
.FirstOrDefaultAsync(ct);
public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
{
var course = new Course
{
Code = request.code,
Title = request.title,
MaxCapacity = request.MaxCapacity
};
context.Courses.Add(course);
await context.SaveChangesAsync(ct);
logger.LogInformation("Created course {CourseId} ({Code})", course.
Id, course.Code);
return (await GetByIdAsync(course.Id, ct))!;
}
public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
context.Courses.AnyAsync(c => c.Code == code, ct);
public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync( PagedRequest request,
    CancellationToken ct)
{
    // Step 1: Start with a no-tracking query
    IQueryable<Course> query = context.Courses.AsNoTracking();

    // Step 2: Apply search
    if (!string.IsNullOrWhiteSpace(request.Search))
    {
        query = query.Where(c =>
            EF.Functions.ILike(c.Title, $"%{request.Search}%") ||
            EF.Functions.ILike(c.Code, $"%{request.Search}%"));
    }

    // Step 3: Count before paging
    var totalCount = await query.CountAsync(ct);

    // Step 4: Apply sorting
    query = request.OrderBy switch
    {
        "Code" => request.Descending
            ? query.OrderByDescending(c => c.Code)
            : query.OrderBy(c => c.Code),

        "MaxCapacity" => request.Descending
            ? query.OrderByDescending(c => c.MaxCapacity)
            : query.OrderBy(c => c.MaxCapacity),

        _ => request.Descending
            ? query.OrderByDescending(c => c.Title)
            : query.OrderBy(c => c.Title)
    };

    // Step 5: Paging and projection
    var items = await query
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .Select(c => new CourseResponseDto(
            c.Id,
            c.Code,
            c.Title,
            c.MaxCapacity,
            c.Enrollments.Count))
        .ToListAsync(ct);

    // Step 6: Return paged response
    return new PagedResponse<CourseResponseDto>(
        items,
        totalCount,
        request.Page,
        request.PageSize);
}
/*
    public async Task<Course> CreateAsync(Course course, CancellationToken ct)
    {
        context.Courses.Add(course);
        await context.SaveChangesAsync(ct);
        logger.LogInformation("Course created with ID: {CourseId}", course.Id);
        return course;
        throw new NotImplementedException();
    }
     */
}



/* using TmsApi.Entities;

public interface ICourseService
{
    Task<List<Course>> GetAllAsync();
    Task<Course?> GetByCodeAsync(string code);
}
public class CourseService : ICourseService
{
    private readonly List<Course> _courses =
    [
        new Course(){
            
            Code="CS-101", Title="C# Fundamentals", 
            MaxCapacity=30 },
        new Course(){
            Code="WEB-201", Title="ASP.NET Core",   MaxCapacity=4 },
        new Course(){
            Code="DB-301", Title="SQL Server", MaxCapacity=3 }
    ];

    public Task<List<Course>> GetAllAsync()
        => Task.FromResult(_courses);

    public Task<Course?> GetByCodeAsync(string code)
        => Task.FromResult(
            _courses.FirstOrDefault(c => c.Code == code));
}
*/