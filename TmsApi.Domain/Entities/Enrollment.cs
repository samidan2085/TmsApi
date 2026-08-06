
namespace TmsApi.Domain.Entities;

public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public decimal? Grade { get; set; } // Nullable, as student may be currently enrolled
    public bool IsArchived { get; set; }
    public string Status { get; set; } = "pending"; // Default status is "pending"
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    // Navigation properties back to entities
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}