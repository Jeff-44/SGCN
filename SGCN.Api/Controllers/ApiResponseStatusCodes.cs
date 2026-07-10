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

        if (Contains(response, "not found") || Contains(response, "introuvable"))
        {
            return StatusCodes.Status404NotFound;
        }

        if (Contains(response, "invalid") ||
            Contains(response, "required") ||
            Contains(response, "already exists"))
        {
            return StatusCodes.Status400BadRequest;
        }

        if (Contains(response, "unauthorized") || Contains(response, "identité de l’utilisateur est indisponible"))
        {
            return StatusCodes.Status401Unauthorized;
        }

        if (Contains(response, "forbidden") || Contains(response, "pas autorisé"))
        {
            return StatusCodes.Status403Forbidden;
        }

        if (Contains(response, "annulé"))
        {
            return StatusCodes.Status409Conflict;
        }

        if (Contains(response, "a échoué"))
        {
            return StatusCodes.Status500InternalServerError;
        }

        return StatusCodes.Status400BadRequest;
    }

    private static bool Contains<T>(ApiResponse<T> response, string value)
    {
        return response.Message.Contains(value, StringComparison.OrdinalIgnoreCase);
    }
}
