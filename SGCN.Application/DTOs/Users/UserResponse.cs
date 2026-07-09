using System.Text.Json.Serialization;

namespace SGCN.Application.DTOs.Users;

public sealed record UserResponse(
    string Id,
    string FullName,
    string Email,
    string UserName,
    string? PhoneNumber,
    string? NifOrCin,
    bool IsActive,
    bool ForcePasswordChange,
    IReadOnlyCollection<string> Roles,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    string? TemporaryPassword = null);
