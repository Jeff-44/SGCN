namespace SGCN.Application.DTOs.Users;

public sealed record CreateUserRequest(
    string FullName,
    string Email,
    string UserName,
    string? PhoneNumber,
    string? NifOrCin,
    string Role);
