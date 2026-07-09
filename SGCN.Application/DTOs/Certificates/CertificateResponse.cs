using SGCN.Domain.Enums;

namespace SGCN.Application.DTOs.Certificates;

public sealed record CertificateResponse(
    Guid Id,
    string CertificateNumber,
    Guid? CertificateRequestId,
    string? RequestNumber,
    Guid BirthRecordId,
    string SgcnId,
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
    string CreatedByUserId,
    string CreatedByFullName,
    CertificateStatus Status,
    string VerificationCode,
    string? PdfPath,
    string? QrCodePath,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? AnnulledAt,
    string? AnnulledReason);
