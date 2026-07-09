using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Referentials.Commune;
using SGCN.Application.Interfaces;
using SGCN.Domain.Constants;

namespace SGCN.Api.Controllers;

[ApiController]
[Route("api/v1/communes")]
public sealed class CommunesController : ControllerBase
{
    private readonly ICommuneService _service;

    public CommunesController(ICommuneService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<CommuneResponse>>>> GetAll([FromQuery] string? search, [FromQuery] Guid? departmentId, [FromQuery] bool? isActive, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(search, departmentId, isActive, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<CommuneResponse>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<CommuneResponse>>> Create([FromBody] CreateCommuneRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<CommuneResponse>>> Update(Guid id, [FromBody] UpdateCommuneRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<CommuneResponse>>> Activate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.ActivateAsync(id, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<CommuneResponse>>> Deactivate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.DeactivateAsync(id, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }
}
