using SGCN.Domain.Enums;

namespace SGCN.Application.DTOs.CertificateRequests;

public sealed record CreateCertificateRequestRequest(
    string TargetFirstName,
    string TargetLastName,
    Gender? TargetGender,
    DateOnly? TargetBirthDate,
    string MotherFullName,
    string? FatherFullName,
    string BirthPlace,
    string? HospitalFileNumber,
    string RelationshipToTarget);
