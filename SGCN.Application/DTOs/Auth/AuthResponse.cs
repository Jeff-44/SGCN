namespace SGCN.Application.DTOs.Auth;

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    string FullName,
    bool ForcePasswordChange,
    IReadOnlyCollection<string> Roles);
