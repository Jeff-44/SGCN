namespace SGCN.Application.DTOs.Referentials.Hospital;

public sealed record UpdateHospitalRequest(
    string? Name,
    string? Code,
    string? Address,
    Guid? CommuneId);
