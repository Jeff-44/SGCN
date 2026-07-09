using SGCN.Application.DTOs.BirthRecords;
using SGCN.Application.DTOs.CertificateRequests;
using SGCN.Application.DTOs.Common;

namespace SGCN.Application.Interfaces;

public interface ICertificateRequestService
{
    Task<ApiResponse<IReadOnlyCollection<CertificateRequestResponse>>> GetAllAsync(
        string currentUserId,
        bool isCitizen,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CertificateRequestResponse>> GetByIdAsync(
        Guid id,
        string currentUserId,
        bool isCitizen,
        bool isAdministrator,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CertificateRequestResponse>> CreateAsync(
        CreateCertificateRequestRequest request,
        string requestedByUserId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CertificateRequestResponse>> CancelAsync(
        Guid id,
        string currentUserId,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CertificateRequestResponse>> RejectAsync(
        Guid id,
        RejectCertificateRequestRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<BirthRecordResponse>>> GetMatchingBirthRecordsAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<CertificateRequestResponse>> LinkBirthRecordAsync(
        Guid id,
        Guid birthRecordId,
        CancellationToken cancellationToken = default);
}
