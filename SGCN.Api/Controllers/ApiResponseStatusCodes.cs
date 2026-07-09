using SGCN.Application.DTOs.Common;

namespace SGCN.Api.Controllers;

internal static class ApiResponseStatusCodes
{
    public static int From<T>(ApiResponse<T> response)
    {
        if (response.Success)
        {
            return StatusCodes.Status200OK;
        }

        if (Contains(response, "not found"))
        {
            return StatusCodes.Status404NotFound;
        }

        if (Contains(response, "invalid") ||
            Contains(response, "required") ||
            Contains(response, "already exists"))
        {
            return StatusCodes.Status400BadRequest;
        }

        if (Contains(response, "unauthorized"))
        {
            return StatusCodes.Status401Unauthorized;
        }

        if (Contains(response, "forbidden"))
        {
            return StatusCodes.Status403Forbidden;
        }

        return StatusCodes.Status400BadRequest;
    }

    private static bool Contains<T>(ApiResponse<T> response, string value)
    {
        return response.Message.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}
