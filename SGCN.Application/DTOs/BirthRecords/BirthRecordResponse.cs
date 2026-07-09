using SGCN.Domain.Enums;

namespace SGCN.Application.DTOs.BirthRecords;

public sealed record BirthRecordResponse(
    Guid Id,
    string SgcnId,
    string ChildFirstName,
    string ChildLastName,
    Gender Gender,
    DateOnly BirthDate,
    TimeOnly BirthTime,
    string BirthPlace,
    Guid HospitalId,
    string HospitalName,
    string CommuneName,
    string DepartmentName,
    string MotherFullName,
    string? FatherFullName,
    string? HospitalFileNumber,
    string CreatedByUserId,
    string CreatedByFullName,
    bool IsLocked,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
