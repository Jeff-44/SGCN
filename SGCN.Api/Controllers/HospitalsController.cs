using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Referentials.Hospital;
using SGCN.Application.Interfaces;
using SGCN.Domain.Constants;

namespace SGCN.Api.Controllers;

[ApiController]
[Route("api/v1/hospitals")]
public sealed class HospitalsController : ControllerBase
{
    private readonly IHospitalService _service;

    public HospitalsController(IHospitalService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<HospitalResponse>>>> GetAll([FromQuery] string? search, [FromQuery] Guid? communeId, [FromQuery] Guid? departmentId, [FromQuery] bool? isActive, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(search, communeId, departmentId, isActive, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<HospitalResponse>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<HospitalResponse>>> Create([FromBody] CreateHospitalRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<HospitalResponse>>> Update(Guid id, [FromBody] UpdateHospitalRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<HospitalResponse>>> Activate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.ActivateAsync(id, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<HospitalResponse>>> Deactivate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.DeactivateAsync(id, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }
}
