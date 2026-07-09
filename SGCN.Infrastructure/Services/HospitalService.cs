using Microsoft.EntityFrameworkCore;
using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Referentials.Hospital;
using SGCN.Application.Interfaces;
using SGCN.Domain.Entities;
using SGCN.Infrastructure.Persistence;

namespace SGCN.Infrastructure.Services;

public sealed class HospitalService : IHospitalService
{
    private readonly ApplicationDbContext _dbContext;

    public HospitalService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<IReadOnlyCollection<HospitalResponse>>> GetAllAsync(string? search, Guid? communeId, Guid? departmentId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Hospitals
            .AsNoTracking()
            .Include(hospital => hospital.Commune)
            .ThenInclude(commune => commune.Department)
            .Where(hospital => !hospital.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(hospital =>
                hospital.Name.Contains(term) ||
                (hospital.Code != null && hospital.Code.Contains(term)) ||
                (hospital.Address != null && hospital.Address.Contains(term)));
        }

        if (communeId.HasValue)
        {
            query = query.Where(hospital => hospital.CommuneId == communeId.Value);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(hospital => hospital.Commune.DepartmentId == departmentId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(hospital => hospital.IsActive == isActive.Value);
        }

        var items = await query
            .OrderByDescending(hospital => hospital.CreatedAt)
            .Select(hospital => new HospitalResponse(
                hospital.Id,
                hospital.Name,
                hospital.Code,
                hospital.CommuneId,
                hospital.Commune.Name,
                hospital.Commune.DepartmentId,
                hospital.Commune.Department.Name,
                hospital.Address,
                hospital.IsActive,
                hospital.CreatedAt,
                hospital.UpdatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<HospitalResponse>>.Ok(items, "Hospitals retrieved successfully.");
    }

    public async Task<ApiResponse<HospitalResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hospital = await _dbContext.Hospitals
            .AsNoTracking()
            .Include(hospital => hospital.Commune)
            .ThenInclude(commune => commune.Department)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (hospital is null)
        {
            return Fail("Hospital not found.");
        }

        return ApiResponse<HospitalResponse>.Ok(Map(hospital), "Hospital retrieved successfully.");
    }

    public async Task<ApiResponse<HospitalResponse>> CreateAsync(CreateHospitalRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Fail("Name is required.");
        }

        if (request.CommuneId == Guid.Empty)
        {
            return Fail("CommuneId is required.");
        }

        var commune = await _dbContext.Communes
            .Include(item => item.Department)
            .FirstOrDefaultAsync(item => item.Id == request.CommuneId && !item.IsDeleted, cancellationToken);

        if (commune is null)
        {
            return Fail("Commune not found.");
        }

        var code = NormalizeOptional(request.Code);
        if (code is not null)
        {
            var exists = await _dbContext.Hospitals.AnyAsync(hospital => hospital.Code == code && !hospital.IsDeleted, cancellationToken);
            if (exists)
            {
                return Fail("A hospital with this code already exists.");
            }
        }

        var hospital = new Hospital
        {
            Name = request.Name.Trim(),
            Code = code,
            CommuneId = request.CommuneId,
            Commune = commune,
            Address = NormalizeOptional(request.Address),
            IsActive = true
        };

        await _dbContext.Hospitals.AddAsync(hospital, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<HospitalResponse>.Ok(Map(hospital), "Hospital created successfully.");
    }

    public async Task<ApiResponse<HospitalResponse>> UpdateAsync(Guid id, UpdateHospitalRequest request, CancellationToken cancellationToken = default)
    {
        var hospital = await _dbContext.Hospitals
            .Include(item => item.Commune)
            .ThenInclude(item => item.Department)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (hospital is null)
        {
            return Fail("Hospital not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            hospital.Name = request.Name.Trim();
        }

        if (request.Code is not null)
        {
            var code = NormalizeOptional(request.Code);
            if (code is not null)
            {
                var exists = await _dbContext.Hospitals.AnyAsync(item => item.Id != hospital.Id && item.Code == code && !item.IsDeleted, cancellationToken);
                if (exists)
                {
                    return Fail("A hospital with this code already exists.");
                }
            }

            hospital.Code = code;
        }

        if (request.Address is not null)
        {
            hospital.Address = NormalizeOptional(request.Address);
        }

        if (request.CommuneId.HasValue)
        {
            if (request.CommuneId.Value == Guid.Empty)
            {
                return Fail("CommuneId is required.");
            }

            var commune = await _dbContext.Communes
                .Include(item => item.Department)
                .FirstOrDefaultAsync(item => item.Id == request.CommuneId.Value && !item.IsDeleted, cancellationToken);

            if (commune is null)
            {
                return Fail("Commune not found.");
            }

            hospital.CommuneId = commune.Id;
            hospital.Commune = commune;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<HospitalResponse>.Ok(Map(hospital), "Hospital updated successfully.");
    }

    public async Task<ApiResponse<HospitalResponse>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hospital = await _dbContext.Hospitals
            .Include(item => item.Commune)
            .ThenInclude(item => item.Department)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (hospital is null)
        {
            return Fail("Hospital not found.");
        }

        hospital.IsActive = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<HospitalResponse>.Ok(Map(hospital), "Hospital activated successfully.");
    }

    public async Task<ApiResponse<HospitalResponse>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var hospital = await _dbContext.Hospitals
            .Include(item => item.Commune)
            .ThenInclude(item => item.Department)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (hospital is null)
        {
            return Fail("Hospital not found.");
        }

        hospital.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<HospitalResponse>.Ok(Map(hospital), "Hospital deactivated successfully.");
    }

    private static HospitalResponse Map(Hospital hospital)
    {
        return new HospitalResponse(
            hospital.Id,
            hospital.Name,
            hospital.Code,
            hospital.CommuneId,
            hospital.Commune?.Name ?? string.Empty,
            hospital.Commune?.DepartmentId ?? Guid.Empty,
            hospital.Commune?.Department?.Name ?? string.Empty,
            hospital.Address,
            hospital.IsActive,
            hospital.CreatedAt,
            hospital.UpdatedAt);
    }

    private static ApiResponse<HospitalResponse> Fail(string message)
    {
        return ApiResponse<HospitalResponse>.Fail(message);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
