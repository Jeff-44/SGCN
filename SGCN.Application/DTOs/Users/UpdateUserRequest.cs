namespace SGCN.Application.DTOs.Users;

public sealed record UpdateUserRequest(
    string? FullName,
    string? PhoneNumber,
    string? NifOrCin,
    string? Role);
