namespace SGCN.Application.DTOs.Referentials.Department;

public sealed record UpdateDepartmentRequest(
    string? Name,
    string? Code);
