using SGCN.Application.DTOs.BirthRecords;
using SGCN.Application.DTOs.Common;

namespace SGCN.Application.Interfaces;

public interface IBirthRecordService
{
    Task<ApiResponse<IReadOnlyCollection<BirthRecordResponse>>> GetAllAsync(
        string currentUserId,
        bool isAdministrator,
        string? search,
        Guid? hospitalId,
        bool? isLocked,
        bool? isActive,
        DateOnly? birthDateFrom,
        DateOnly? birthDateTo,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<BirthRecordResponse>> GetByIdAsync(
        Guid id,
        string currentUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<BirthRecordResponse>>> GetWithoutCertificateAsync(
        string currentUserId,
        bool isAdministrator,
        string? search,
        Guid? hospitalId,
        DateOnly? birthDateFrom,
        DateOnly? birthDateTo,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<BirthRecordResponse>> CreateAsync(
        CreateBirthRecordRequest request,
        string createdByUserId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<BirthRecordResponse>> UpdateAsync(
        Guid id,
        UpdateBirthRecordRequest request,
        string currentUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<BirthRecordResponse>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<BirthRecordResponse>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
}
