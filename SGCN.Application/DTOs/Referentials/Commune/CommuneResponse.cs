namespace SGCN.Application.DTOs.Referentials.Commune;

public sealed record CommuneResponse(
    Guid Id,
    string Name,
    string? Code,
    Guid DepartmentId,
    string DepartmentName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
