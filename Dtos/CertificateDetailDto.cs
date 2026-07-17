namespace TmsApi. Dtos;
public record CertificateDetailDto(
    int Id,
    string SerialNumber,
    string Title,
      DateTime IssudAt,
    int StudentId,
  
    int CourseId,
    List<LinkDto> Links);