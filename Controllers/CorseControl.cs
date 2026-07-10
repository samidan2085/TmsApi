
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TmsApi.Entities;
using TmsApi.Services;
using TmsApi.Dtos;

namespace TmsApi.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(
    ICourseService courseService,
    LinkGenerator linkGenerator


) : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetCourseById))]
    public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
    {
        var course = await courseService.GetByIdAsync(id, ct);
        if (course is null)
            return NotFound();


        var selfHref = linkGenerator.GetPathByName(
        HttpContext,
        nameof(GetCourseById),
         new { id }) ?? string.Empty;
        var enrollmentsHref = linkGenerator.GetPathByAction(
       HttpContext,
       action: "GetEnrollments",
       controller: "Enrollments",
       values: new { courseId = id }) ?? string.Empty;
        var links = new List<LinkDto>
    {
        new(selfHref, "self",  "GET" ),
        new(selfHref,"UPDATE","PUT" ),
        new(selfHref,"delete", "DELETE" ),
        new(enrollmentsHref,  "enrollments","GET")
    
    };

        if (course.EnrollmentCount < course.MaxCapacity)
        {
             links.Add(new (
        
             enrollmentsHref,
            "enroll",
           "POST"
          
        ));
        }

        var detailDto = new
        {
            Id = course.Id,
            Code = course.Code,
            Title = course.Title,
            MaxCapacity = course.MaxCapacity,
            EnrollmentCount = course.EnrollmentCount,
            Links = links
        };

        return Ok(detailDto);
        throw new NotImplementedException();
    }

        /*
        [HttpPost]
        public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
        {
        var result = await courseService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetCourseById), new { id = result.Id }, result);
        }
        */
        [HttpPost]
        public async Task<IActionResult> CreateCourse(CreateCourseRequest request, CancellationToken ct)
        {
            // TODO 1: Call courseService.CodeExistsAsync(request.Code, ct).
            var codeExists = await courseService.CodeExistsAsync(request.code, ct);
            // If it returns true, return Conflict(new ProblemDetails{ ... }) with:
            if (codeExists)
            {
                return Conflict(new ProblemDetails
                {
                    // Title = "Course code already exists"
                    Title = "Course code already exists",
                    // Detail = $"A course with code '{request.Code}' is already registered."
                    Detail = $"A course with code '{request.code}' is already registered.",
                    // Status = StatusCodes.Status409Conflict
                    Status = StatusCodes.Status409Conflict
                    // You do not need a try/catch the framework's ProblemDetails middleware handles unhandled exceptions.
                });
            }
            var result = await courseService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetCourseById), new
            {
                id = result.Id
            }, result);
        }
        [HttpGet]
        public async Task<IActionResult> GetCourses(
    [FromQuery] PagedRequest request, CancellationToken ct)
        {
            var result = await courseService.GetCoursesAsync(request, ct);
            return Ok(result);
        }
    }
/*
public class CoursesController(ICourseService courseService) : ControllerBase
{
[HttpGet("{id:int}", Name = nameof(GetCourseById))]
public async Task<IActionResult> GetCourseById(int id, CancellationToken ct)
{
// TODO 3: Call courseService.GetByIdAsync(id, ct).
// Return Ok(course) when the result is not null.
// Return NotFound() when the result is null.

var course = await courseService.GetByIdAsync(id, ct);
if (course is not null)
{
    return Ok(course);
}
else
{
    return NotFound();
}


throw new NotImplementedException();
}
[HttpPost]

 [HttpPost]
public async Task<IActionResult> CreateCourse(Course course, CancellationToken ct)
{
// TODO 4: Call courseService.CreateAsync(course, ct).
//var result = await courseService.CreateAsync(course, ct);
var result= await courseService.CreateAsync(Course, ct);
// Return CreatedAtAction(nameof(GetCourseById), new {id = result.Id }, result).
return CreatedAtAction(nameof(GetCourseById), new {id = result.Id }, result);
// CreatedAtAction sets the Location header automatically.


throw new NotImplementedException();
}

}
*/














/*using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using TmsApi.Data;
[ApiController]
[Route("api/courses")]
public class CoursesController(
    ICourseService courseService,
    TmsDbContext context)
    : ControllerBase
{
    private readonly TmsDbContext _context = context;
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await courseService.GetAllAsync());
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var course = await courseService.GetByCodeAsync(code);

        return course is not null
            ? Ok(course)
            : NotFound();
    }
    [HttpGet("top-5-courses")]
    public async Task<IActionResult> GetTop5Courses(
    CancellationToken ct = default)
    {
        var topCourses = await _context.Enrollments
            .GroupBy(e => e.CourseId)
            .Select(g => new
            {
                CourseId = g.Key,
                EnrollmentCount = g.Count()
            })
            .OrderByDescending(x => x.EnrollmentCount)
            .Take(5)
            .ToListAsync(ct);

        return Ok(topCourses);
    }
}
*/