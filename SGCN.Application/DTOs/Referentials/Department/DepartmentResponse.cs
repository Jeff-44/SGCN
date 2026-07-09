namespace SGCN.Application.DTOs.Referentials.Department;

public sealed record DepartmentResponse(
    Guid Id,
    string Name,
    string? Code,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
