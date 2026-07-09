using Microsoft.EntityFrameworkCore;
using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Referentials.Department;
using SGCN.Application.Interfaces;
using SGCN.Domain.Entities;
using SGCN.Infrastructure.Persistence;

namespace SGCN.Infrastructure.Services;

public sealed class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _dbContext;

    public DepartmentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<IReadOnlyCollection<DepartmentResponse>>> GetAllAsync(string? search, bool? isActive, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Departments.AsNoTracking().Where(department => !department.IsDeleted);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(department =>
                department.Name.Contains(term) ||
                (department.Code != null && department.Code.Contains(term)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(department => department.IsActive == isActive.Value);
        }

        var items = await query
            .OrderByDescending(department => department.CreatedAt)
            .Select(department => new DepartmentResponse(
                department.Id,
                department.Name,
                department.Code,
                department.IsActive,
                department.CreatedAt,
                department.UpdatedAt))
            .ToListAsync(cancellationToken);

        return ApiResponse<IReadOnlyCollection<DepartmentResponse>>.Ok(items, "Departments retrieved successfully.");
    }

    public async Task<ApiResponse<DepartmentResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (department is null)
        {
            return Fail("Department not found.");
        }

        return ApiResponse<DepartmentResponse>.Ok(Map(department), "Department retrieved successfully.");
    }

    public async Task<ApiResponse<DepartmentResponse>> CreateAsync(CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Fail("Name is required.");
        }

        var code = NormalizeOptional(request.Code);
        if (code is not null)
        {
            var exists = await _dbContext.Departments.AnyAsync(department => department.Code == code && !department.IsDeleted, cancellationToken);
            if (exists)
            {
                return Fail("A department with this code already exists.");
            }
        }

        var department = new Department
        {
            Name = request.Name.Trim(),
            Code = code,
            IsActive = true
        };

        await _dbContext.Departments.AddAsync(department, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<DepartmentResponse>.Ok(Map(department), "Department created successfully.");
    }

    public async Task<ApiResponse<DepartmentResponse>> UpdateAsync(Guid id, UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (department is null)
        {
            return Fail("Department not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            department.Name = request.Name.Trim();
        }

        if (request.Code is not null)
        {
            var code = NormalizeOptional(request.Code);
            if (code is not null)
            {
                var exists = await _dbContext.Departments.AnyAsync(item => item.Id != department.Id && item.Code == code && !item.IsDeleted, cancellationToken);
                if (exists)
                {
                    return Fail("A department with this code already exists.");
                }
            }

            department.Code = code;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<DepartmentResponse>.Ok(Map(department), "Department updated successfully.");
    }

    public async Task<ApiResponse<DepartmentResponse>> ActivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (department is null)
        {
            return Fail("Department not found.");
        }

        department.IsActive = true;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<DepartmentResponse>.Ok(Map(department), "Department activated successfully.");
    }

    public async Task<ApiResponse<DepartmentResponse>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var department = await _dbContext.Departments.FirstOrDefaultAsync(item => item.Id == id && !item.IsDeleted, cancellationToken);
        if (department is null)
        {
            return Fail("Department not found.");
        }

        department.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<DepartmentResponse>.Ok(Map(department), "Department deactivated successfully.");
    }

    private static DepartmentResponse Map(Department department)
    {
        return new DepartmentResponse(
            department.Id,
            department.Name,
            department.Code,
            department.IsActive,
            department.CreatedAt,
            department.UpdatedAt);
    }

    private static ApiResponse<DepartmentResponse> Fail(string message)
    {
        return ApiResponse<DepartmentResponse>.Fail(message);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
