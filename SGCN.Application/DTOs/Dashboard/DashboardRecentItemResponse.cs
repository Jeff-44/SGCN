namespace SGCN.Application.DTOs.Dashboard;

public sealed record DashboardRecentItemResponse(
    string Type,
    string Title,
    string? Description,
    string? Reference,
    string? Status,
    DateTime CreatedAt);
