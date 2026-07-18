namespace TmsApi.Application.DTOs;
public record StudentDetailDto(
int Id,
    string RegistrationNumber,
    string Name,
    int Age,
    decimal GPA,
    List<LinkDto> Links
);
