namespace SGCN.Application.DTOs.Referentials.Hospital;

public sealed record CreateHospitalRequest(
    string Name,
    string? Code,
    Guid CommuneId,
    string? Address);
