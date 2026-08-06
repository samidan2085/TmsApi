
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence.Context;
namespace TmsApi.Infrastructure.Services;



public class CourseService : ICourseService
{
    /*
        public async Task<Course?> GetByIdAsync(int id, CancellationToken ct)
        {
            return await context.Courses.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id, ct);
                throw new NotImplementedException();
        }
       */
    private readonly TmsDbContext _context;
   
    private readonly ILogger<CourseService> _logger;

    public CourseService(
        TmsDbContext context,
       
        ILogger<CourseService> logger)
    {
        _context = context;
       
        _logger = logger;
    }
    public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
    _context.Courses
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
        _context.Courses.Add(course);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Created course {CourseId} ({Code})", course.
        Id, course.Code);
        return (await GetByIdAsync(course.Id, ct))!;
    }
    public Task<bool> CodeExistsAsync(string code, CancellationToken ct)
    {
        return _context.Courses
        .AsNoTracking()
        .AnyAsync(c => c.Code == code);
    }

    public async Task<TmsApi.Application.DTOs.PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request,
        CancellationToken ct)
    {
        // Step 1: Start with a no-tracking query
        IQueryable<Course> query = _context.Courses.AsNoTracking();

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
        return new TmsApi.Application.DTOs.PagedResponse<CourseResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<CourseResponseDto?> UpdateAsync(
    int id,
    UpdateCourseRequest request,
    CancellationToken ct)
{
    var course = await _context.Courses
        .FirstOrDefaultAsync(c => c.Id == id, ct);

    if (course == null)
        return null;

    course.Title = request.Title;
    course.Code = request.Code;
    course.MaxCapacity = request.MaxCapacity;

    await _context.SaveChangesAsync(ct);

    return new CourseResponseDto(
        course.Id,
        course.Title,
        course.Code,
        course.MaxCapacity,
        course.Enrollments.Count);
}
    public async Task<Course?> GetByCodeAsync(string code, CancellationToken ct)
    {
        return await _context.Courses
        .Include(c => c.Enrollments)
        .FirstOrDefaultAsync(c => c.Code == code, ct);
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == id, ct);
        if (course is null)
        {
            return false;
        }
        _context.Courses.Remove(course);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    
       public async Task<List<CourseDto>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Courses
            .AsNoTracking()
            .OrderBy(c => c.Title)
            .Select(c => new CourseDto(
                c.Id,
                c.Title,
                c.Code,
                c.MaxCapacity,
                c.Enrollments.Count))
            .ToListAsync(ct);
    }
    

    public async Task<CourseDto?> UpdateAsyncn(int id, UpdateCourseRequest request, CancellationToken ct)
    {
       var course = await _context.Courses
        .FirstOrDefaultAsync(c => c.Id == id, ct);

    if (course == null)
        return null;

    course.Title = request.Title;
    course.Code = request.Code;
    course.MaxCapacity = request.MaxCapacity;

    await _context.SaveChangesAsync(ct);

    return new CourseDto(
        course.Id,
        course.Title,
        course.Code,
        course.MaxCapacity,
        course.Enrollments.Count);
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