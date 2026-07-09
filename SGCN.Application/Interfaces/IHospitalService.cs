using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Referentials.Hospital;

namespace SGCN.Application.Interfaces;

public interface IHospitalService
{
    Task<ApiResponse<IReadOnlyCollection<HospitalResponse>>> GetAllAsync(string? search, Guid? communeId, Guid? departmentId, bool? isActive, CancellationToken cancellationToken = default);
    Task<ApiResponse<HospitalResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<HospitalResponse>> CreateAsync(CreateHospitalRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<HospitalResponse>> UpdateAsync(Guid id, UpdateHospitalRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<HospitalResponse>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<HospitalResponse>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
