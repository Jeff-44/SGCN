using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Users;

namespace SGCN.Application.Interfaces;

public interface IUserService
{
    Task<ApiResponse<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<UserResponse>>> GetAllAsync(string? search, string? role, bool? isActive, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserResponse>> UpdateAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserResponse>> ActivateAsync(string id, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserResponse>> DeactivateAsync(string id, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserResponse>> GetProfileAsync(string userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
}
