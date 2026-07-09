using SGCN.Domain.Enums;

namespace SGCN.Application.DTOs.BirthRecords;

public sealed record UpdateBirthRecordRequest(
    string? ChildFirstName,
    string? ChildLastName,
    Gender? Gender,
    DateOnly? BirthDate,
    TimeOnly? BirthTime,
    string? BirthPlace,
    string? MotherFullName,
    string? FatherFullName,
    string? HospitalFileNumber);
