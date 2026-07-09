using SGCN.Application.DTOs.Certificates;
using SGCN.Application.DTOs.Common;

namespace SGCN.Application.Interfaces;

public interface ICertificateService
{
    Task<ApiResponse<IReadOnlyCollection<CertificateResponse>>> GetAllAsync(
        string currentUserId,
        bool isCitizen,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CertificateResponse>> GetByIdAsync(
        Guid id,
        string currentUserId,
        bool isCitizen,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CertificatePreviewResponse>> PreviewFromRequestAsync(
        Guid requestId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CertificatePreviewResponse>> PreviewFromBirthRecordAsync(
        Guid birthRecordId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CertificateResponse>> GenerateFromRequestAsync(
        Guid requestId,
        string createdByUserId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CertificateResponse>> GenerateFromBirthRecordAsync(
        Guid birthRecordId,
        string createdByUserId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CertificateResponse>> AnnulAsync(
        Guid id,
        AnnulCertificateRequest request,
        CancellationToken cancellationToken = default);
}
