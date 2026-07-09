using Microsoft.EntityFrameworkCore;
using SGCN.Application.DTOs.BirthRecords;
using SGCN.Application.DTOs.Common;
using SGCN.Application.Interfaces;
using SGCN.Domain.Entities;
using SGCN.Domain.Enums;
using SGCN.Infrastructure.Persistence;

namespace SGCN.Infrastructure.Services;

public sealed class BirthRecordService : IBirthRecordService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ISgcnIdGenerator _sgcnIdGenerator;

    public BirthRecordService(
        ApplicationDbContext dbContext,
        ISgcnIdGenerator sgcnIdGenerator)
    {
        _dbContext = dbContext;
        _sgcnIdGenerator = sgcnIdGenerator;
    }

    public async Task<ApiResponse<IReadOnlyCollection<BirthRecordResponse>>> GetAllAsync(
        string currentUserId,
        bool isAdministrator,
        string? search,
        Guid? hospitalId,
        bool? isLocked,
        bool? isActive,
        DateOnly? birthDateFrom,
        DateOnly? birthDateTo,
        CancellationToken cancellationToken = default)
    {
        var query = IncludedBirthRecords()
            .AsNoTracking()
            .Where(record => !record.IsDeleted);

        if (!isAdministrator)
        {
            // TODO: Restrict Medecin and AgentEtatCivil users to assigned hospitals when hospital assignments exist.
            _ = currentUserId;
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(record =>
                record.SgcnId.Contains(term) ||
                record.ChildFirstName.Contains(term) ||
                record.ChildLastName.Contains(term) ||
                record.MotherFullName.Contains(term) ||
                (record.FatherFullName != null && record.FatherFullName.Contains(term)) ||
                (record.HospitalFileNumber != null && record.HospitalFileNumber.Contains(term)));
        }

        if (hospitalId.HasValue)
        {
            query = query.Where(record => record.HospitalId == hospitalId.Value);
        }

        if (isLocked.HasValue)
        {
            query = query.Where(record => record.IsLocked == isLocked.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(record => record.IsActive == isActive.Value);
        }

        if (birthDateFrom.HasValue)
        {
            query = query.Where(record => record.BirthDate >= birthDateFrom.Value);
        }

        if (birthDateTo.HasValue)
        {
            query = query.Where(record => record.BirthDate <= birthDateTo.Value);
        }

        var records = await query
            .OrderByDescending(record => record.CreatedAt)
            .ToListAsync(cancellationToken);

        var items = records.Select(Map).ToList();

        return ApiResponse<IReadOnlyCollection<BirthRecordResponse>>.Ok(items, "Birth records retrieved successfully.");
    }

    public async Task<ApiResponse<BirthRecordResponse>> GetByIdAsync(
        Guid id,
        string currentUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        var birthRecord = await IncludedBirthRecords()
            .AsNoTracking()
            .FirstOrDefaultAsync(record => record.Id == id && !record.IsDeleted, cancellationToken);

        if (birthRecord is null)
        {
            return Fail("Birth record not found.");
        }

        if (!isAdministrator)
        {
            // TODO: Restrict Medecin and AgentEtatCivil users to assigned hospitals when hospital assignments exist.
            _ = currentUserId;
        }

        return ApiResponse<BirthRecordResponse>.Ok(Map(birthRecord), "Birth record retrieved successfully.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<BirthRecordResponse>>> GetWithoutCertificateAsync(
        string currentUserId,
        bool isAdministrator,
        string? search,
        Guid? hospitalId,
        DateOnly? birthDateFrom,
        DateOnly? birthDateTo,
        CancellationToken cancellationToken = default)
    {
        var query = IncludedBirthRecords()
            .AsNoTracking()
            .Where(record =>
                !record.IsDeleted &&
                record.IsActive &&
                !record.IsLocked &&
                !_dbContext.Certificates.Any(certificate =>
                    certificate.BirthRecordId == record.Id &&
                    !certificate.IsDeleted));

        if (!isAdministrator)
        {
            // TODO: Restrict AgentEtatCivil users to BirthRecords belonging to assigned hospitals when assignments exist.
            _ = currentUserId;
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(record =>
                record.SgcnId.Contains(term) ||
                record.ChildFirstName.Contains(term) ||
                record.ChildLastName.Contains(term) ||
                record.MotherFullName.Contains(term) ||
                (record.FatherFullName != null && record.FatherFullName.Contains(term)) ||
                (record.HospitalFileNumber != null && record.HospitalFileNumber.Contains(term)));
        }

        if (hospitalId.HasValue)
        {
            query = query.Where(record => record.HospitalId == hospitalId.Value);
        }

        if (birthDateFrom.HasValue)
        {
            query = query.Where(record => record.BirthDate >= birthDateFrom.Value);
        }

        if (birthDateTo.HasValue)
        {
            query = query.Where(record => record.BirthDate <= birthDateTo.Value);
        }

        var records = await query
            .OrderByDescending(record => record.CreatedAt)
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<BirthRecordResponse>>.Ok(
            records.Select(Map).ToList(),
            "Birth records without certificates retrieved successfully.");
    }

    public async Task<ApiResponse<BirthRecordResponse>> CreateAsync(
        CreateBirthRecordRequest request,
        string createdByUserId,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateCreate(request);
        if (validationError is not null)
        {
            return Fail(validationError);
        }

        var hospital = await _dbContext.Hospitals
            .Include(item => item.Commune)
            .ThenInclude(item => item.Department)
            .FirstOrDefaultAsync(item => item.Id == request.HospitalId && !item.IsDeleted, cancellationToken);

        if (hospital is null)
        {
            return Fail("Hospital not found.");
        }

        var createdByUser = await _dbContext.Users
            .FirstOrDefaultAsync(user => user.Id == createdByUserId, cancellationToken);

        if (createdByUser is null)
        {
            return Fail("Created by user not found.");
        }

        var birthRecord = new BirthRecord
        {
            SgcnId = await _sgcnIdGenerator.GenerateAsync(cancellationToken),
            ChildFirstName = request.ChildFirstName.Trim(),
            ChildLastName = request.ChildLastName.Trim(),
            Gender = request.Gender!.Value,
            BirthDate = request.BirthDate!.Value,
            BirthTime = request.BirthTime!.Value,
            BirthPlace = request.BirthPlace.Trim(),
            HospitalId = request.HospitalId,
            Hospital = hospital,
            MotherFullName = request.MotherFullName.Trim(),
            FatherFullName = NormalizeOptional(request.FatherFullName),
            HospitalFileNumber = NormalizeOptional(request.HospitalFileNumber),
            CreatedByUserId = createdByUserId,
            CreatedByUser = createdByUser,
            IsLocked = false,
            IsActive = true
        };

        await _dbContext.BirthRecords.AddAsync(birthRecord, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<BirthRecordResponse>.Ok(Map(birthRecord), "Birth record created successfully.");
    }

    public async Task<ApiResponse<BirthRecordResponse>> UpdateAsync(
        Guid id,
        UpdateBirthRecordRequest request,
        string currentUserId,
        bool isAdministrator,
        CancellationToken cancellationToken = default)
    {
        var birthRecord = await IncludedBirthRecords()
            .FirstOrDefaultAsync(record => record.Id == id && !record.IsDeleted, cancellationToken);

        if (birthRecord is null)
        {
            return Fail("Birth record not found.");
        }

        if (birthRecord.IsLocked)
        {
            return Fail("Birth record is locked and cannot be updated.");
        }

        if (!isAdministrator)
        {
            // TODO: Restrict Medecin updates to records from assigned hospitals when hospital assignments exist.
            _ = currentUserId;
        }

        if (request.ChildFirstName is not null)
        {
            if (string.IsNullOrWhiteSpace(request.ChildFirstName))
            {
                return Fail("ChildFirstName cannot be empty.");
            }

            birthRecord.ChildFirstName = request.ChildFirstName.Trim();
        }

        if (request.ChildLastName is not null)
        {
            if (string.IsNullOrWhiteSpace(request.ChildLastName))
            {
                return Fail("ChildLastName cannot be empty.");
            }

            birthRecord.ChildLastName = request.ChildLastName.Trim();
        }

        if (request.Gender.HasValue)
        {
            if (!Enum.IsDefined(typeof(Gender), request.Gender.Value))
            {
                return Fail("Gender is invalid.");
            }

            birthRecord.Gender = request.Gender.Value;
        }

        if (request.BirthDate.HasValue)
        {
            birthRecord.BirthDate = request.BirthDate.Value;
        }

        if (request.BirthTime.HasValue)
        {
            birthRecord.BirthTime = request.BirthTime.Value;
        }

        if (request.BirthPlace is not null)
        {
            if (string.IsNullOrWhiteSpace(request.BirthPlace))
            {
                return Fail("BirthPlace cannot be empty.");
            }

            birthRecord.BirthPlace = request.BirthPlace.Trim();
        }

        if (request.MotherFullName is not null)
        {
            if (string.IsNullOrWhiteSpace(request.MotherFullName))
            {
                return Fail("MotherFullName cannot be empty.");
            }

            birthRecord.MotherFullName = request.MotherFullName.Trim();
        }

        if (request.FatherFullName is not null)
        {
            birthRecord.FatherFullName = NormalizeOptional(request.FatherFullName);
        }

        if (request.HospitalFileNumber is not null)
        {
            birthRecord.HospitalFileNumber = NormalizeOptional(request.HospitalFileNumber);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<BirthRecordResponse>.Ok(Map(birthRecord), "Birth record updated successfully.");
    }

    public async Task<ApiResponse<BirthRecordResponse>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var birthRecord = await IncludedBirthRecords()
            .FirstOrDefaultAsync(record => record.Id == id && !record.IsDeleted, cancellationToken);

        if (birthRecord is null)
        {
            return Fail("Birth record not found.");
        }

        birthRecord.IsActive = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<BirthRecordResponse>.Ok(Map(birthRecord), "Birth record activated successfully.");
    }

    public async Task<ApiResponse<BirthRecordResponse>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var birthRecord = await IncludedBirthRecords()
            .FirstOrDefaultAsync(record => record.Id == id && !record.IsDeleted, cancellationToken);

        if (birthRecord is null)
        {
            return Fail("Birth record not found.");
        }

        birthRecord.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<BirthRecordResponse>.Ok(Map(birthRecord), "Birth record deactivated successfully.");
    }

    private IQueryable<BirthRecord> IncludedBirthRecords()
    {
        return _dbContext.BirthRecords
            .Include(record => record.Hospital)
            .ThenInclude(hospital => hospital.Commune)
            .ThenInclude(commune => commune.Department)
            .Include(record => record.CreatedByUser);
    }

    private static BirthRecordResponse Map(BirthRecord record)
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

    private static string? ValidateCreate(CreateBirthRecordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ChildFirstName))
        {
            return "ChildFirstName is required.";
        }

        if (string.IsNullOrWhiteSpace(request.ChildLastName))
        {
            return "ChildLastName is required.";
        }

        if (!request.Gender.HasValue || !Enum.IsDefined(typeof(Gender), request.Gender.Value))
        {
            return "Gender is required.";
        }

        if (!request.BirthDate.HasValue)
        {
            return "BirthDate is required.";
        }

        if (!request.BirthTime.HasValue)
        {
            return "BirthTime is required.";
        }

        if (string.IsNullOrWhiteSpace(request.BirthPlace))
        {
            return "BirthPlace is required.";
        }

        if (request.HospitalId == Guid.Empty)
        {
            return "HospitalId is required.";
        }

        if (string.IsNullOrWhiteSpace(request.MotherFullName))
        {
            return "MotherFullName is required.";
        }

        return null;
    }

    private static ApiResponse<BirthRecordResponse> Fail(string message)
    {
        return ApiResponse<BirthRecordResponse>.Fail(message);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
