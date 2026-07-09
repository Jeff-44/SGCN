using Microsoft.EntityFrameworkCore;
using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Referentials.Commune;
using SGCN.Application.Interfaces;
using SGCN.Domain.Entities;
using SGCN.Infrastructure.Persistence;

namespace SGCN.Infrastructure.Services;

public sealed class CommuneService : ICommuneService
{
    private readonly ApplicationDbContext _dbContext;

    public CommuneService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<IReadOnlyCollection<CommuneResponse>>> GetAllAsync(string? search, Guid? departmentId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Communes
            .AsNoTracking()
            .Include(commune => commune.Department)
            .Where(commune => !commune.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(commune =>
                commune.Name.Contains(term) ||
                (commune.Code != null && commune.Code.Contains(term)));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(commune => commune.DepartmentId == departmentId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(commune => commune.IsActive == isActive.Value);
        }

        var items = await query
            .OrderByDescending(commune => commune.CreatedAt)
            .Select(commune => new CommuneResponse(
                commune.Id,
                commune.Name,
                commune.Code,
                commune.DepartmentId,
                commune.Department.Name,
                commune.IsActive,
                commune.CreatedAt,
                commune.UpdatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<CommuneResponse>>.Ok(items, "Communes retrieved successfully.");
    }

    public async Task<ApiResponse<CommuneResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var commune = await _dbContext.Communes
            .AsNoTracking()
            .Include(commune => commune.Department)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (commune is null)
        {
            return Fail("Commune not found.");
        }

        return ApiResponse<CommuneResponse>.Ok(Map(commune), "Commune retrieved successfully.");
    }

    public async Task<ApiResponse<CommuneResponse>> CreateAsync(CreateCommuneRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Fail("Name is required.");
        }

        if (request.DepartmentId == Guid.Empty)
        {
            return Fail("DepartmentId is required.");
        }

        var department = await _dbContext.Departments.FirstOrDefaultAsync(item => item.Id == request.DepartmentId && !item.IsDeleted, cancellationToken);
        if (department is null)
        {
            return Fail("Department not found.");
        }

        var code = NormalizeOptional(request.Code);
        if (code is not null)
        {
            var exists = await _dbContext.Communes.AnyAsync(commune => commune.Code == code && !commune.IsDeleted, cancellationToken);
            if (exists)
            {
                return Fail("A commune with this code already exists.");
            }
        }

        var commune = new Commune
        {
            Name = request.Name.Trim(),
            Code = code,
            DepartmentId = request.DepartmentId,
            Department = department,
            IsActive = true
        };

        await _dbContext.Communes.AddAsync(commune, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CommuneResponse>.Ok(Map(commune), "Commune created successfully.");
    }

    public async Task<ApiResponse<CommuneResponse>> UpdateAsync(Guid id, UpdateCommuneRequest request, CancellationToken cancellationToken = default)
    {
        var commune = await _dbContext.Communes
            .Include(item => item.Department)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (commune is null)
        {
            return Fail("Commune not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            commune.Name = request.Name.Trim();
        }

        if (request.Code is not null)
        {
            var code = NormalizeOptional(request.Code);
            if (code is not null)
            {
                var exists = await _dbContext.Communes.AnyAsync(item => item.Id != commune.Id && item.Code == code && !item.IsDeleted, cancellationToken);
                if (exists)
                {
                    return Fail("A commune with this code already exists.");
                }
            }

            commune.Code = code;
        }

        if (request.DepartmentId.HasValue)
        {
            if (request.DepartmentId.Value == Guid.Empty)
            {
                return Fail("DepartmentId is required.");
            }

            var department = await _dbContext.Departments
                .FirstOrDefaultAsync(item => item.Id == request.DepartmentId.Value && !item.IsDeleted, cancellationToken);

            if (department is null)
            {
                return Fail("Department not found.");
            }

            commune.DepartmentId = department.Id;
            commune.Department = department;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CommuneResponse>.Ok(Map(commune), "Commune updated successfully.");
    }

    public async Task<ApiResponse<CommuneResponse>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var commune = await _dbContext.Communes
            .Include(item => item.Department)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (commune is null)
        {
            return Fail("Commune not found.");
        }

        commune.IsActive = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CommuneResponse>.Ok(Map(commune), "Commune activated successfully.");
    }

    public async Task<ApiResponse<CommuneResponse>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var commune = await _dbContext.Communes
            .Include(item => item.Department)
            .FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);

        if (commune is null)
        {
            return Fail("Commune not found.");
        }

        commune.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<CommuneResponse>.Ok(Map(commune), "Commune deactivated successfully.");
    }

    private static CommuneResponse Map(Commune commune)
    {
        return new CommuneResponse(
            commune.Id,
            commune.Name,
            commune.Code,
            commune.DepartmentId,
            commune.Department?.Name ?? string.Empty,
            commune.IsActive,
            commune.CreatedAt,
            commune.UpdatedAt);
    }

    private static ApiResponse<CommuneResponse> Fail(string message)
    {
        return ApiResponse<CommuneResponse>.Fail(message);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
