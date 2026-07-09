using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Referentials.Department;
using SGCN.Application.Interfaces;
using SGCN.Domain.Constants;

namespace SGCN.Api.Controllers;

[ApiController]
[Route("api/v1/departments")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<DepartmentResponse>>>> GetAll([FromQuery] string? search, [FromQuery] bool? isActive, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(search, isActive, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<DepartmentResponse>>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<DepartmentResponse>>> Create([FromBody] CreateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return result.Success
            ? StatusCode(StatusCodes.Status201Created, result)
            : StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<DepartmentResponse>>> Update(Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}/activate")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<DepartmentResponse>>> Activate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.ActivateAsync(id, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }

    [HttpPatch("{id:guid}/deactivate")]
    [Authorize(Roles = SystemRoles.Administrateur)]
    public async Task<ActionResult<ApiResponse<DepartmentResponse>>> Deactivate(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _service.DeactivateAsync(id, cancellationToken);
        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }
}
