using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Enrollments.Queries;
[ApiController]
[Route("api/v{version:apiVersion}/enrollments")]
[ApiVersion("2.0")]
public class EnrollmentsController(IMediator mediator) : ControllerBase
{
[HttpPost]
public async Task<IActionResult> Enroll(
EnrollStudentCommand command, CancellationToken ct)
{
var result = await mediator.Send(command, ct);
return result.Match<IActionResult>(
onSuccess: created => CreatedAtAction(
nameof(GetSchedule),
new { studentId = created.StudentId },
created),
onFailure: error =>
{
var status = error.Code switch
{
"course_not_found" => StatusCodes.Status404NotFound,
"course_full" or "already_enrolled" => StatusCodes.
Status409Conflict,
_ => StatusCodes.Status400BadRequest
};
return Problem(
statusCode: status,
title: "Enrollment rejected",
detail: error.Message,
type: $"https://tms.local/errors/{error.Code}");
});
}
[HttpGet("{studentId}/schedule")]
public async Task<IActionResult> GetSchedule(
int studentId, CancellationToken ct)
{
var schedule = await mediator.Send(
new GetStudentScheduleQuery(studentId), ct);
return Ok(schedule);
}
}




/*using Microsoft.AspNetCore.Mvc;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
namespace TmsApi.Api.Controllers;

[ApiController]

[Route("api/courses/{courseId:int}/enrollments")]
[Tags("Enrollments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class EnrollmentsController(ICourseService courseService, IEnrollmentService enrollmentService) : ControllerBase
{
[HttpGet("{id:int}", Name = nameof(GetEnrollment))]
[ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[EndpointSummary("Get one enrolment for a course")]
public async Task<IActionResult> GetEnrollment(int courseId, int id,
    CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);
        return enrollment is not null ? Ok(enrollment) : NotFound();
    }
  
[HttpPost]
[ProducesResponseType(typeof(EnrollmentResponseDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.
Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
[EndpointSummary("Enrol a student in a course")]
[EndpointDescription("Returns 404 if the course does not exist, 409 if the course has reached MaxCapacity.")]
    public async Task<IActionResult> EnrollStudent(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        // TODO 3: Look up the parent course (courseService.GetByIdAsync). If null, return NotFound().
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null)
        {
            return NotFound();
        }
// Then check capacity (course.EnrollmentCount >= course.MaxCapacity).

        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail = $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });
        }
    
        var enrollment = await enrollmentService.CreateAsync(courseId, request, ct);
        if (enrollment is null)
        {
            return NotFound();
        }
    
        return CreatedAtAction(nameof(GetEnrollment), new { courseId, id = enrollment.Id }, enrollment);
    }
        [HttpGet(Name = "ListCourseEnrollments")]
        [ProducesResponseType(typeof(IReadOnlyList<EnrollmentResponseDto>),StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[EndpointSummary("List enrolments for a course")]
public async Task<IActionResult> GetEnrollments(
    int courseId,
    CancellationToken ct)
{
    var course = await courseService.GetByIdAsync(courseId, ct);

    if (course is null)
    {
        return NotFound();
    }

    var enrollments = await enrollmentService.GetByCourseAsync(courseId, ct);

    return Ok(enrollments);
    throw new NotImplementedException();
}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEnrollment(int courseId, CancellationToken ct)
    {
        var delete = await enrollmentService.DeleteAsync(courseId,ct);
        if (!delete)
        {
            return NotFound();
        }
        return NoContent();
    }
}
// If full, return Conflict(new ProblemDetails { ... })with:
        // Title = "Course is full"
        // Detail = $"Course '{course.Title}' has reached itsmaximum capacity of {course.MaxCapacity}."
        // Status = StatusCodes.Status409Conflict
        // Otherwise, call enrollmentService.CreateAsync and return CreatedAtAction(nameof(GetEnrollment),
        // new { courseId, id = enrollment.Id }, enrollment).

     
    

/*
using Microsoft.AspNetCore.Mvc;
using TmsApi.Services;
using TmsApi.Controllers;

[ApiController]
[Route("api/enrollments")]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
// GET /api/enrollments returns all enrollment records
  [HttpGet]
public async Task<IActionResult> GetAll()
{
var enrollments = await enrollmentService.GetAllAsync();
return Ok(enrollments);
}
// GET /api/enrollments/{id} returns one or 404
[HttpGet("{id}")]
public async Task<IActionResult> GetById(string id)
{
var record = await enrollmentService.GetByIdAsync(id);
return record is not null ? Ok(record) : NotFound();
}


// POST /api/enrollments creates and returns 201 with Location header
[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateEnrollmentRequest request)
{
var record = await enrollmentService.EnrollAsync(request.StudentId, request.CourseCode);
return CreatedAtAction(nameof(GetById), new { id = record.Id }, record);
}
public record CreateEnrollmentRequest(string StudentId, string CourseCode);
// DELETE /api/enrollments/{id} returns 204 or 404
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(string id)
{
var deleted = await enrollmentService.DeleteAsync(id);
return deleted ? NoContent() : NotFound();
}
// --- NEW ARCHIVE ENDPOINT ---
    // PUT /api/enrollments/{id}/archive
    [HttpPut("{id}/archive")]
    public async Task<IActionResult> ArchiveEnrollment(string id)
    {
        try
        {
            await enrollmentService.ArchiveEnrollmentAsync(id);
            return NoContent(); // Returns HTTP 204 on success
        }
        catch (KeyNotFoundException ex)
        {
            // Returns HTTP 404 with a clear message if the ID is invalid
            return NotFound(new { message = ex.Message }); 
        }
    }

}
public record CreateEnrollmentRequest(string StudentId, string CourseCode);
*/
