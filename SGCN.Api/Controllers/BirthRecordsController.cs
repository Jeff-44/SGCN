using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCN.Application.DTOs.BirthRecords;
using SGCN.Application.DTOs.Common;
using SGCN.Application.Interfaces;
using SGCN.Domain.Constants;

namespace SGCN.Api.Controllers;

[ApiController]
[Route("api/v1/birth-records")]
[Authorize]
public sealed class BirthRecordsController : ControllerBase
{
    private readonly IBirthRecordService _service;

    public BirthRecordsController(IBirthRecordService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = SystemRoles.Administrateur + "," + SystemRoles.Medecin)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BirthRecordResponse>>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] Guid? hospitalId,
        [FromQuery] bool? isLocked,
        [FromQuery] bool? isActive,
        [FromQuery] DateOnly? birthDateFrom,
        [FromQuery] DateOnly? birthDateTo,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<IReadOnlyCollection<BirthRecordResponse>>.Fail("User identity is not available."));
        }

        var result = await _service.GetAllAsync(
            currentUserId,
            User.IsInRole(SystemRoles.Administrateur),
            search,
            hospitalId,
            isLocked,
            isActive,
            birthDateFrom,
            birthDateTo,
            cancellationToken);

        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = SystemRoles.Administrateur + "," + SystemRoles.Medecin)]
    public async Task<ActionResult<ApiResponse<BirthRecordResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<BirthRecordResponse>.Fail("User identity is not available."));
        }

        var result = await _service.GetByIdAsync(
            id,
            currentUserId,
            User.IsInRole(SystemRoles.Administrateur),
            cancellationToken);

        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpGet("without-certificate")]
    [Authorize(Roles = SystemRoles.AgentEtatCivil + "," + SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BirthRecordResponse>>>> GetWithoutCertificate(
        [FromQuery] string? search,
        [FromQuery] Guid? hospitalId,
        [FromQuery] DateOnly? birthDateFrom,
        [FromQuery] DateOnly? birthDateTo,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<IReadOnlyCollection<BirthRecordResponse>>.Fail("User identity is not available."));
        }

        var result = await _service.GetWithoutCertificateAsync(
            currentUserId,
            User.IsInRole(SystemRoles.Administrateur),
            search,
            hospitalId,
            birthDateFrom,
            birthDateTo,
            cancellationToken);

        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Medecin)]
    public async Task<ActionResult<ApiResponse<BirthRecordResponse>>> Create(
        [FromBody] CreateBirthRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<BirthRecordResponse>.Fail("User identity is not available."));
        }

        var result = await _service.CreateAsync(request, currentUserId, cancellationToken);

        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = SystemRoles.Medecin)]
    public async Task<ActionResult<ApiResponse<BirthRecordResponse>>> Update(
        Guid id,
        [FromBody] UpdateBirthRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<BirthRecordResponse>.Fail("User identity is not available."));
        }

        var result = await _service.UpdateAsync(
            id,
            request,
            currentUserId,
            User.IsInRole(SystemRoles.Administrateur),
            cancellationToken);

        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<BirthRecordResponse>>> Activate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ActivateAsync(id, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<BirthRecordResponse>>> Deactivate(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.DeactivateAsync(id, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
