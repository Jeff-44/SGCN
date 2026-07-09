namespace SGCN.Application.DTOs.Referentials.Commune;

public sealed record UpdateCommuneRequest(
    string? Name,
    string? Code,
    Guid? DepartmentId);
