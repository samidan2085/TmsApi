using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace TmsApi.Controllers.V2;

[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/grades")]
public class GradesController : ControllerBase
{
    [HttpPost]
    public IActionResult SubmitGrade(
        [FromBody] GradeRequest request)
    {
        Console.WriteLine(
            $"Student: {request.StudentId}"
        );

        Console.WriteLine(
            $"Course: {request.CourseId}"
        );

        Console.WriteLine(
            $"Score: {request.Score}"
        );

        return Ok(new
        {
            id = 1,
            studentId = request.StudentId,
            courseId = request.CourseId,
            score = request.Score,
            message = "Grade saved successfully"
        });
    }
}

public record GradeRequest(
    int StudentId,
    int CourseId,
    decimal Score
);