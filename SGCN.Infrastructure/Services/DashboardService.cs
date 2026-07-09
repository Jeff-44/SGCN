using Microsoft.EntityFrameworkCore;
using SGCN.Application.DTOs.Common;
using SGCN.Application.DTOs.Dashboard;
using SGCN.Application.Interfaces;
using SGCN.Domain.Constants;
using SGCN.Domain.Entities;
using SGCN.Domain.Enums;
using SGCN.Infrastructure.Persistence;

namespace SGCN.Infrastructure.Services;

public sealed class DashboardService : IDashboardService
{
    private const int RecentItemsLimit = 6;

    private readonly ApplicationDbContext _dbContext;

    public DashboardService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiResponse<DashboardResponse>> GetAsync(
        string currentUserId,
        IReadOnlyCollection<string> roles,
        CancellationToken cancellationToken = default)
    {
        var role = ResolveRole(roles);
        if (role is null)
        {
            return ApiResponse<DashboardResponse>.Fail("Forbidden.");
        }

        return role switch
        {
            SystemRoles.Administrateur => await GetAdministratorDashboardAsync(cancellationToken),
            SystemRoles.AgentEtatCivil => await GetCivilRegistryAgentDashboardAsync(cancellationToken),
            SystemRoles.Medecin => await GetDoctorDashboardAsync(currentUserId, cancellationToken),
            SystemRoles.Citoyen => await GetCitizenDashboardAsync(currentUserId, cancellationToken),
            _ => ApiResponse<DashboardResponse>.Fail("Forbidden.")
        };
    }

    private async Task<ApiResponse<DashboardResponse>> GetAdministratorDashboardAsync(
        CancellationToken cancellationToken)
    {
        var totalUsers = await _dbContext.Users.CountAsync(cancellationToken);
        var totalCitizens = await CountUsersInRoleAsync(SystemRoles.Citoyen, cancellationToken);
        var totalDoctors = await CountUsersInRoleAsync(SystemRoles.Medecin, cancellationToken);
        var totalCivilRegistryAgents = await CountUsersInRoleAsync(SystemRoles.AgentEtatCivil, cancellationToken);
        var totalAdministrators = await CountUsersInRoleAsync(SystemRoles.Administrateur, cancellationToken);
        var totalHospitals = await _dbContext.Hospitals.CountAsync(item => !item.IsDeleted, cancellationToken);
        var totalBirthRecords = await BirthRecords().CountAsync(cancellationToken);
        var pendingCertificateRequests = await CertificateRequests()
            .CountAsync(item => item.Status == CertificateRequestStatus.Pending, cancellationToken);
        var approvedCertificateRequests = await CertificateRequests()
            .CountAsync(item => item.Status == CertificateRequestStatus.CertificateCreated, cancellationToken);
        var rejectedCertificateRequests = await CertificateRequests()
            .CountAsync(item => item.Status == CertificateRequestStatus.Rejected, cancellationToken);
        var totalCertificatesIssued = await Certificates().CountAsync(cancellationToken);

        var metrics = new List<DashboardMetricResponse>
        {
            Metric("totalUsers", "Total users", totalUsers),
            Metric("totalCitizens", "Total citizens", totalCitizens),
            Metric("totalDoctors", "Total doctors", totalDoctors),
            Metric("totalCivilRegistryAgents", "Total civil registry agents", totalCivilRegistryAgents),
            Metric("totalHospitals", "Total hospitals", totalHospitals),
            Metric("totalBirthRecords", "Total birth records", totalBirthRecords),
            Metric("pendingCertificateRequests", "Pending certificate requests", pendingCertificateRequests),
            Metric("totalCertificatesIssued", "Total certificates issued", totalCertificatesIssued)
        };

        var charts = new List<DashboardChartResponse>
        {
            Chart(
                "usersByRole",
                "Users by role",
                "bar",
                ChartItem("Citizens", totalCitizens),
                ChartItem("Doctors", totalDoctors),
                ChartItem("Civil registry agents", totalCivilRegistryAgents),
                ChartItem("Administrators", totalAdministrators)),
            Chart(
                "certificateRequestsByStatus",
                "Certificate requests by status",
                "donut",
                ChartItem("Pending", pendingCertificateRequests),
                ChartItem("Approved", approvedCertificateRequests),
                ChartItem("Rejected", rejectedCertificateRequests)),
            Chart(
                "birthRecordsVsCertificates",
                "Birth records vs certificates issued",
                "bar",
                ChartItem("Birth records", totalBirthRecords),
                ChartItem("Certificates issued", totalCertificatesIssued))
        };

        var response = new DashboardResponse(
            SystemRoles.Administrateur,
            metrics,
            charts,
            "Recent activity",
            await GetRecentAdministratorActivityAsync(cancellationToken));

        return ApiResponse<DashboardResponse>.Ok(response, "Dashboard retrieved successfully.");
    }

    private async Task<ApiResponse<DashboardResponse>> GetCivilRegistryAgentDashboardAsync(
        CancellationToken cancellationToken)
    {
        var birthRecordsCreated = await BirthRecords().CountAsync(cancellationToken);
        var pendingCertificateRequests = await CertificateRequests()
            .CountAsync(item => item.Status == CertificateRequestStatus.Pending, cancellationToken);
        var approvedCertificateRequests = await CertificateRequests()
            .CountAsync(item => item.Status == CertificateRequestStatus.CertificateCreated, cancellationToken);
        var rejectedCertificateRequests = await CertificateRequests()
            .CountAsync(item => item.Status == CertificateRequestStatus.Rejected, cancellationToken);
        var certificatesIssued = await Certificates().CountAsync(cancellationToken);

        var metrics = new List<DashboardMetricResponse>
        {
            Metric("birthRecordsCreated", "Birth records created", birthRecordsCreated),
            Metric("pendingCertificateRequests", "Pending certificate requests", pendingCertificateRequests),
            Metric("approvedCertificateRequests", "Approved certificate requests", approvedCertificateRequests),
            Metric("rejectedCertificateRequests", "Rejected certificate requests", rejectedCertificateRequests),
            Metric("certificatesIssued", "Certificates issued", certificatesIssued)
        };

        var charts = new List<DashboardChartResponse>
        {
            Chart(
                "certificateRequestsByStatus",
                "Certificate requests by status",
                "donut",
                ChartItem("Pending", pendingCertificateRequests),
                ChartItem("Approved", approvedCertificateRequests),
                ChartItem("Rejected", rejectedCertificateRequests)),
            Chart(
                "birthRecordsVsCertificates",
                "Birth records created vs certificates issued",
                "bar",
                ChartItem("Birth records created", birthRecordsCreated),
                ChartItem("Certificates issued", certificatesIssued))
        };

        var response = new DashboardResponse(
            SystemRoles.AgentEtatCivil,
            metrics,
            charts,
            "Recent birth records",
            await GetRecentBirthRecordsAsync(BirthRecords(), cancellationToken));

        return ApiResponse<DashboardResponse>.Ok(response, "Dashboard retrieved successfully.");
    }

    private async Task<ApiResponse<DashboardResponse>> GetDoctorDashboardAsync(
        string currentUserId,
        CancellationToken cancellationToken)
    {
        var ownBirthRecords = BirthRecords()
            .Where(record => record.CreatedByUserId == currentUserId);

        var birthRecordsCreated = await ownBirthRecords.CountAsync(cancellationToken);
        var pendingBirthRecords = await ownBirthRecords
            .CountAsync(record => record.IsActive && !record.IsLocked, cancellationToken);
        var validatedBirthRecords = await ownBirthRecords
            .CountAsync(record => record.IsActive && record.IsLocked, cancellationToken);
        var rejectedBirthRecords = await ownBirthRecords
            .CountAsync(record => !record.IsActive, cancellationToken);

        var metrics = new List<DashboardMetricResponse>
        {
            Metric("birthRecordsCreated", "Birth records created", birthRecordsCreated),
            Metric("pendingBirthRecords", "Pending birth records", pendingBirthRecords),
            Metric("validatedBirthRecords", "Validated birth records", validatedBirthRecords),
            Metric("rejectedBirthRecords", "Rejected birth records", rejectedBirthRecords)
        };

        var charts = new List<DashboardChartResponse>
        {
            Chart(
                "birthRecordsByStatus",
                "Birth records by status",
                "donut",
                ChartItem("Pending", pendingBirthRecords),
                ChartItem("Validated", validatedBirthRecords),
                ChartItem("Rejected", rejectedBirthRecords))
        };

        var response = new DashboardResponse(
            SystemRoles.Medecin,
            metrics,
            charts,
            "Recent birth records",
            await GetRecentBirthRecordsAsync(ownBirthRecords, cancellationToken));

        return ApiResponse<DashboardResponse>.Ok(response, "Dashboard retrieved successfully.");
    }

    private async Task<ApiResponse<DashboardResponse>> GetCitizenDashboardAsync(
        string currentUserId,
        CancellationToken cancellationToken)
    {
        var ownRequests = CertificateRequests()
            .Where(request => request.RequestedByUserId == currentUserId);

        var ownCertificates = Certificates()
            .Where(certificate =>
                certificate.CertificateRequest != null &&
                certificate.CertificateRequest.RequestedByUserId == currentUserId);

        var myCertificateRequests = await ownRequests.CountAsync(cancellationToken);
        var pendingRequests = await ownRequests
            .CountAsync(request => request.Status == CertificateRequestStatus.Pending, cancellationToken);
        var approvedRequests = await ownRequests
            .CountAsync(request => request.Status == CertificateRequestStatus.CertificateCreated, cancellationToken);
        var rejectedRequests = await ownRequests
            .CountAsync(request => request.Status == CertificateRequestStatus.Rejected, cancellationToken);
        var myCertificates = await ownCertificates.CountAsync(cancellationToken);
        var availableCertificates = await ownCertificates
            .CountAsync(certificate => certificate.Status == CertificateStatus.Active, cancellationToken);
        var unavailableCertificates = await ownCertificates
            .CountAsync(certificate => certificate.Status != CertificateStatus.Active, cancellationToken);

        var metrics = new List<DashboardMetricResponse>
        {
            Metric("myCertificateRequests", "My certificate requests", myCertificateRequests),
            Metric("pendingRequests", "Pending requests", pendingRequests),
            Metric("approvedRequests", "Approved requests", approvedRequests),
            Metric("rejectedRequests", "Rejected requests", rejectedRequests),
            Metric("myCertificates", "My certificates", myCertificates)
        };

        var charts = new List<DashboardChartResponse>
        {
            Chart(
                "myCertificateRequestsByStatus",
                "My certificate requests by status",
                "donut",
                ChartItem("Pending", pendingRequests),
                ChartItem("Approved", approvedRequests),
                ChartItem("Rejected", rejectedRequests)),
            Chart(
                "myAvailableCertificates",
                "My available certificates",
                "summary",
                ChartItem("Available", availableCertificates),
                ChartItem("Unavailable", unavailableCertificates))
        };

        var response = new DashboardResponse(
            SystemRoles.Citoyen,
            metrics,
            charts,
            null,
            Array.Empty<DashboardRecentItemResponse>());

        return ApiResponse<DashboardResponse>.Ok(response, "Dashboard retrieved successfully.");
    }

    private async Task<IReadOnlyCollection<DashboardRecentItemResponse>> GetRecentAdministratorActivityAsync(
        CancellationToken cancellationToken)
    {
        var recentUsers = await _dbContext.Users
            .AsNoTracking()
            .OrderByDescending(user => user.CreatedAt)
            .Take(RecentItemsLimit)
            .Select(user => new
            {
                user.FullName,
                user.Email,
                user.Id,
                user.IsActive,
                user.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var userItems = recentUsers.Select(user => new DashboardRecentItemResponse(
            "User",
            user.FullName,
            user.Email,
            user.Id,
            user.IsActive ? "Active" : "Inactive",
            user.CreatedAt));

        var requestRows = await CertificateRequests()
            .OrderByDescending(request => request.CreatedAt)
            .Take(RecentItemsLimit)
            .Select(request => new
            {
                request.RequestNumber,
                request.TargetFirstName,
                request.TargetLastName,
                RequestedByFullName = request.RequestedByUser.FullName,
                request.Status,
                request.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var requestItems = requestRows.Select(request => new DashboardRecentItemResponse(
            "Certificate request",
            $"{request.TargetFirstName} {request.TargetLastName}",
            request.RequestedByFullName,
            request.RequestNumber,
            request.Status.ToString(),
            request.CreatedAt));

        var certificateRows = await Certificates()
            .OrderByDescending(certificate => certificate.CreatedAt)
            .Take(RecentItemsLimit)
            .Select(certificate => new
            {
                certificate.CertificateNumber,
                certificate.ChildFirstName,
                certificate.ChildLastName,
                certificate.CreatedByUser.FullName,
                certificate.Status,
                certificate.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var certificateItems = certificateRows.Select(certificate => new DashboardRecentItemResponse(
            "Certificate",
            $"{certificate.ChildFirstName} {certificate.ChildLastName}",
            certificate.FullName,
            certificate.CertificateNumber,
            certificate.Status.ToString(),
            certificate.CreatedAt));

        var birthRecordItems = await GetRecentBirthRecordsAsync(BirthRecords(), cancellationToken);

        return userItems
            .Concat(requestItems)
            .Concat(certificateItems)
            .Concat(birthRecordItems)
            .OrderByDescending(item => item.CreatedAt)
            .Take(RecentItemsLimit)
            .ToList();
    }

    private async Task<IReadOnlyCollection<DashboardRecentItemResponse>> GetRecentBirthRecordsAsync(
        IQueryable<BirthRecord> query,
        CancellationToken cancellationToken)
    {
        var rows = await query
            .AsNoTracking()
            .OrderByDescending(record => record.CreatedAt)
            .Take(RecentItemsLimit)
            .Select(record => new
            {
                record.SgcnId,
                record.ChildFirstName,
                record.ChildLastName,
                record.BirthPlace,
                HospitalName = record.Hospital.Name,
                record.IsActive,
                record.IsLocked,
                record.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(record => new DashboardRecentItemResponse(
                "Birth record",
                $"{record.ChildFirstName} {record.ChildLastName}",
                $"{record.HospitalName} - {record.BirthPlace}",
                record.SgcnId,
                GetBirthRecordStatus(record.IsActive, record.IsLocked),
                record.CreatedAt))
            .ToList();
    }

    private async Task<int> CountUsersInRoleAsync(
        string role,
        CancellationToken cancellationToken)
    {
        var normalizedRole = role.ToUpperInvariant();

        return await (
                from user in _dbContext.Users
                join userRole in _dbContext.UserRoles on user.Id equals userRole.UserId
                join identityRole in _dbContext.Roles on userRole.RoleId equals identityRole.Id
                where identityRole.NormalizedName == normalizedRole
                select user.Id)
            .Distinct()
            .CountAsync(cancellationToken);
    }

    private IQueryable<BirthRecord> BirthRecords()
    {
        return _dbContext.BirthRecords
            .AsNoTracking()
            .Where(record => !record.IsDeleted);
    }

    private IQueryable<CertificateRequest> CertificateRequests()
    {
        return _dbContext.CertificateRequests
            .AsNoTracking()
            .Where(request => !request.IsDeleted);
    }

    private IQueryable<Certificate> Certificates()
    {
        return _dbContext.Certificates
            .AsNoTracking()
            .Where(certificate => !certificate.IsDeleted);
    }

    private static DashboardMetricResponse Metric(string key, string label, int value)
    {
        return new DashboardMetricResponse(key, label, value);
    }

    private static DashboardChartResponse Chart(
        string key,
        string title,
        string type,
        params DashboardChartItemResponse[] items)
    {
        return new DashboardChartResponse(key, title, type, items);
    }

    private static DashboardChartItemResponse ChartItem(string label, int value)
    {
        return new DashboardChartItemResponse(label, value);
    }

    private static string GetBirthRecordStatus(bool isActive, bool isLocked)
    {
        if (!isActive)
        {
            return "Rejected";
        }

        return isLocked ? "Validated" : "Pending";
    }

    private static string? ResolveRole(IReadOnlyCollection<string> roles)
    {
        if (roles.Any(role => string.Equals(role, SystemRoles.Administrateur, StringComparison.OrdinalIgnoreCase)))
        {
            return SystemRoles.Administrateur;
        }

        if (roles.Any(role => string.Equals(role, SystemRoles.AgentEtatCivil, StringComparison.OrdinalIgnoreCase)))
        {
            return SystemRoles.AgentEtatCivil;
        }

        if (roles.Any(role => string.Equals(role, SystemRoles.Medecin, StringComparison.OrdinalIgnoreCase)))
        {
            return SystemRoles.Medecin;
        }

        if (roles.Any(role => string.Equals(role, SystemRoles.Citoyen, StringComparison.OrdinalIgnoreCase)))
        {
            return SystemRoles.Citoyen;
        }

        return null;
    }
}
