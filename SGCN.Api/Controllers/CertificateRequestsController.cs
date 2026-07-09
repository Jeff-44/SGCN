using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCN.Application.DTOs.BirthRecords;
using SGCN.Application.DTOs.CertificateRequests;
using SGCN.Application.DTOs.Common;
using SGCN.Application.Interfaces;
using SGCN.Domain.Constants;

namespace SGCN.Api.Controllers;

[ApiController]
[Route("api/v1/certificate-requests")]
[Authorize]
public sealed class CertificateRequestsController : ControllerBase
{
    private readonly ICertificateRequestService _service;

    public CertificateRequestsController(ICertificateRequestService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Citoyen)]
    public async Task<ActionResult<ApiResponse<CertificateRequestResponse>>> Create(
        [FromBody] CreateCertificateRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<CertificateRequestResponse>.Fail("User identity is not available."));
        }

        var result = await _service.CreateAsync(request, currentUserId, cancellationToken);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpGet]
    [Authorize(Roles = SystemRoles.Citoyen + "," + SystemRoles.AgentEtatCivil + "," + SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CertificateRequestResponse>>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<IReadOnlyCollection<CertificateRequestResponse>>.Fail("User identity is not available."));
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
    public async Task<ActionResult<ApiResponse<CertificateRequestResponse>>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<CertificateRequestResponse>.Fail("User identity is not available."));
        }

        var result = await _service.GetByIdAsync(
            id,
            currentUserId,
            User.IsInRole(SystemRoles.Citoyen),
            User.IsInRole(SystemRoles.Administrateur),
            cancellationToken);

        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = SystemRoles.Citoyen)]
    public async Task<ActionResult<ApiResponse<CertificateRequestResponse>>> Cancel(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<CertificateRequestResponse>.Fail("User identity is not available."));
        }

        var result = await _service.CancelAsync(id, currentUserId, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}/reject")]
    [Authorize(Roles = SystemRoles.AgentEtatCivil + "," + SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<CertificateRequestResponse>>> Reject(
        Guid id,
        [FromBody] RejectCertificateRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.RejectAsync(id, request, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpGet("{id:guid}/matching-birth-records")]
    [Authorize(Roles = SystemRoles.AgentEtatCivil + "," + SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<BirthRecordResponse>>>> GetMatchingBirthRecords(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetMatchingBirthRecordsAsync(id, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}/link-birth-record/{birthRecordId:guid}")]
    [Authorize(Roles = SystemRoles.AgentEtatCivil + "," + SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<CertificateRequestResponse>>> LinkBirthRecord(
        Guid id,
        Guid birthRecordId,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.LinkBirthRecordAsync(id, birthRecordId, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
