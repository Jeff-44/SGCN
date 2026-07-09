using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Referentials.Commune;

namespace SGCN.Application.Interfaces;

public interface ICommuneService
{
    Task<ApiResponse<IReadOnlyCollection<CommuneResponse>>> GetAllAsync(string? search, Guid? departmentId, bool? isActive, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommuneResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommuneResponse>> CreateAsync(CreateCommuneRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommuneResponse>> UpdateAsync(Guid id, UpdateCommuneRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommuneResponse>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CommuneResponse>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
