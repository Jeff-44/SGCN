namespace SGCN.Application.DTOs.Referentials.Department;

public sealed record CreateDepartmentRequest(
    string Name,
    string? Code);
