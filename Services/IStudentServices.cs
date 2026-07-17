namespace TmsApi.Services;

using System.Collections.Generic;
using TmsApi.Entities;
using TmsApi.Dtos;

public interface IStudentService
{
    Task<List<StudentResponseDto>> GetAllAsync(CancellationToken ct);
    Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct);

    Task<StudentResponseDto?> UpdateAsync(int id,UpdateStudentRequest request, CancellationToken ct);

    Task<bool> DeleteAsync(int id, CancellationToken ct);
}

