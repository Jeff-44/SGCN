using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SGCN.Application.Interfaces;
using SGCN.Infrastructure.Persistence;

namespace SGCN.Infrastructure.Services;

public sealed class CertificateNumberGenerator : ICertificateNumberGenerator
{
    private readonly ApplicationDbContext _dbContext;

    public CertificateNumberGenerator(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> GenerateAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture);
        var prefix = $"CERT-{today}-";

        // TODO: Replace this count-based approach with a concurrency-safe sequence before production use.
        var sequence = await _dbContext.Certificates
            .CountAsync(certificate => certificate.CertificateNumber.StartsWith(prefix), cancellationToken) + 1;

        string certificateNumber;
        do
        {
            certificateNumber = $"{prefix}{sequence:0000}";
            sequence++;
        }
        while (await _dbContext.Certificates.AnyAsync(certificate => certificate.CertificateNumber == certificateNumber, cancellationToken));

        return certificateNumber;
    }
}
