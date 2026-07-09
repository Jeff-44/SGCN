using SGCN.Domain.Enums;

namespace SGCN.Application.DTOs.BirthRecords;

public sealed record CreateBirthRecordRequest(
    string ChildFirstName,
    string ChildLastName,
    Gender? Gender,
    DateOnly? BirthDate,
    TimeOnly? BirthTime,
    string BirthPlace,
    Guid HospitalId,
    string MotherFullName,
    string? FatherFullName,
    string? HospitalFileNumber);
