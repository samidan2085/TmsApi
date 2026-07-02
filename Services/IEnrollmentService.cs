namespace TmsApi.Services;
public interface IEnrollmentService
{
    Task<List<EnrollmentRecord>> GetAllAsync();
    Task<EnrollmentRecord?> GetByIdAsync(string id);
    Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode);
    Task<bool> DeleteAsync(string id);
    Task ArchiveEnrollmentAsync(string id);
}

