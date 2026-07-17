namespace TmsApi.Dtos;
using System.ComponentModel.DataAnnotations;
public record CertificatResponseDto(
int Id,
string SerialNumber,
string Title,
DateTime IssuedAt,
int StudentId,
int CourseId
);