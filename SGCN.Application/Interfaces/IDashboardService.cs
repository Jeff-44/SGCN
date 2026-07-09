using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Dashboard;

namespace SGCN.Application.Interfaces;

public interface IDashboardService
{
    Task<ApiResponse<DashboardResponse>> GetAsync(
        string currentUserId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default);
}
