namespace SGCN.Application.DTOs.Dashboard;

public sealed record DashboardChartResponse(
    string Key,
    string Title,
    string Type,
    IReadOnlyCollection<DashboardChartItemResponse> Items);
