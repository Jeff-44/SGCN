using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCN.Application.DTOs.Certificates;
using SGCN.Application.DTOs.Common;
using SGCN.Application.Interfaces;
using SGCN.Domain.Constants;

namespace SGCN.Api.Controllers;

[ApiController]
[Route("api/v1/certificates")]
[Authorize]
public sealed class CertificatesController : ControllerBase
{
    private readonly ICertificateService _service;

    public CertificatesController(ICertificateService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = SystemRoles.Citoyen + "," + SystemRoles.AgentEtatCivil + "," + SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CertificateResponse>>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<IReadOnlyCollection<CertificateResponse>>.Fail("User identity is not available."));
        }

        var result = await _service.GetAllAsync(
            currentUserId,
            User.IsInRole(SystemRoles.Citoyen),
            User.IsInRole(SystemRoles.Administrateur),
            cancellationToken);

        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = SystemRoles.Citoyen + "," + SystemRoles.AgentEtatCivil + "," + SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<CertificateResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<CertificateResponse>.Fail("User identity is not available."));
        }

        var result = await _service.GetByIdAsync(
            id,
            currentUserId,
            User.IsInRole(SystemRoles.Citoyen),
            User.IsInRole(SystemRoles.Administrateur),
            cancellationToken);

        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpGet("preview/from-request/{requestId:guid}")]
    [Authorize(Roles = SystemRoles.AgentEtatCivil + "," + SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<CertificatePreviewResponse>>> PreviewFromRequest(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.PreviewFromRequestAsync(requestId, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpGet("preview/from-birth-record/{birthRecordId:guid}")]
    [Authorize(Roles = SystemRoles.AgentEtatCivil + "," + SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<CertificatePreviewResponse>>> PreviewFromBirthRecord(
        Guid birthRecordId,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.PreviewFromBirthRecordAsync(birthRecordId, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPost("generate/from-request/{requestId:guid}")]
    [Authorize(Roles = SystemRoles.AgentEtatCivil + "," + SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<CertificateResponse>>> GenerateFromRequest(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<CertificateResponse>.Fail("User identity is not available."));
        }

        var result = await _service.GenerateFromRequestAsync(requestId, currentUserId, cancellationToken);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPost("generate/from-birth-record/{birthRecordId:guid}")]
    [Authorize(Roles = SystemRoles.AgentEtatCivil + "," + SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<CertificateResponse>>> GenerateFromBirthRecord(
        Guid birthRecordId,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<CertificateResponse>.Fail("User identity is not available."));
        }

        var result = await _service.GenerateFromBirthRecordAsync(birthRecordId, currentUserId, cancellationToken);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}/annul")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<CertificateResponse>>> Annul(
        Guid id,
        [FromBody] AnnulCertificateRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.AnnulAsync(id, request, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
