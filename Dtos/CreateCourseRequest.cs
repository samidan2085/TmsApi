using System.ComponentModel.DataAnnotations;
namespace TmsApi.Services;
public record CreateCourseRequest
{
    [Required,RegularExpression(@"^[A-Z]{2,4}-\d{3}$", 
    ErrorMessage = "Code must be in the format 'CSE-101'")]
    public required string code { get; init; }
    [Required, MaxLength(200)]
    public required string title { get; init; }
    [Required, Range(1, 200)]
    public required int MaxCapacity { get; init; 
    }
}