namespace SGCN.Application.DTOs.Users;

public sealed record UpdateProfileRequest(
    string? FullName,
    string? PhoneNumber,
    string? NifOrCin);
