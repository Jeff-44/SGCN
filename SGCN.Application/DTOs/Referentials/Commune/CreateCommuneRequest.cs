namespace SGCN.Application.DTOs.Referentials.Commune;

public sealed record CreateCommuneRequest(
    string Name,
    string? Code,
    Guid DepartmentId);
