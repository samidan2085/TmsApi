namespace TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using  TmsApi.Application.DTOs;


public interface IEnrollmentService
{
    Task<EnrollmentResponseDto> GetByIdAsync(int courseId,int id, CancellationToken ct);
    Task<EnrollmentResponseDto?> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct);
    Task<List<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct);
   
    Task<bool> DeleteAsync(int courseId,CancellationToken ct);
    Task<bool> ExistsAsync(int studentId, string CourseCode, CancellationToken ct);
    Task<EnrollmentResponseDto> AddAsync(Enrollment enrollment,CancellationToken ct);
    Task<IEnumerable<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct);
    
  
}

/*public interface IEnrollmentService
{
    Task<List<EnrollmentRecord>> GetAllAsync();
    Task<EnrollmentRecord?> GetByIdAsync(string id);
    Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
    Task<bool> DeleteAsync(string id);
    Task ArchiveEnrollmentAsync(string id);
}
*/
