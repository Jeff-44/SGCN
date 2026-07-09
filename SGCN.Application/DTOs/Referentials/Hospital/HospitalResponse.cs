namespace SGCN.Application.DTOs.Referentials.Hospital;

public sealed record HospitalResponse(
    Guid Id,
    string Name,
    string? Code,
    Guid CommuneId,
    string CommuneName,
    Guid DepartmentId,
    string DepartmentName,
    string? Address,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
