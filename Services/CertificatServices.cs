namespace TmsApi.Services;
using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;
using TmsApi.Data;
using TmsApi.Dtos;
using System.Runtime.ConstrainedExecution;
using Microsoft.AspNetCore.Http.HttpResults;


public class CertificatService(TmsDbContext context): ICertificatServices
{
    public async Task<List<CertificatResponseDto>> GetAllAsync(CancellationToken ct)
    {
        return  await context.certificates
        .AsNoTracking()
        .Select(c => new CertificatResponseDto(
            c.Id,
            c.SerialNumber,
             c.Title,
             c.IssuedAt,
            c.StudentId,
            c.CourseId

        ))
        .ToListAsync(ct);
    }
    public async Task<CertificatResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.certificates
        .AsNoTracking()
        .Where(c =>  c.Id ==id)
        .Select(c => new CertificatResponseDto(
            c.Id,
            c.SerialNumber,
            c.Title,
            c.IssuedAt,
            c.StudentId,
            c.CourseId
        ))
        .FirstOrDefaultAsync(ct);

    }
public async Task<CertificatResponseDto> CreateAsync(
    CreateCertificateRequest request,
    CancellationToken ct)
{
    var studentExists = await context.Students
        .AnyAsync(s => s.Id == request.StudentId, ct);
 
    if (!studentExists)
        throw new KeyNotFoundException("Student not found.");
 
    var courseExists = await context.Courses
        .AnyAsync(c => c.Id == request.CourseId, ct);
        
 
    if (!courseExists)
        throw new KeyNotFoundException("Course not found.");
 
    var duplicateSerial = await context.certificates
        .AnyAsync(c => c.SerialNumber == request.SerialNumber, ct);
 
    if (duplicateSerial)
        throw new InvalidOperationException(
            $"A certificate with serial number '{request.SerialNumber}' already exists.");
 
    var certificate = new Certificate
    {
        SerialNumber = request.SerialNumber,
        Title = request.Title,
        IssuedAt = DateTime.UtcNow,
        StudentId = request.StudentId,
        CourseId = request.CourseId
    };
 
    context.certificates.Add(certificate);
 
    await context.SaveChangesAsync(ct);
 
    return new CertificatResponseDto(
        certificate.Id,
        certificate.SerialNumber,
        certificate.Title,
       certificate.IssuedAt =DateTime.UtcNow,
        certificate.StudentId,
        certificate.CourseId);
}
    public async Task<CertificatResponseDto?> UpdateAsync( int id, UpdateCertificateRequest request, CancellationToken ct)
    {
        var certificate = await context.certificates.FirstOrDefaultAsync(c => c.Id == id,ct);
        if(certificate is null)
            return null;
 
        var studentExists = await context.Students
            .AnyAsync(s => s.Id == request.StudentId, ct);
 
        if (!studentExists)
            throw new KeyNotFoundException("Student not found.");
 
        var courseExists = await context.Courses
            .AnyAsync(c => c.Id == request.CourseId, ct);
 
        if (!courseExists)
            throw new KeyNotFoundException("Course not found.");
 
        var duplicateSerial = await context.certificates
            .AnyAsync(c => c.SerialNumber == request.SerialNumber && c.Id != id, ct);
 
        if (duplicateSerial)
            throw new InvalidOperationException(
                $"A certificate with serial number '{request.SerialNumber}' already exists.");
 
        certificate.SerialNumber = request.SerialNumber;
        certificate.Title = request.Title;
        certificate.StudentId = request.StudentId;
        certificate.CourseId = request.CourseId;
        await context.SaveChangesAsync(ct);
         return new CertificatResponseDto(
            certificate.Id,
            certificate.SerialNumber,
            certificate.Title,
            certificate.IssuedAt,
            certificate.StudentId,
            certificate.CourseId
        );
    }
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var certificate = await context.certificates.FirstOrDefaultAsync(c => c.Id == id,ct);
        if(certificate is null)
        return false;
        context.certificates.Remove(certificate);
        await context.SaveChangesAsync(ct);
        return true;
        
    }
    
}