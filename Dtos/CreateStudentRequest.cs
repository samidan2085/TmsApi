using System.ComponentModel.DataAnnotations;

namespace TmsApi.Dtos;

public class CreateStudentRequest
{
    [Required]
    [RegularExpression(@"^STU-\d{4}$",
        ErrorMessage = "Registration Number must be in the format STU-0001.")]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[A-Za-z\s]+$",
        ErrorMessage = "Name can contain only letters and spaces.")]
    public string Name { get; set; } = string.Empty;

    [Range(16, 30,
        ErrorMessage = "Age must be between 16 and 100.")]
    public int Age { get; set; }

    [Range(typeof(decimal), "0.00", "4.00",
        ErrorMessage = "GPA must be between 0.00 and 4.00.")]
    public decimal GPA { get; set; }
}