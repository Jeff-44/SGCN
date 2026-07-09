using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SGCN.Application.DTOs.BirthRecords;
using SGCN.Application.DTOs.CertificateRequests;
using SGCN.Application.DTOs.Common;
using SGCN.Application.Interfaces;
using SGCN.Domain.Entities;
using SGCN.Domain.Enums;
using SGCN.Infrastructure.Persistence;

namespace SGCN.Infrastructure.Services;

public sealed class CertificateRequestService : ICertificateRequestService
{
    private readonly ApplicationDbContext _dbContext;

    public CertificateRequestService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<IReadOnlyCollection<CertificateRequestResponse>>> GetAllAsync(
        string currentUserId,
        bool isCitizen,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        var query = IncludedCertificateRequests()
            .AsNoTracking()
            .Where(request => !request.IsDeleted);

        if (isCitizen)
        {
            query = query.Where(request => request.RequestedByUserId == currentUserId);
        }
        else if (!isAdministrator)
        {
            // TODO: Restrict AgentEtatCivil users to requests related to assigned hospitals when assignments exist.
        }

        var requests = await query
            .OrderByDescending(request => request.CreatedAt)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<CertificateRequestResponse>>.Ok(
            requests.Select(Map).ToList(),
            "Certificate requests retrieved successfully.");
    }

    public async Task<ApiResponse<CertificateRequestResponse>> GetByIdAsync(
        Guid id,
        string currentUserId,
        bool isCitizen,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        var request = await IncludedCertificateRequests()
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (request is null)
        {
            return Fail("Certificate request not found.");
        }

        if (isCitizen && request.RequestedByUserId != currentUserId)
        {
            return Fail("Forbidden.");
        }

        if (!isCitizen && !isAdministrator)
        {
            // TODO: Restrict AgentEtatCivil users to requests related to assigned hospitals when assignments exist.
        }

        return ApiResponse<CertificateRequestResponse>.Ok(Map(request), "Certificate request retrieved successfully.");
    }

    public async Task<ApiResponse<CertificateRequestResponse>> CreateAsync(
        CreateCertificateRequestRequest request,
        string requestedByUserId,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateCreate(request);
        if (validationError is not null)
        {
            return Fail(validationError);
        }

        var requestedByUser = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == requestedByUserId, cancellationToken);

        if (requestedByUser is null)
        {
            return Fail("Requested by user not found.");
        }

        var certificateRequest = new CertificateRequest
        {
            RequestNumber = await GenerateRequestNumberAsync(cancellationToken),
            RequestedByUserId = requestedByUserId,
            RequestedByUser = requestedByUser,
            TargetFirstName = request.TargetFirstName.Trim(),
            TargetLastName = request.TargetLastName.Trim(),
            TargetGender = request.TargetGender!.Value,
            TargetBirthDate = request.TargetBirthDate!.Value,
            MotherFullName = request.MotherFullName.Trim(),
            FatherFullName = NormalizeOptional(request.FatherFullName),
            BirthPlace = request.BirthPlace.Trim(),
            HospitalFileNumber = NormalizeOptional(request.HospitalFileNumber),
            RelationshipToTarget = request.RelationshipToTarget.Trim(),
            Status = CertificateRequestStatus.Pending
        };

        await _dbContext.CertificateRequests.AddAsync(certificateRequest, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CertificateRequestResponse>.Ok(Map(certificateRequest), "Certificate request created successfully.");
    }

    public async Task<ApiResponse<CertificateRequestResponse>> CancelAsync(
        Guid id,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        var request = await IncludedCertificateRequests()
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (request is null)
        {
            return Fail("Certificate request not found.");
        }

        if (request.RequestedByUserId != currentUserId)
        {
            return Fail("Forbidden.");
        }

        if (request.Status != CertificateRequestStatus.Pending)
        {
            return Fail("Only pending certificate requests can be cancelled.");
        }

        request.Status = CertificateRequestStatus.Cancelled;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CertificateRequestResponse>.Ok(Map(request), "Certificate request cancelled successfully.");
    }

    public async Task<ApiResponse<CertificateRequestResponse>> RejectAsync(
        Guid id,
        RejectCertificateRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.RejectionReason))
        {
            return Fail("RejectionReason is required.");
        }

        var certificateRequest = await IncludedCertificateRequests()
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (certificateRequest is null)
        {
            return Fail("Certificate request not found.");
        }

        if (certificateRequest.Status == CertificateRequestStatus.CertificateCreated)
        {
            return Fail("Certificate request already has a certificate and cannot be rejected.");
        }

        certificateRequest.Status = CertificateRequestStatus.Rejected;
        certificateRequest.RejectionReason = request.RejectionReason.Trim();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CertificateRequestResponse>.Ok(Map(certificateRequest), "Certificate request rejected successfully.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<BirthRecordResponse>>> GetMatchingBirthRecordsAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var request = await _dbContext.CertificateRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (request is null)
        {
            return ApiResponse<IReadOnlyCollection<BirthRecordResponse>>.Fail("Certificate request not found.");
        }

        var query = IncludedBirthRecords()
            .AsNoTracking()
            .Where(record =>
                !record.IsDeleted &&
                record.IsActive &&
                !record.IsLocked &&
                record.BirthDate == request.TargetBirthDate &&
                record.ChildFirstName.ToLower().Contains(request.TargetFirstName.ToLower()) &&
                record.ChildLastName.ToLower().Contains(request.TargetLastName.ToLower()) &&
                record.MotherFullName.ToLower().Contains(request.MotherFullName.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.FatherFullName))
        {
            var fatherName = request.FatherFullName.ToLower();
            query = query.Where(record => record.FatherFullName != null && record.FatherFullName.ToLower().Contains(fatherName));
        }

        if (!string.IsNullOrWhiteSpace(request.HospitalFileNumber))
        {
            var fileNumber = request.HospitalFileNumber.ToLower();
            query = query.Where(record => record.HospitalFileNumber != null && record.HospitalFileNumber.ToLower().Contains(fileNumber));
        }

        // TODO: Add fuzzy matching for names and birth-place variants.
        var birthRecords = await query
            .OrderByDescending(record => record.CreatedAt)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<BirthRecordResponse>>.Ok(
            birthRecords.Select(MapBirthRecord).ToList(),
            "Matching birth records retrieved successfully.");
    }

    public async Task<ApiResponse<CertificateRequestResponse>> LinkBirthRecordAsync(
        Guid id,
        Guid birthRecordId,
        CancellationToken cancellationToken = default)
    {
        var request = await IncludedCertificateRequests()
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (request is null)
        {
            return Fail("Certificate request not found.");
        }

        var birthRecord = await _dbContext.BirthRecords
            .FirstOrDefaultAsync(record => record.Id == birthRecordId && !record.IsDeleted, cancellationToken);

        if (birthRecord is null)
        {
            return Fail("Birth record not found.");
        }

        if (birthRecord.IsLocked)
        {
            return Fail("Birth record is locked.");
        }

        request.LinkedBirthRecordId = birthRecordId;
        request.LinkedBirthRecord = birthRecord;
        request.Status = CertificateRequestStatus.InProgress;
        request.RejectionReason = null;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CertificateRequestResponse>.Ok(Map(request), "Birth record linked successfully.");
    }

    private async Task<string> GenerateRequestNumberAsync(CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var prefix = $"REQ-{today}-";

        // TODO: Replace this count-based approach with a concurrency-safe sequence before production use.
        var sequence = await _dbContext.CertificateRequests
            .CountAsync(request => request.RequestNumber.StartsWith(prefix), cancellationToken) + 1;

        string requestNumber;
        do
        {
            requestNumber = $"{prefix}{sequence:0000}";
            sequence++;
        }
        while (await _dbContext.CertificateRequests.AnyAsync(request => request.RequestNumber == requestNumber, cancellationToken));

        return requestNumber;
    }

    private IQueryable<CertificateRequest> IncludedCertificateRequests()
    {
        return _dbContext.CertificateRequests
            .Include(request => request.RequestedByUser)
            .Include(request => request.LinkedBirthRecord);
    }

    private IQueryable<BirthRecord> IncludedBirthRecords()
    {
        return _dbContext.BirthRecords
            .Include(record => record.Hospital)
            .ThenInclude(hospital => hospital.Commune)
            .ThenInclude(commune => commune.Department)
            .Include(record => record.CreatedByUser);
    }

    private static CertificateRequestResponse Map(CertificateRequest request)
    {
        return new CertificateRequestResponse(
            request.Id,
            request.RequestNumber,
            request.RequestedByUserId,
            request.RequestedByUser?.FullName ?? string.Empty,
            request.TargetFirstName,
            request.TargetLastName,
            request.TargetGender,
            request.TargetBirthDate,
            request.MotherFullName,
            request.FatherFullName,
            request.BirthPlace,
            request.HospitalFileNumber,
            request.RelationshipToTarget,
            request.Status,
            request.RejectionReason,
            request.LinkedBirthRecordId,
            request.LinkedBirthRecord?.SgcnId,
            request.CreatedAt,
            request.UpdatedAt);
    }

    private static BirthRecordResponse MapBirthRecord(BirthRecord record)
    {
        return new BirthRecordResponse(
            record.Id,
            record.SgcnId,
            record.ChildFirstName,
            record.ChildLastName,
            record.Gender,
            record.BirthDate,
            record.BirthTime,
            record.BirthPlace,
            record.HospitalId,
            record.Hospital?.Name ?? string.Empty,
            record.Hospital?.Commune?.Name ?? string.Empty,
            record.Hospital?.Commune?.Department?.Name ?? string.Empty,
            record.MotherFullName,
            record.FatherFullName,
            record.HospitalFileNumber,
            record.CreatedByUserId,
            record.CreatedByUser?.FullName ?? string.Empty,
            record.IsLocked,
            record.IsActive,
            record.CreatedAt,
            record.UpdatedAt);
    }

    private static string? ValidateCreate(CreateCertificateRequestRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TargetFirstName))
        {
            return "TargetFirstName is required.";
        }

        if (string.IsNullOrWhiteSpace(request.TargetLastName))
        {
            return "TargetLastName is required.";
        }

        if (!request.TargetGender.HasValue || !Enum.IsDefined(typeof(Gender), request.TargetGender.Value))
        {
            return "TargetGender is required.";
        }

        if (!request.TargetBirthDate.HasValue)
        {
            return "TargetBirthDate is required.";
        }

        if (string.IsNullOrWhiteSpace(request.MotherFullName))
        {
            return "MotherFullName is required.";
        }

        if (string.IsNullOrWhiteSpace(request.BirthPlace))
        {
            return "BirthPlace is required.";
        }

        if (string.IsNullOrWhiteSpace(request.RelationshipToTarget))
        {
            return "RelationshipToTarget is required.";
        }

        return null;
    }

    private static ApiResponse<CertificateRequestResponse> Fail(string message)
    {
        return ApiResponse<CertificateRequestResponse>.Fail(message);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
