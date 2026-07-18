
using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;
using TmsApi.Application.DTOs;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Persistence.Context;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Services;

public class StudentService(TmsDbContext context) : IStudentService
{


    public async Task<List<StudentResponseDto>> GetAllAsync(CancellationToken ct)
    {
        return await context.Students
            .AsNoTracking()
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.Age,
                s.GPA,
                s.IsActive
            ))
            .ToListAsync(ct);
    }

    public async Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await context.Students
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new StudentResponseDto(
                s.Id,
                s.RegistrationNumber,
                s.Name,
                s.Age,
                s.GPA,
                s.IsActive
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<StudentResponseDto> CreateAsync(
        CreateStudentRequest request,
        CancellationToken ct)
    {
        var student = new Student
        {
            RegistrationNumber = request.RegistrationNumber,
            Name = request.Name,
            Age = request.Age,
            GPA = request.GPA
        };

        context.Students.Add(student);

        await context.SaveChangesAsync(ct);

        return new StudentResponseDto(
            student.Id,
            student.RegistrationNumber,
            student.Name,
            student.Age,
            student.GPA,
            student.IsActive
        );
    }
    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken ct)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (student is null)
        {
            return false;
        }

        context.Students.Remove(student);

        await context.SaveChangesAsync(ct);

        return true;
    }

    public async Task<StudentResponseDto?> UpdateAsync(int id, UpdateStudentRequest request, CancellationToken ct)
    {
        var student = await context.Students
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (student is null)
        {
            return null;
        }

        student.RegistrationNumber = request.RegistrationNumber;
        student.Name = request.Name;
        student.Age = request.Age;
        student.GPA = request.GPA;
        student.IsActive = request.IsActive;

        await context.SaveChangesAsync(ct);

        return new StudentResponseDto(
            student.Id,
            student.RegistrationNumber,
            student.Name,
            student.Age,
            student.GPA,
            student.IsActive
        );
    }

}