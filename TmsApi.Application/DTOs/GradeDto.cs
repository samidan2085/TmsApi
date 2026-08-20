namespace TmsApi.Application.DTOs;

public record GradeRequest(
    int StudentId,
    int CourseId,
    decimal Score
);

public record GradeResponse(
    int Id,
    int StudentId,
    int CourseId,
    decimal Score,
    string Message
);