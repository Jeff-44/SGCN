using SGCN.Domain.Enums;

namespace SGCN.Application.DTOs.CertificateRequests;

public sealed record CertificateRequestResponse(
    Guid Id,
    string RequestNumber,
    string RequestedByUserId,
    string RequestedByFullName,
    string TargetFirstName,
    string TargetLastName,
    Gender TargetGender,
    DateOnly TargetBirthDate,
    string MotherFullName,
    string? FatherFullName,
    string BirthPlace,
    string? HospitalFileNumber,
    string RelationshipToTarget,
    CertificateRequestStatus Status,
    string? RejectionReason,
    Guid? LinkedBirthRecordId,
    string? LinkedBirthRecordSgcnId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
