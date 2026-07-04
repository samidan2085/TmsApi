
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