using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Users;
using SGCN.Application.Interfaces;
using SGCN.Domain.Constants;
using SGCN.Domain.Identity;

namespace SGCN.Infrastructure.Services;

public sealed class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<ApiResponse<UserResponse>> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FullName))
        {
            return Fail("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Fail("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return Fail("Username is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Role))
        {
            return Fail("Role is required.");
        }

        if (!IsAllowedRole(request.Role))
        {
            return Fail("Role must be one of Administrateur, AgentEtatCivil, or Medecin.");
        }

        var existingEmailUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingEmailUser is not null)
        {
            return Fail("A user with this email already exists.");
        }

        var existingUserNameUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUserNameUser is not null)
        {
            return Fail("A user with this username already exists.");
        }

        // TODO: replace this temporary password flow with email delivery later.
        var temporaryPassword = GenerateTemporaryPassword();
        var user = new ApplicationUser
        {
            UserName = request.UserName,
            Email = request.Email,
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            NifOrCin = request.NifOrCin,
            IsActive = true,
            ForcePasswordChange = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        var createResult = await _userManager.CreateAsync(user, temporaryPassword);
        if (!createResult.Succeeded)
        {
            return Fail(
                "User creation failed.",
                createResult.Errors.Select(error => error.Description).ToArray());
        }

        var normalizedRole = NormalizeRole(request.Role);
        var roleResult = await _userManager.AddToRoleAsync(user, normalizedRole);
        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Fail(
                "User creation failed.",
                roleResult.Errors.Select(error => error.Description).ToArray());
        }

        var response = await MapToResponseAsync(user, cancellationToken);
        response = response with { TemporaryPassword = temporaryPassword };

        return ApiResponse<UserResponse>.Ok(response, "User created successfully.");
    }

    public async Task<ApiResponse<IReadOnlyCollection<UserResponse>>> GetAllAsync(string? search, string? role, bool? isActive, CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);

        var filteredUsers = users.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            filteredUsers = filteredUsers.Where(user =>
                (user.FullName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                || (user.Email?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                || (user.UserName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false)
                || (user.NifOrCin?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        if (isActive.HasValue)
        {
            filteredUsers = filteredUsers.Where(user => user.IsActive == isActive.Value);
        }

        var userResponses = new List<UserResponse>();
        foreach (var user in filteredUsers)
        {
            if (!string.IsNullOrWhiteSpace(role))
            {
                var normalizedRole = NormalizeRole(role);
                var userRoles = await _userManager.GetRolesAsync(user);
                if (!userRoles.Any(existingRole => string.Equals(existingRole, normalizedRole, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }
            }

            userResponses.Add(await MapToResponseAsync(user, cancellationToken));
        }

        return ApiResponse<IReadOnlyCollection<UserResponse>>.Ok(userResponses.OrderByDescending(user => user.CreatedAt).ToList(), "Users retrieved successfully.");
    }

    public async Task<ApiResponse<UserResponse>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return Fail("User not found.");
        }

        return ApiResponse<UserResponse>.Ok(await MapToResponseAsync(user, cancellationToken), "User retrieved successfully.");
    }

    public async Task<ApiResponse<UserResponse>> UpdateAsync(string id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return Fail("User not found.");
        }

        user.FullName = request.FullName ?? user.FullName;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.NifOrCin = request.NifOrCin ?? user.NifOrCin;
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Fail(
                "User update failed.",
                updateResult.Errors.Select(error => error.Description).ToArray());
        }

        if (request.Role is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Role))
            {
                return Fail("Role cannot be empty.");
            }

            if (!IsAllowedRole(request.Role))
            {
                return Fail("Role must be one of Administrateur, AgentEtatCivil, or Medecin.");
            }

            var normalizedRole = NormalizeRole(request.Role);
            if (!await _roleManager.RoleExistsAsync(normalizedRole))
            {
                return Fail("Role does not exist.");
            }

            var existingRoles = await _userManager.GetRolesAsync(user);
            if (!existingRoles.Any(role => string.Equals(role, normalizedRole, StringComparison.OrdinalIgnoreCase))
                || existingRoles.Count > 1)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, existingRoles);
                if (!removeResult.Succeeded)
                {
                    return Fail(
                        "Role update failed.",
                        removeResult.Errors.Select(error => error.Description).ToArray());
                }

                var addResult = await _userManager.AddToRoleAsync(user, normalizedRole);
                if (!addResult.Succeeded)
                {
                    return Fail(
                        "Role update failed.",
                        addResult.Errors.Select(error => error.Description).ToArray());
                }
            }
        }

        return ApiResponse<UserResponse>.Ok(await MapToResponseAsync(user, cancellationToken), "User updated successfully.");
    }

    public async Task<ApiResponse<UserResponse>> ActivateAsync(string id, CancellationToken cancellationToken = default)
    {
        return await SetActiveStatusAsync(id, true, cancellationToken);
    }

    public async Task<ApiResponse<UserResponse>> DeactivateAsync(string id, CancellationToken cancellationToken = default)
    {
        return await SetActiveStatusAsync(id, false, cancellationToken);
    }

    public async Task<ApiResponse<UserResponse>> GetProfileAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Fail("User not found.");
        }

        return ApiResponse<UserResponse>.Ok(await MapToResponseAsync(user, cancellationToken), "Profile retrieved successfully.");
    }

    public async Task<ApiResponse<UserResponse>> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Fail("User not found.");
        }

        user.FullName = request.FullName ?? user.FullName;
        user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;
        user.NifOrCin = request.NifOrCin ?? user.NifOrCin;
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Fail(
                "Profile update failed.",
                updateResult.Errors.Select(error => error.Description).ToArray());
        }

        return ApiResponse<UserResponse>.Ok(await MapToResponseAsync(user, cancellationToken), "Profile updated successfully.");
    }

    private async Task<ApiResponse<UserResponse>> SetActiveStatusAsync(string id, bool isActive, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null)
        {
            return Fail("User not found.");
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;

        // TODO: send account status email notification later.
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Fail(
                "User status update failed.",
                updateResult.Errors.Select(error => error.Description).ToArray());
        }

        return ApiResponse<UserResponse>.Ok(await MapToResponseAsync(user, cancellationToken), isActive ? "User activated successfully." : "User deactivated successfully.");
    }

    private async Task<UserResponse> MapToResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return new UserResponse(
            user.Id,
            user.FullName,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.PhoneNumber,
            user.NifOrCin,
            user.IsActive,
            user.ForcePasswordChange,
            roles.ToArray(),
            user.CreatedAt,
            user.UpdatedAt);
    }

    private static bool IsAllowedRole(string role)
    {
        return string.Equals(role, SystemRoles.Administrateur, StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, SystemRoles.AgentEtatCivil, StringComparison.OrdinalIgnoreCase)
            || string.Equals(role, SystemRoles.Medecin, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeRole(string role)
    {
        if (string.Equals(role, SystemRoles.Administrateur, StringComparison.OrdinalIgnoreCase))
        {
            return SystemRoles.Administrateur;
        }

        if (string.Equals(role, SystemRoles.AgentEtatCivil, StringComparison.OrdinalIgnoreCase))
        {
            return SystemRoles.AgentEtatCivil;
        }

        if (string.Equals(role, SystemRoles.Medecin, StringComparison.OrdinalIgnoreCase))
        {
            return SystemRoles.Medecin;
        }

        return role.Trim();
    }

    private static string GenerateTemporaryPassword()
    {
        const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowercase = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "@#$%!";
        const string all = uppercase + lowercase + digits + symbols;

        var password = new List<char>
        {
            uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)],
            lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)],
            digits[RandomNumberGenerator.GetInt32(digits.Length)],
            symbols[RandomNumberGenerator.GetInt32(symbols.Length)]
        };

        while (password.Count < 14)
        {
            password.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
        }

        return new string(password
            .OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue))
            .ToArray());
    }

    private static ApiResponse<UserResponse> Fail(string message, IReadOnlyCollection<string>? errors = null)
    {
        return ApiResponse<UserResponse>.Fail(message, errors);
    }

    private static ApiResponse<IReadOnlyCollection<UserResponse>> FailUsers(string message, IReadOnlyCollection<string>? errors = null)
    {
        return ApiResponse<IReadOnlyCollection<UserResponse>>.Fail(message, errors);
    }
}
