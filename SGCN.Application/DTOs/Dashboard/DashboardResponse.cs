namespace SGCN.Application.DTOs.Dashboard;

public sealed record DashboardResponse(
    string Role,
    IReadOnlyCollection<DashboardMetricResponse> Metrics,
    IReadOnlyCollection<DashboardChartResponse> Charts,
    string? RecentItemsTitle,
    IReadOnlyCollection<DashboardRecentItemResponse> RecentItems);
