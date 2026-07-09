using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Users;
using SGCN.Application.Interfaces;
using SGCN.Domain.Constants;

namespace SGCN.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/users")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<UserResponse>>>> GetUsers(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetAllAsync(search, role, isActive, cancellationToken);
        return StatusCode(GetStatusCode(result), result);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUserById(
        string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.GetByIdAsync(id, cancellationToken);
        return StatusCode(GetStatusCode(result), result);
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.CreateAsync(request, cancellationToken);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : StatusCode(GetStatusCode(result), result);
    }

    [HttpPatch("{id}")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(
        string id,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.UpdateAsync(id, request, cancellationToken);
        return StatusCode(GetStatusCode(result), result);
    }

    [HttpPatch("{id}/activate")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> ActivateUser(
        string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.ActivateAsync(id, cancellationToken);
        return StatusCode(GetStatusCode(result), result);
    }

    [HttpPatch("{id}/deactivate")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> DeactivateUser(
        string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _userService.DeactivateAsync(id, cancellationToken);
        return StatusCode(GetStatusCode(result), result);
    }

    [HttpGet("profile")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetProfile(
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<UserResponse>.Fail("User identity is not available."));
        }

        var result = await _userService.GetProfileAsync(userId, cancellationToken);
        return StatusCode(GetStatusCode(result), result);
    }

    [HttpPatch("profile")]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(ApiResponse<UserResponse>.Fail("User identity is not available."));
        }

        var result = await _userService.UpdateProfileAsync(userId, request, cancellationToken);
        return StatusCode(GetStatusCode(result), result);
    }

    private static int GetStatusCode<T>(ApiResponse<T> response)
    {
        return response.Success switch
        {
            false when response.Message.Contains("not found", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status404NotFound,
            false when response.Message.Contains("invalid", StringComparison.OrdinalIgnoreCase) || response.Message.Contains("required", StringComparison.OrdinalIgnoreCase) || response.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status400BadRequest,
            false when response.Message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status401Unauthorized,
            false when response.Message.Contains("forbidden", StringComparison.OrdinalIgnoreCase) => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status200OK
        };
    }
}
