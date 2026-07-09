namespace SGCN.Application.DTOs.Auth;

public sealed record RegisterRequest(
    string FullName,
    string Email,
    string Password,
    string? NifOrCin,
    string? PhoneNumber);
