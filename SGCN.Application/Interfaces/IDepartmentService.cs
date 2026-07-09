using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Referentials.Department;

namespace SGCN.Application.Interfaces;

public interface IDepartmentService
{
    Task<ApiResponse<IReadOnlyCollection<DepartmentResponse>>> GetAllAsync(string? search, bool? isActive, CancellationToken cancellationToken = default);
    Task<ApiResponse<DepartmentResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<DepartmentResponse>> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<DepartmentResponse>> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<DepartmentResponse>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<DepartmentResponse>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
