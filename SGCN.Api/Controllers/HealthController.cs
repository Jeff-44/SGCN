using Microsoft.AspNetCore.Mvc;
using SGCN.Application.DTOs.Common;

namespace SGCN.Api.Controllers;

[ApiController]
[Route("api/v1/health")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public ActionResult<ApiResponse<string>> Get()
    {
        return Ok(ApiResponse<string>.Ok("Healthy", "SGCN API is running."));
    }
}
