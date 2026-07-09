using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Auth;
using SGCN.Domain.Constants;
using SGCN.Domain.Identity;

namespace SGCN.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("A user with this email already exists."));
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName,
            NifOrCin = request.NifOrCin,
            PhoneNumber = request.PhoneNumber,
            IsActive = true,
            ForcePasswordChange = false
        };

        var createResult = await _userManager.CreateAsync(user, request.Password);

        if (!createResult.Succeeded)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail(
                "Registration failed.",
                createResult.Errors.Select(error => error.Description).ToArray()));
        }

        var roleResult = await _userManager.AddToRoleAsync(user, SystemRoles.Citoyen);

        if (!roleResult.Succeeded)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail(
                "User was created, but role assignment failed.",
                roleResult.Errors.Select(error => error.Description).ToArray()));
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var authResponse = CreateAuthResponse(user, roles);

        return StatusCode(
            StatusCodes.Status201Created,
            ApiResponse<AuthResponse>.Ok(authResponse, "Registration completed successfully."));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("Invalid email or password."));
        }

        if (!user.IsActive)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("This account is inactive."));
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var authResponse = CreateAuthResponse(user, roles);

        return Ok(ApiResponse<AuthResponse>.Ok(authResponse, "Login successful."));
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> ChangePassword(
        ChangePasswordRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("User identity is not available."));
        }

        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("CurrentPassword is required."));
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("NewPassword is required."));
        }

        if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail("New password and confirmation password do not match."));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound(ApiResponse<AuthResponse>.Fail("User not found."));
        }

        if (!user.IsActive)
        {
            return Unauthorized(ApiResponse<AuthResponse>.Fail("This account is inactive."));
        }

        var changeResult = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        if (!changeResult.Succeeded)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail(
                "Password change failed.",
                changeResult.Errors.Select(error => error.Description).ToArray()));
        }

        user.ForcePasswordChange = false;
        user.UpdatedAt = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return BadRequest(ApiResponse<AuthResponse>.Fail(
                "Password was changed, but user status update failed.",
                updateResult.Errors.Select(error => error.Description).ToArray()));
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToArray();
        var authResponse = CreateAuthResponse(user, roles);

        return Ok(ApiResponse<AuthResponse>.Ok(authResponse, "Password changed successfully."));
    }

    [HttpPost("forgot-password")]
    public ActionResult<ApiResponse<string>> ForgotPassword(ForgotPasswordRequest request)
    {
        return Ok(ApiResponse<string>.Ok(
            "Password reset email placeholder.",
            "Email sending is not implemented yet."));
    }

    [HttpPost("reset-password")]
    public ActionResult<ApiResponse<string>> ResetPassword(ResetPasswordRequest request)
    {
        return Ok(ApiResponse<string>.Ok(
            "Password reset placeholder.",
            "Password reset will be connected when email token delivery is implemented."));
    }

    [HttpPost("verify-email")]
    public ActionResult<ApiResponse<string>> VerifyEmail(VerifyEmailRequest request)
    {
        return Ok(ApiResponse<string>.Ok(
            "Email verification placeholder.",
            "Email verification will be connected when email token delivery is implemented."));
    }

    private AuthResponse CreateAuthResponse(ApplicationUser user, IReadOnlyCollection<string> roles)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(GetJwtExpiresInMinutes());

        return new AuthResponse(
            CreateJwtToken(user, roles, expiresAt),
            expiresAt,
            user.Id,
            user.Email ?? string.Empty,
            user.FullName,
            user.ForcePasswordChange,
            roles);
    }

    private string CreateJwtToken(
        ApplicationUser user,
        IReadOnlyCollection<string> roles,
        DateTime expiresAt)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is not configured.");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new("fullName", user.FullName),
            new("forcePasswordChange", user.ForcePasswordChange.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private int GetJwtExpiresInMinutes()
    {
        return _configuration.GetValue("Jwt:ExpiresInMinutes", 60);
    }
}
