using SGCN.Domain.Enums;

namespace SGCN.Application.DTOs.Certificates;

public sealed record CertificatePreviewResponse(
    string ChildFirstName,
    string ChildLastName,
    Gender Gender,
    DateOnly BirthDate,
    TimeOnly BirthTime,
    string BirthPlace,
    string HospitalName,
    string CommuneName,
    string DepartmentName,
    string MotherFullName,
    string? FatherFullName,
    string SgcnId,
    string? RequestNumber);
