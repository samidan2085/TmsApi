
namespace TmsApi.Application.DTOs;

public record CertificateDetailDto(
    int Id,
    string SerialNumber,
    string Title,
    DateTime IssuedAt,
    int StudentId,
    int CourseId,
    List<LinkDto> Links);