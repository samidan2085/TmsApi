using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;
using TmsApi.Infrastructure.Persistence.Context;

namespace TmsApi.Infrastructure.Services;
public class EnrollmentService(TmsDbContext context, ILogger<EnrollmentService> logger)
    : IEnrollmentService
{
    public Task<EnrollmentResponseDto> GetByIdAsync(
        int courseId,
        int id,
        CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .FirstAsync(ct);

    public async Task<EnrollmentResponseDto?> CreateAsync(
        int courseId,
        EnrollStudentRequest request,
        CancellationToken ct)
    {
        // Create a new Enrollment
        var enrollment = context.Enrollments.Add(new()
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        }).Entity;

        // Save changes
        await context.SaveChangesAsync(ct);

        // Log information
        logger.LogInformation(
            "Student {StudentId} enrolled in Course {CourseId} with EnrollmentId {EnrollmentId}",
            enrollment.StudentId,
            enrollment.CourseId,
            enrollment.Id);

        // Return the created enrollment
        return await GetByIdAsync(courseId, enrollment.Id, ct);
    }

    public async Task<List<EnrollmentResponseDto>> GetByCourseAsync(
        int courseId,
        CancellationToken ct)
    {
        return await context.Enrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(
                e.Id,
                e.CourseId,
                e.StudentId,
                e.EnrolledAt))
            .ToListAsync(ct);
    }
    public async Task<bool> DeleteAsync(int courseId, CancellationToken ct)
    {
        var delete = await context.Enrollments.FirstOrDefaultAsync(e => e.CourseId == courseId, ct);
        if (delete is null)
            return false;
        context.Enrollments.Remove(delete);
        await context.SaveChangesAsync();
        return true;

    }
     public async Task<bool> ExistsAsync(
    int studentId,
    string courseCode,
    CancellationToken ct)
{
    return await context.Enrollments
        .Include(e => e.Course)
        .AnyAsync(
            e => e.StudentId == studentId &&
                 e.Course.Code == courseCode,
            ct);
}

public async Task<EnrollmentResponseDto> AddAsync(
    Enrollment enrollment,
    CancellationToken ct)
{
    context.Enrollments.Add(enrollment);

    await context.SaveChangesAsync(ct);

    return new EnrollmentResponseDto(
        enrollment.Id,
        enrollment.CourseId,
        enrollment.StudentId,
        enrollment.EnrolledAt);
}

public async Task<IEnumerable<Enrollment>> GetByStudentIdAsync(
    int studentId,
    CancellationToken ct)
{
    return await context.Enrollments
        .AsNoTracking()
        .Include(e => e.Course)
        .Where(e => e.StudentId == studentId)
        .ToListAsync(ct);
}

}




/*
// --- The contract --- 
using Microsoft.EntityFrameworkCore; 
namespace TmsApi.Services;
using TmsApi.Data;
using TmsApi.Services;
public class EnrollmentService : IEnrollmentService
{
    private readonly Dictionary<string, EnrollmentRecord> _store = new();
    private readonly ILogger<EnrollmentService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TmsDbContext _dbContext;

    public EnrollmentService(TmsDbContext dbContext,ILogger<EnrollmentService> logger, IServiceScopeFactory scopeFactory)
    {
        _dbContext = dbContext;

        _logger = logger;
        _scopeFactory = scopeFactory;
    }
    public Task<EnrollmentRecord> EnrollAsync(string studentId, string courseCode)
    {
        // Check for duplicate enrollment
        var existing = _store.Values
            .FirstOrDefault(e => e.StudentId == studentId && e.CourseCode == courseCode);
        if (existing is not null)
        {
            _logger.LogWarning("Duplicate enrollment attempt {StudentId} already in {CourseCode} (record {EnrollmentId})", studentId, courseCode, existing.Id);
            return Task.FromResult(existing);
        }
        var id = Guid.NewGuid().ToString("N")[..8];
        var record = new EnrollmentRecord(id, studentId, courseCode, DateTime.UtcNow);
        _store[id] = record;
        _logger.LogInformation("Enrolled {StudentId} in {CourseCode} record {EnrollmentId}", studentId, courseCode, id);
        return Task.FromResult(record);
    }
    public Task<EnrollmentRecord?> GetByIdAsync(string id)
    {
        _store.TryGetValue(id, out var record);
        if (record is null)
        {
            _logger.LogWarning("Enrollment {EnrollmentId} not found", id);
        }
        return Task.FromResult(record);
    }
    public Task<List<EnrollmentRecord>> GetAllAsync()
    {
        List<EnrollmentRecord> all = _store.Values.ToList();
        return Task.FromResult(all);
    }
    public Task<bool> DeleteAsync(string id)
    {
        var removed = _store.Remove(id);
        if (removed)
            _logger.LogInformation("Deleted enrollment {EnrollmentId}", id);
        else
            _logger.LogWarning("Delete failed enrollment {EnrollmentId} not found", id);
        return Task.FromResult(removed);
    }
   // EnrollmentService.cs

// 1. Ensure your interface match: Task ArchiveEnrollmentAsync(string id);
public async Task ArchiveEnrollmentAsync(string id)
{
    // 1. Convert the string into an integer ID
    if (!int.TryParse(id, out int enrollmentId))
    {
        throw new ArgumentException("ID must be a valid integer.");
    }

    // 2. Now both sides of the == are integers!
    var enrollment = await _dbContext.Enrollments
        .FirstOrDefaultAsync(e => e.Id == enrollmentId);

    if (enrollment is null)
        throw new KeyNotFoundException($"Enrollment record with ID '{id}' was not found.");

    enrollment.IsArchived = true;
    await _dbContext.SaveChangesAsync();
}
}
// --- The data shape --- 
public record EnrollmentRecord(
string Id, string StudentId, string CourseCode, DateTime EnrolledAt);
public class EnrollmentWorker(IServiceScopeFactory scopeFactory)
{
    public async Task ProcessBatch()
    {
        // TODO 2: Create a short-lived scope using the injected factory.
        using var scope = scopeFactory.CreateScope();

        // TODO 3: Resolve the scoped service from the new scope's provider.
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

        // TODO 4: Use the service.
        // Example:
        var enrollments = await svc.GetAllAsync();

    }

}
  
*/