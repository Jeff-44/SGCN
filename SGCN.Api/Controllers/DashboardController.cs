using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Dashboard;
using SGCN.Application.Interfaces;
using SGCN.Domain.Constants;

namespace SGCN.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
[Authorize(Roles = SystemRoles.Administrateur + "," + SystemRoles.AgentEtatCivil + "," + SystemRoles.Medecin + "," + SystemRoles.Citoyen)]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<DashboardResponse>>> Get(
        CancellationToken cancellationToken = default)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized(ApiResponse<DashboardResponse>.Fail("User identity is not available."));
        }

        var roles = User
            .FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToArray();

        var result = await _dashboardService.GetAsync(currentUserId, roles, cancellationToken);

        return StatusCode(ApiResponseStatusCodes.From(result), result);
    }
}
