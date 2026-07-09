namespace SGCN.Application.DTOs.Dashboard;

public sealed record DashboardMetricResponse(
    string Key,
    string Label,
    int Value);
