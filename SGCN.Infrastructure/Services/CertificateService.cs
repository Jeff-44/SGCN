using Microsoft.EntityFrameworkCore;
using SGCN.Application.DTOs.Certificates;
using SGCN.Application.DTOs.Common;
using SGCN.Application.Interfaces;
using SGCN.Domain.Entities;
using SGCN.Domain.Enums;
using SGCN.Infrastructure.Persistence;

namespace SGCN.Infrastructure.Services;

public sealed class CertificateService : ICertificateService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ICertificateNumberGenerator _certificateNumberGenerator;
    private readonly IVerificationCodeGenerator _verificationCodeGenerator;

    public CertificateService(
        ApplicationDbContext dbContext,
        ICertificateNumberGenerator certificateNumberGenerator,
        IVerificationCodeGenerator verificationCodeGenerator)
    {
        _dbContext = dbContext;
        _certificateNumberGenerator = certificateNumberGenerator;
        _verificationCodeGenerator = verificationCodeGenerator;
    }

    public async Task<ApiResponse<IReadOnlyCollection<CertificateResponse>>> GetAllAsync(
        string currentUserId,
        bool isCitizen,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        var query = IncludedCertificates()
            .AsNoTracking()
            .Where(certificate => !certificate.IsDeleted);

        if (isCitizen)
        {
            query = query.Where(certificate =>
                certificate.CertificateRequest != null &&
                certificate.CertificateRequest.RequestedByUserId == currentUserId);
            // TODO: Later improve citizen filtering when certificate has no request.
        }
        else if (!isAdministrator)
        {
            // TODO: Restrict AgentEtatCivil users to certificates related to assigned hospitals when assignments exist.
        }

        var certificates = await query
            .OrderByDescending(certificate => certificate.CreatedAt)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<CertificateResponse>>.Ok(
            certificates.Select(Map).ToList(),
            "Certificates retrieved successfully.");
    }

    public async Task<ApiResponse<CertificateResponse>> GetByIdAsync(
        Guid id,
        string currentUserId,
        bool isCitizen,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        var certificate = await IncludedCertificates()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (certificate is null)
        {
            return Fail("Certificate not found.");
        }

        if (isCitizen &&
            (certificate.CertificateRequest is null ||
             certificate.CertificateRequest.RequestedByUserId != currentUserId))
        {
            return Fail("Forbidden.");
        }

        if (!isCitizen && !isAdministrator)
        {
            // TODO: Restrict AgentEtatCivil users to certificates related to assigned hospitals when assignments exist.
        }

        return ApiResponse<CertificateResponse>.Ok(Map(certificate), "Certificate retrieved successfully.");
    }

    public async Task<ApiResponse<CertificatePreviewResponse>> PreviewFromRequestAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var request = await _dbContext.CertificateRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == requestId && !item.IsDeleted, cancellationToken);

        if (request is null)
        {
            return ApiResponse<CertificatePreviewResponse>.Fail("Certificate request not found.");
        }

        if (!request.LinkedBirthRecordId.HasValue)
        {
            return ApiResponse<CertificatePreviewResponse>.Fail("Certificate request is not linked to a birth record.");
        }

        var birthRecord = await IncludedBirthRecords()
            .AsNoTracking()
            .FirstOrDefaultAsync(record => record.Id == request.LinkedBirthRecordId.Value && !record.IsDeleted, cancellationToken);

        if (birthRecord is null)
        {
            return ApiResponse<CertificatePreviewResponse>.Fail("Birth record not found.");
        }

        var validationError = ValidateBirthRecordCanPreviewCertificate(birthRecord);
        if (validationError is not null)
        {
            return ApiResponse<CertificatePreviewResponse>.Fail(validationError);
        }

        return ApiResponse<CertificatePreviewResponse>.Ok(
            MapPreview(birthRecord, request.RequestNumber),
            "Certificate preview generated successfully.");
    }

    public async Task<ApiResponse<CertificatePreviewResponse>> PreviewFromBirthRecordAsync(
        Guid birthRecordId,
        CancellationToken cancellationToken = default)
    {
        var birthRecord = await IncludedBirthRecords()
            .AsNoTracking()
            .FirstOrDefaultAsync(record => record.Id == birthRecordId && !record.IsDeleted, cancellationToken);

        if (birthRecord is null)
        {
            return ApiResponse<CertificatePreviewResponse>.Fail("Birth record not found.");
        }

        var validationError = ValidateBirthRecordCanPreviewCertificate(birthRecord);
        if (validationError is not null)
        {
            return ApiResponse<CertificatePreviewResponse>.Fail(validationError);
        }

        return ApiResponse<CertificatePreviewResponse>.Ok(
            MapPreview(birthRecord, null),
            "Certificate preview generated successfully.");
    }

    public async Task<ApiResponse<CertificateResponse>> GenerateFromRequestAsync(
        Guid requestId,
        string createdByUserId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var request = await _dbContext.CertificateRequests
            .FirstOrDefaultAsync(item => item.Id == requestId && !item.IsDeleted, cancellationToken);

        if (request is null)
        {
            return Fail("Certificate request not found.");
        }

        if (!request.LinkedBirthRecordId.HasValue)
        {
            return Fail("Certificate request is not linked to a birth record.");
        }

        var requestCertificateExists = await _dbContext.Certificates
            .AnyAsync(certificate => certificate.CertificateRequestId == request.Id && !certificate.IsDeleted, cancellationToken);

        if (requestCertificateExists)
        {
            return Fail("A certificate already exists for this certificate request.");
        }

        var birthRecord = await IncludedBirthRecords()
            .FirstOrDefaultAsync(record => record.Id == request.LinkedBirthRecordId.Value && !record.IsDeleted, cancellationToken);

        if (birthRecord is null)
        {
            return Fail("Birth record not found.");
        }

        var validationError = await ValidateBirthRecordCanCreateCertificateAsync(birthRecord, request.Id, cancellationToken);
        if (validationError is not null)
        {
            return Fail(validationError);
        }

        var createdByUser = await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == createdByUserId, cancellationToken);
        if (createdByUser is null)
        {
            return Fail("Created by user not found.");
        }

        var certificate = await CreateCertificateAsync(birthRecord, request, createdByUser, cancellationToken);

        request.Status = CertificateRequestStatus.CertificateCreated;
        birthRecord.IsLocked = true;

        await _dbContext.Certificates.AddAsync(certificate, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ApiResponse<CertificateResponse>.Ok(Map(certificate), "Certificate generated successfully.");
    }

    public async Task<ApiResponse<CertificateResponse>> GenerateFromBirthRecordAsync(
        Guid birthRecordId,
        string createdByUserId,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var birthRecord = await IncludedBirthRecords()
            .FirstOrDefaultAsync(record => record.Id == birthRecordId && !record.IsDeleted, cancellationToken);

        if (birthRecord is null)
        {
            return Fail("Birth record not found.");
        }

        var validationError = await ValidateBirthRecordCanCreateCertificateAsync(birthRecord, null, cancellationToken);
        if (validationError is not null)
        {
            return Fail(validationError);
        }

        var createdByUser = await _dbContext.Users.FirstOrDefaultAsync(user => user.Id == createdByUserId, cancellationToken);
        if (createdByUser is null)
        {
            return Fail("Created by user not found.");
        }

        var certificate = await CreateCertificateAsync(birthRecord, null, createdByUser, cancellationToken);

        birthRecord.IsLocked = true;

        await _dbContext.Certificates.AddAsync(certificate, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ApiResponse<CertificateResponse>.Ok(Map(certificate), "Certificate generated successfully.");
    }

    public async Task<ApiResponse<CertificateResponse>> AnnulAsync(
        Guid id,
        AnnulCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.AnnulledReason))
        {
            return Fail("AnnulledReason is required.");
        }

        var certificate = await IncludedCertificates()
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (certificate is null)
        {
            return Fail("Certificate not found.");
        }

        certificate.Status = CertificateStatus.Annulled;
        certificate.AnnulledAt = DateTime.UtcNow;
        certificate.AnnulledReason = request.AnnulledReason.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CertificateResponse>.Ok(Map(certificate), "Certificate annulled successfully.");
    }

    private async Task<Certificate> CreateCertificateAsync(
        BirthRecord birthRecord,
        CertificateRequest? request,
        Domain.Identity.ApplicationUser createdByUser,
        CancellationToken cancellationToken)
    {
        // TODO: Generate and store the PDF file for the certificate.
        // TODO: Generate and store the QR code file for public verification.
        // TODO: Notify the requester by email when notification delivery is implemented.
        return new Certificate
        {
            CertificateNumber = await _certificateNumberGenerator.GenerateAsync(cancellationToken),
            CertificateRequestId = request?.Id,
            CertificateRequest = request,
            BirthRecordId = birthRecord.Id,
            BirthRecord = birthRecord,
            ChildFirstName = birthRecord.ChildFirstName,
            ChildLastName = birthRecord.ChildLastName,
            Gender = birthRecord.Gender,
            BirthDate = birthRecord.BirthDate,
            BirthTime = birthRecord.BirthTime,
            BirthPlace = birthRecord.BirthPlace,
            HospitalName = birthRecord.Hospital?.Name ?? string.Empty,
            CommuneName = birthRecord.Hospital?.Commune?.Name ?? string.Empty,
            DepartmentName = birthRecord.Hospital?.Commune?.Department?.Name ?? string.Empty,
            MotherFullName = birthRecord.MotherFullName,
            FatherFullName = birthRecord.FatherFullName,
            CreatedByUserId = createdByUser.Id,
            CreatedByUser = createdByUser,
            Status = CertificateStatus.Active,
            VerificationCode = await _verificationCodeGenerator.GenerateAsync(cancellationToken),
            PdfPath = null,
            QrCodePath = null
        };
    }

    private async Task<string?> ValidateBirthRecordCanCreateCertificateAsync(
        BirthRecord birthRecord,
        Guid? requestId,
        CancellationToken cancellationToken)
    {
        if (!birthRecord.IsActive)
        {
            return "Birth record is inactive.";
        }

        if (birthRecord.IsLocked)
        {
            return "Birth record is locked.";
        }

        var birthRecordCertificateExists = await _dbContext.Certificates
            .AnyAsync(certificate => certificate.BirthRecordId == birthRecord.Id && !certificate.IsDeleted, cancellationToken);

        if (birthRecordCertificateExists)
        {
            return "A certificate already exists for this birth record.";
        }

        if (requestId.HasValue)
        {
            var requestCertificateExists = await _dbContext.Certificates
                .AnyAsync(certificate => certificate.CertificateRequestId == requestId.Value && !certificate.IsDeleted, cancellationToken);

            if (requestCertificateExists)
            {
                return "A certificate already exists for this certificate request.";
            }
        }

        return null;
    }

    private static string? ValidateBirthRecordCanPreviewCertificate(BirthRecord birthRecord)
    {
        if (!birthRecord.IsActive)
        {
            return "Birth record is inactive.";
        }

        return null;
    }

    private IQueryable<Certificate> IncludedCertificates()
    {
        return _dbContext.Certificates
            .Include(certificate => certificate.BirthRecord)
            .Include(certificate => certificate.CertificateRequest)
            .ThenInclude(request => request!.RequestedByUser)
            .Include(certificate => certificate.CreatedByUser);
    }

    private IQueryable<BirthRecord> IncludedBirthRecords()
    {
        return _dbContext.BirthRecords
            .Include(record => record.Hospital)
            .ThenInclude(hospital => hospital.Commune)
            .ThenInclude(commune => commune.Department)
            .Include(record => record.CreatedByUser);
    }

    private static CertificatePreviewResponse MapPreview(BirthRecord record, string? requestNumber)
    {
        return new CertificatePreviewResponse(
            record.ChildFirstName,
            record.ChildLastName,
            record.Gender,
            record.BirthDate,
            record.BirthTime,
            record.BirthPlace,
            record.Hospital?.Name ?? string.Empty,
            record.Hospital?.Commune?.Name ?? string.Empty,
            record.Hospital?.Commune?.Department?.Name ?? string.Empty,
            record.MotherFullName,
            record.FatherFullName,
            record.SgcnId,
            requestNumber);
    }

    private static CertificateResponse Map(Certificate certificate)
    {
        return new CertificateResponse(
            certificate.Id,
            certificate.CertificateNumber,
            certificate.CertificateRequestId,
            certificate.CertificateRequest?.RequestNumber,
            certificate.BirthRecordId,
            certificate.BirthRecord?.SgcnId ?? string.Empty,
            certificate.ChildFirstName,
            certificate.ChildLastName,
            certificate.Gender,
            certificate.BirthDate,
            certificate.BirthTime,
            certificate.BirthPlace,
            certificate.HospitalName,
            certificate.CommuneName,
            certificate.DepartmentName,
            certificate.MotherFullName,
            certificate.FatherFullName,
            certificate.CreatedByUserId,
            certificate.CreatedByUser?.FullName ?? string.Empty,
            certificate.Status,
            certificate.VerificationCode,
            certificate.PdfPath,
            certificate.QrCodePath,
            certificate.CreatedAt,
            certificate.UpdatedAt,
            certificate.AnnulledAt,
            certificate.AnnulledReason);
    }

    private static ApiResponse<CertificateResponse> Fail(string message)
    {
        return ApiResponse<CertificateResponse>.Fail(message);
    }
}
