
namespace TmsApi.Application.Interfaces;
using TmsApi.Application.DTOs;
public interface ICertificatServices
{Task<List<CertificatResponseDto>> GetAllAsync(CancellationToken ct);

Task<CertificatResponseDto?> GetByIdAsync(int id,CancellationToken ct);

Task<CertificatResponseDto> CreateAsync(CreateCertificateRequest request,CancellationToken ct);

Task<CertificatResponseDto?> UpdateAsync(int id,UpdateCertificateRequest request,CancellationToken ct);

Task<bool> DeleteAsync(int id,CancellationToken ct);
}